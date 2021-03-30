using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.ComponentModel;
using System.Data;
using System.Threading;

namespace DatabaseActivity
{
    [Designer(typeof(ExecInsertDesigner))]
    public sealed class ExecInsert : AsyncCodeActivity
    {
        #region 属性分类：常见

        public string _displayName;
        [Category("常见")]
        [DisplayName("显示名称")]
        public new string DisplayName
        {
            get
            {
                if (_displayName == null)
                {
                    _displayName = base.DisplayName;
                }
                else
                {
                    base.DisplayName = _displayName;
                }

                return _displayName;
            }
            set
            {
                _displayName = value;
            }
        }
        [Browsable(false)]
        public string _DisplayName
        {
            get
            {
                return this.DisplayName;
            }
        }

        [Category("常见")]
        [DisplayName("出错时继续")]
        [Description("指定即使在当前活动失败的情况下，仍继续执行剩余的活动。仅支持布尔值（True,False）。")]
        public bool ContinueOnError { get; set; }

        [Category("常见")]
        [DisplayName("在此之前延迟")]
        [Description("活动开始执行任何操作之前的延迟时间（以毫秒为单位）。默认时间量为200毫秒。")]
        public InArgument<int> DelayBefore { get; set; }

        [Category("常见")]
        [DisplayName("在此之后延迟")]
        [Description("执行活动之后的延迟时间（以毫秒为单位）。默认时间量为300毫秒。")]
        public InArgument<int> DelayAfter { get; set; }

        #endregion


        #region 属性分类：连接配置

        [Category("连接配置")]
        [RequiredArgument]
        [OverloadGroup("ConnectionConfig")]
        [DisplayName("连接字符串")]
        [Description("用于建立数据库连接的连接字符串。必须将文本放入引号中。")]
        public InArgument<string> ConnectionString { get; set; }


        [Category("连接配置")]
        [RequiredArgument]
        [OverloadGroup("ConnectionConfig")]
        [DisplayName("程序名称")]
        [Description("用于访问数据库的数据库提供程序的名称。必须将文本放入引号中。")]
        public InArgument<string> ProviderName { get; set; }

        [Category("连接配置")]
        [RequiredArgument]
        [OverloadGroup("DBConnection")]
        [DisplayName("现有数据库连接")]
        [Description("用于访问数据库的数据库提供程序的名称。")]
        public InArgument<DatabaseConnection> DBConnection { get; set; }

        #endregion


        #region 属性分类：输入

        [Category("输入")]
        [DisplayName("数据表")]
        [Description("插入表中的 DataTable 变量。DataTable 列的名称和描述必须与数据库表中的名称和描述相匹配。")]
        public InArgument<DataTable> DataTable { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("表名")]
        [Description("要在其中插入数据的SQL表。必须将文本放入引号中。")]
        public InArgument<string> TableName { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("执行结果")]
        [Description("将受影响的行数存储到 Int32 变量中。")]
        public OutArgument<Int32> AffectedRecords { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataBase/insert.png";
            }
        }

        #endregion


        DatabaseConnection DbConn;

        [Browsable(false)]
        public string ClassName { get { return "ExecInsert"; } }


        public ExecInsert()
        {
        }

        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return ClassName;
        }



        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            DataTable dataTable = null;
            string connString = null;
            string provName = null;
            string tableName = null;
            try
            {
                DbConn = DBConnection.Get(context);
                connString = ConnectionString.Get(context);
                provName = ProviderName.Get(context);
                tableName = TableName.Get(context);
                dataTable = DataTable.Get(context);

                Func<int> action = () =>
                {
                    DbConn = DbConn ?? new DatabaseConnection().Initialize(connString, provName);
                    if (DbConn == null)
                    {
                        return 0;
                    }

                    return DbConn.InsertDataTable(tableName, dataTable);
                };
                context.UserState = action;

                return action.BeginInvoke(callback, state);
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            m_Delegate = new runDelegate(Run);
            return m_Delegate.BeginInvoke(callback, state);
        }


        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            DatabaseConnection existingConnection = DBConnection.Get(context);
            try
            {

                Func<int> action = (Func<int>)context.UserState;
                int affectedRecords = action.EndInvoke(result);
                this.AffectedRecords.Set(context, affectedRecords);
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, "", e.Message);
            }
            finally
            {
                if (existingConnection == null)
                {
                    DbConn.Dispose();
                }

                int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
                Thread.Sleep(delayAfter);

            }
        }
    }
}