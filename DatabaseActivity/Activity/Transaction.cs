using System;
using System.Activities;
using System.ComponentModel;
using System.Activities.Statements;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;

namespace DatabaseActivity
{
    [Designer(typeof(TransactionDesigner))]
    public class Transaction : NativeActivity
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


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("事务")]
        [Description("指定此活动中的数据库操作是否应包装在数据库事务中。")]
        public bool UseTransaction { get; set; }

        #endregion


        #region 属性分类：连接配置

        [Category("连接配置")]
        [RequiredArgument]
        [OverloadGroup("新建连接")]
        [DisplayName("连接字符串")]
        [Description("用于建立数据库连接的连接字符串。必须将文本放入引号中。")]
        public InArgument<string> ConnectionString { get; set; }

        [Category("连接配置")]
        [RequiredArgument]
        [OverloadGroup("新建连接")]
        [DisplayName("程序名称")]
        [Description("用于访问数据库的数据库提供程序的名称。必须将文本放入引号中。")]
        public InArgument<string> ProviderName { get; set; }

        [Category("连接配置")]
        [RequiredArgument]
        [OverloadGroup("现有连接")]
        [DisplayName("现有数据库连接")]
        [Description("用于访问数据库的数据库提供程序的名称。")]
        public InArgument<DatabaseConnection> DBConnection { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("数据库连接")]
        [Description("用于保存数据库连接的变量。仅支持 DatabaseConnection 变量。")]
        public OutArgument<DatabaseConnection> DBCONN { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public ActivityAction Body { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataBase/transaction.png";
            }
        }

        #endregion


        public static string OpenBrowsersPropertyTag { get { return "Transaction"; } }

        public Transaction()
        {
            Body = new ActivityAction
            {
                Handler = new Sequence()
                {
                    DisplayName = "Do"
                }
            };
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }


        protected override void Execute(NativeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);
            try
            {
                var connString = ConnectionString.Get(context);
                var provName = ProviderName.Get(context);
                var dbConnection = DBConnection.Get(context) ?? new DatabaseConnection().Initialize(connString, provName);


                if (dbConnection == null) return;
                if (UseTransaction)
                {
                    dbConnection.BeginTransaction();
                }
                DBCONN.Set(context, dbConnection);

                if (Body != null)
                {
                    context.ScheduleAction(Body, OnCompletedCallback, OnFaultedCallback);
                }
            }
           
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            Thread.Sleep(delayAfter);
        }

        private void OnCompletedCallback(NativeActivityContext context, ActivityInstance activityInstance)
        {
            DatabaseConnection conn = null;
            try
            {
                conn = DBConnection.Get(context);
                if (UseTransaction)
                {
                    conn.Commit();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, ContinueOnError);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Dispose();
                }
            }
        }

        private void OnFaultedCallback(NativeActivityFaultContext faultContext, Exception exception, ActivityInstance source)
        {
            faultContext.CancelChildren();
            DatabaseConnection conn = DBConnection.Get(faultContext);
            if (conn != null)
            {
                try
                {
                    if (UseTransaction)
                    {
                        conn.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Dispose();
                }
            }

            HandleException(exception, ContinueOnError);
            faultContext.HandleFault();
        }

        private void HandleException(Exception ex, bool continueOnError)
        {
            if (continueOnError) return;
            throw ex;
        }


        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            //TODO
        }
        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            //TODO
        }
    }
}
