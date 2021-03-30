using System;
using System.Activities;
using System.ComponentModel;
using Plugins.Shared.Library;
using System.Data.SqlClient;
using System.Threading;
using MouseActivity;

namespace DatabaseActivity
{
    [Designer(typeof(ConnectDesigner))]
    public sealed class Connect : AsyncCodeActivity
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

        //[Category("常见")]
        //[DisplayName("出错时继续")]
        //[Description("指定即使在当前活动失败的情况下，仍继续执行剩余的活动。仅支持布尔值（True,False）。")]
        //public bool ContinueOnError { get; set; }

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
        [DisplayName("连接字符串")]
        [Description("用于建立数据库连接的连接字符串。必须将文本放入引号中。")]
        public InArgument<string> ConnectionString { get; set; }

        [Category("连接配置")]
        [RequiredArgument]
        [DisplayName("程序名称")]
        [Description("用于访问数据库的数据库提供程序的名称。必须将文本放入引号中。")]
        public InArgument<string> ProviderName { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("数据库连接")]
        [Description("用于保存数据库连接的变量，仅支持 DatabaseConnection。")]
        public OutArgument<DatabaseConnection> DatabaseConnection { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataBase/connect.png";
            }
        }

        #endregion


        private void onComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            string connStr = ConnectionString.Get(context);
            string providerName = ProviderName.Get(context);
            SqlConnection sqlConn = new SqlConnection();
            try
            {
                IDBConnectionFactory _connectionFactory = new DBConnectionFactory();
                Func<DatabaseConnection> action = () => _connectionFactory.Create(connStr, providerName);
                context.UserState = action;

                return action.BeginInvoke(callback, state);
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                //TODO like goforwardActivity
                throw;
            }

        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            Thread.Sleep(delayAfter);

            Func<DatabaseConnection> action = (Func<DatabaseConnection>)context.UserState;
            var dbConn = action.EndInvoke(result);
            DatabaseConnection.Set(context, dbConn);
        }
    }
}