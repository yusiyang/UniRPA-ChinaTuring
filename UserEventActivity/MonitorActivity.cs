using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;

namespace UserEventActivity
{
    [Designer(typeof(MonitorActivityDesigner))]
    public class MonitorActivity : NativeActivity
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
        [DisplayName("一直重复")]
        [Description("该字段仅支持布尔值（True、False）。True - 每次激活触发器时都执行程序块；此为默认值。False - 该活动仅执行一次。")]
        public Activity<bool> RepeatForever { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/UserEvent/monitor.png";
            }
        }

        [Browsable(false)]
        public ActivityAction<object> Handler { get; set; }

        [Browsable(false)]
        public List<Activity> Triggers { get; private set; }

        #endregion


        private Bookmark _monitorBookmark;
        private BookmarkResumptionHelper _bookmarkResumptionHelper;
        protected Queue<KeyValuePair<ActivityAction<object>, object>> EventQueue = new Queue<KeyValuePair<ActivityAction<object>, object>>();

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        public MonitorActivity()
        {
            this.Triggers = new List<Activity>();

            this.Handler = new ActivityAction<object>
            {
                Handler = new Sequence
                {
                    DisplayName = "EventHandler"
                },
                Argument = new DelegateInArgument<object>("args")
            };
            this.RepeatForever = true;
        }


        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddChild(this.RepeatForever);
            foreach (Activity current in this.Triggers)
            {
                metadata.AddChild(current);
            }
            metadata.RequireExtension<BookmarkResumptionHelper>();
            base.CacheMetadata(metadata);
            metadata.AddDefaultExtensionProvider<BookmarkResumptionHelper>(() => new BookmarkResumptionHelper());
        }

        protected override void Execute(NativeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                this.EventQueue.Clear();
                this._monitorBookmark = context.CreateBookmark(new BookmarkCallback(this.OnMonitorTrigger), BookmarkOptions.MultipleResume);
                this._bookmarkResumptionHelper = context.GetExtension<BookmarkResumptionHelper>();
                context.Properties.Add("MonitorBookmark", this._monitorBookmark, true);
                this.StartMonitor(context);
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                this.DisposeMonitor(context);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            Thread.Sleep(delayAfter);
        }

        protected void StartMonitor(NativeActivityContext context)
        {
            foreach (Activity current in this.Triggers)
            {
                context.ScheduleActivity(current);
            }
        }

        protected override void Cancel(NativeActivityContext context)
        {
            this.DisposeMonitor(context);
            base.Cancel(context);
        }
        private void OnMonitorTrigger(NativeActivityContext context, Bookmark bookmark, object value)
        {
            this.EventQueue.Enqueue(new KeyValuePair<ActivityAction<object>, object>(this.Handler, value));
            if (this.EventQueue.Count == 1)
            {
                this.ExecuteEventHandler(context);
            }
        }
        protected void ExecuteEventHandler(NativeActivityContext context)
        {
            KeyValuePair<ActivityAction<object>, object> keyValuePair = this.EventQueue.Peek();
            if (keyValuePair.Key != null)
            {
                context.ScheduleAction<object>(keyValuePair.Key, keyValuePair.Value, new CompletionCallback(this.BodyCompleted), new FaultCallback(this.BodyFaulted));
            }
        }
        protected virtual void BodyCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            // EventInfo eventInfo = this.EventQueue.Peek().Value as EventInfo;
            //if (eventInfo != null && eventInfo.ReplayEvent)
            //{
            //    eventInfo.Replay();
            //}
            if (this.RepeatForever == null)
            {
                this.RepeatForever = false;
            }
            context.ScheduleActivity<bool>(this.RepeatForever, new CompletionCallback<bool>(this.RepeatForeverCompleted), null);
        }
        protected void RepeatForeverCompleted(NativeActivityContext context, ActivityInstance completedInstance, bool result)
        {
            if (!result)
            {
                this.DisposeMonitor(context);
                return;
            }
            this.EventQueue.Dequeue();
            if (this.EventQueue.Count > 0)
            {
                this.ExecuteEventHandler(context);
            }
        }
        protected void BodyFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            this.DisposeMonitor(faultContext);
            this.HandleException(propagatedException, this.ContinueOnError);
            faultContext.CancelChildren();
            faultContext.HandleFault();
        }
        protected void DisposeMonitor(NativeActivityContext context)
        {
            context.RemoveBookmark(this._monitorBookmark);
            context.CancelChildren();
        }
        protected void HandleException(Exception ex, bool continueOnError)
        {
            if (continueOnError)
            {
                return;
            }
            throw ex;
        }
    }
}
