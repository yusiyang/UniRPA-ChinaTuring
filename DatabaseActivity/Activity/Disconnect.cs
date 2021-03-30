using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library;

namespace DatabaseActivity
{
    [Designer(typeof(DisconnectDesigner))]
    public sealed class Disconnect : AsyncCodeActivity
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


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("数据库连接")]
        [Description("要关闭的数据库连接（DatabaseConnection 变量）")]
        public InArgument<DatabaseConnection> DBConn { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataBase/disconnect.png";
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

            try
            {
                var dbConnection = DBConn.Get(context);
                Action action = () => dbConnection.Dispose();
                context.UserState = action;

                return action.BeginInvoke(callback, state);
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                //if (!ContinueOnError)
                //{
                //    throw new ActivityRuntimeException(this.DisplayName, e);
                //}
                throw;
            }
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            Thread.Sleep(delayAfter);
            Action action = (Action)context.UserState;
            action.EndInvoke(result);
        }
    }
}