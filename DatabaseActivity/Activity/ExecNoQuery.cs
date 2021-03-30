using System;
using System.Activities;
using System.ComponentModel;
using System.Collections.Generic;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using Plugins.Shared.Library.Editors;
using System.Data;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;

namespace DatabaseActivity
{
    [Designer(typeof(ExecDesigner))]
    public sealed class ExecNoQuery : AsyncCodeActivity
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

        InArgument<Int32> _OverTime = 10 * 1000;
        [Category("选项")]
        [DisplayName("超时时间")]
        [Description("指定浏览器响应超时时间（毫秒）。")]
        public InArgument<Int32> OverTime
        {
            get
            {
                return _OverTime;
            }
            set
            {
                _OverTime = value;
            }
        }

        CommandType _CommandType = CommandType.Text;
        [Category("选项")]
        [DisplayName("命令类型")]
        [Description("指定如何解释命令字符串。")]
        public CommandType CommandType
        {
            get
            {
                return _CommandType;
            }
            set
            {
                _CommandType = value;
            }
                
        }

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
        [OverloadGroup("DatabaseConnection")]
        [DisplayName("现有数据库连接")]
        [Description("用于访问数据库的数据库提供程序的名称。")]
        public InArgument<DatabaseConnection> DBConnection { get; set; }

        #endregion


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("SQL")]
        [Description("SQL语句。必须将文本放入引号中。")]
        public InArgument<string> SQLString { get; set; }

        private Dictionary<string, Argument> parameters;
        [Category("输入")]
        [DisplayName("参数")]
        public Dictionary<string, Argument> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    this.parameters = new Dictionary<string, Argument>();
                }
                return this.parameters;
            }
            set
            {
                parameters = value;
            }
        }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("执行结果")]
        [Description("对于 UPDATE，INSERT 和 DELETE 语句，返回值是受命令影响的行数。对于所有其他类型的语句，返回值为 -1。")]
        public OutArgument<Int32> AffectedRecords { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataBase/noquery.png";
            }
        }

        #endregion


        DatabaseConnection DBConn;

        public ExecNoQuery()
        {
            var builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(ExecNoQuery), nameof(ExecNoQuery.Parameters), new EditorAttribute(typeof(DictionaryArgumentEditor), typeof(DialogPropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override System.IAsyncResult BeginExecute(AsyncCodeActivityContext context, System.AsyncCallback callback, object state)
        {
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            string connString = null;
            string provName = null;
            string sql = string.Empty;
            int commandTimeout = OverTime.Get(context);
            if (commandTimeout < 0)
            {
                //throw new ArgumentException(UiPath.Database.Activities.Properties.Resources.TimeoutMSException, "TimeoutMS");
            }
            Dictionary<string, Tuple<object, ArgumentDirection>> parameters = null;
            try
            {
                sql = SQLString.Get(context);
                DBConn = DBConnection.Get(context);
                connString = ConnectionString.Get(context);
                provName = ProviderName.Get(context);
                if (Parameters != null)
                {
                    parameters = new Dictionary<string, Tuple<object, ArgumentDirection>>();
                    foreach (var param in Parameters)
                    {
                        parameters.Add(param.Key, new Tuple<object, ArgumentDirection>(param.Value.Get(context), param.Value.Direction));
                    }
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

            // create the action for doing the actual work
            Func<DBExecuteCommandResult> action = () =>
            {
                DBExecuteCommandResult executeResult = new DBExecuteCommandResult();
                if (DBConn == null)
                {
                    DBConn = new DatabaseConnection().Initialize(connString, provName);
                }
                if (DBConn == null)
                {
                    return executeResult;
                }
                executeResult = new DBExecuteCommandResult(DBConn.Execute(sql, parameters, commandTimeout, CommandType), parameters);

                return executeResult;
            };

            context.UserState = action;

            return action.BeginInvoke(callback, state);
        }

        protected override void EndExecute(AsyncCodeActivityContext context, System.IAsyncResult result)
        {
            DatabaseConnection existingConnection = DBConnection.Get(context);
            try
            {
                Func<DBExecuteCommandResult> action = (Func<DBExecuteCommandResult>)context.UserState;
                DBExecuteCommandResult commandResult = action.EndInvoke(result);
                this.AffectedRecords.Set(context, commandResult.Result);
                foreach (var param in commandResult.ParametersBind)
                {
                    var currentParam = Parameters[param.Key];
                    if (currentParam.Direction == ArgumentDirection.Out || currentParam.Direction == ArgumentDirection.InOut)
                    {
                        currentParam.Set(context, param.Value.Item1);
                    }
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

            finally
            {
                if (existingConnection == null)
                {
                    DBConn.Dispose();
                }

                int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
                Thread.Sleep(delayAfter);
            }
        }

        private class DBExecuteCommandResult
        {
            public int Result { get; }
            public Dictionary<string, Tuple<object, ArgumentDirection>> ParametersBind { get; }

            public DBExecuteCommandResult()
            {
                this.Result = 0;
                this.ParametersBind = new Dictionary<string, Tuple<object, ArgumentDirection>>();
            }

            public DBExecuteCommandResult(int result, Dictionary<string, Tuple<object, ArgumentDirection>> parametersBind)
            {
                this.Result = result;
                this.ParametersBind = parametersBind;
            }
        }
    }
}