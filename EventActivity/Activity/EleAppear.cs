using System;
using System.Activities;
using System.ComponentModel;
using System.Activities.Statements;
using System.Windows;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library.UiAutomation;
using System.Diagnostics;
using Plugins.Shared.Library;
using System.Threading.Tasks;
using Plugins.Shared.Library.Exceptions;

namespace EventActivity
{
    [Designer(typeof(EleAppearDesigner))]
    public sealed class EleAppear : NativeActivity
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

        #endregion


        #region 属性分类：目标

        [Category("目标")]
        [OverloadGroup("G1")]
        [DisplayName("元素")]
        [Description("使用另一个活动返回的用户界面元素变量， 该属性不能与“选取器”属性一起使用。该字段仅支持用户界面元素变量。")]
        public InArgument<UiElement> Element { get; set; }

        [Category("目标")]
        [OverloadGroup("G1")]
        [DisplayName("超时（毫秒）")]
        [Description("指定最长等待时间（以毫秒为单位），如果超出该时间活动未运行，系统就会报错。默认值为30000毫秒（30秒）。")]
        public InArgument<int> Timeout { get; set; }

        [Category("目标")]
        [RequiredArgument]
        [OverloadGroup("G1")]
        [DisplayName("选取器")]
        [Description("用于在执行活动时查找特定用户界面元素的“文本”属性。它实际上是XML片段，用于指定您要查找的图形用户界面元素及其一些父元素的属性。必须将文本放入引号中。")]
        public InArgument<string> Selector { get; set; }
        [Browsable(false)]
        public string SelectorOrigin { get; set; }

        #endregion

        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("一直重复")]
        [Description("使您能够长期重复该活动。仅支持布尔值（True、False）。")]
        public bool RepeatForever { get; set; }

        [Category("选项")]
        [DisplayName("等待激活")]
        [Description("选中该选框时，该活动还将等待指定用户界面元素处于活动状态。")]
        public bool WaitActivity { get; set; }

        [Category("选项")]
        [DisplayName("等待可见")]
        [Description("选中该复选框时，该活动将等待指定用户界面元素处于可见状态。")]
        public bool WaitVisible { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("已找到的元素")]
        [Description("所获得的用户界面元素。该字段仅支持用户界面元素变量。")]
        public OutArgument<UiElement> FoundElement { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public ActivityAction<object> Body { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Event/ele-appear.png";
            }
        }

        [Browsable(false)]
        public string SourceImgPath { get; set; }

        [Browsable(false)]
        public System.Windows.Visibility visibility { get; set; } = System.Windows.Visibility.Hidden;

        [Browsable(false)]
        public string DefaultName { get { return "捕获元素显示"; } }

        #endregion


        public EleAppear()
        {
            Body = new ActivityAction<object>
            {
                Argument = new DelegateInArgument<object>(),
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
            try
            {
                int timeout = Common.GetValueOrDefault(context, this.Timeout, 30000);
                var selStr = Selector.Get(context);
                var repeatForever = RepeatForever;
                var stopwatch = new Stopwatch();
                if (timeout >= 0)
                {
                    stopwatch.Start();
                }

                var perMilliseconds = 50;
                var findTimeout = 2000;
                var autoSet = new AutoResetEvent(false);
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;
                Task waitTask = new Task(() =>
                {
                    var flag = false;//可见性
                    while (!flag)  //可见时跳出
                    {
                        lock (this)
                        {
                            UiElement element = Common.GetValueOrDefault(context, this.Element, null);
                            if (element == null && selStr != null)
                            {
                                element = UiElement.FromSelector(selStr, findTimeout);
                            }

                            if (element == null)
                            {
                                Thread.Sleep(perMilliseconds);
                                continue;
                            }
                            FoundElement.Set(context, element);
                            if (WaitVisible && WaitActivity)
                            {
                                if (element.IsVisible() && UiCommon.IsForeground(element))
                                {
                                    flag = true;
                                }
                                continue;
                            }

                            if (WaitVisible)
                            {
                                flag = element.IsVisible();
                                continue;
                            }

                            if (WaitActivity)
                            {
                                flag = UiCommon.IsForeground(element);
                                continue;
                            }

                            flag = true;
                        }
                    }

                    autoSet.Set();
                }, token);
                waitTask.Start();
                if (!autoSet.WaitOne(timeout))
                {
                    tokenSource.Cancel();
                    throw new Exception("未能等到元素出现");
                }
                /*
                while (true)
                {
                    UiElement element = Common.GetValueOrDefault(context, this.Element, null);
                    if (element == null && selStr != null)
                    {
                        element = UiElement.FromSelector(selStr, timeout);
                    }
                    bool flag = true;
                    if (element == null)
                    {
                        flag = false;
                    }

                    FoundElement.Set(context, element);

                    if ((flag & WaitVisible) && !element.IsVisible())
                    {
                        flag = false;
                    }
                    if ((flag & WaitActivity) && !(bool)UiCommon.IsForeground(element))
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        break;
                    }
                    if (stopwatch.ElapsedMilliseconds > timeout)
                    {
                        throw new Exception("未能等到元素出现");
                    }
                    Thread.Sleep(perMilliseconds);
                }
                */
                context.ScheduleAction(Body, null, OnCompleted, OnFaulted);              

            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }
        }

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            //TODO
        }

        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            var repeatForever = RepeatForever;
            if (repeatForever)
            {
                try
                {
                    int timeout = Common.GetValueOrDefault(context, this.Timeout, 30000);
                    var selStr = Selector.Get(context);
                    var stopwatch = new Stopwatch();
                    if (timeout >= 0)
                    {
                        stopwatch.Start();
                    }

                    var perMilliseconds = 50;
                    var findTimeout = 2000;
                    var autoSet = new AutoResetEvent(false);
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    CancellationToken token = tokenSource.Token;
                    Task waitTask = new Task(() =>
                    {
                        var flag = false;//可见性
                        while (!flag)  //可见时跳出
                        {
                            lock (this)
                            {
                                UiElement element = Common.GetValueOrDefault(context, this.Element, null);
                                if (element == null && selStr != null)
                                {
                                    element = UiElement.FromSelector(selStr, findTimeout);
                                }

                                if (element == null)
                                {
                                    Thread.Sleep(perMilliseconds);
                                    continue;
                                }
                                FoundElement.Set(context, element);
                                if (WaitVisible && WaitActivity)
                                {
                                    if (element.IsVisible() && UiCommon.IsForeground(element))
                                    {
                                        flag = true;
                                        continue;
                                    }
                                }

                                if (WaitVisible && element.IsVisible())
                                {
                                    flag = true;
                                    continue;
                                }

                                if (WaitActivity && UiCommon.IsForeground(element))
                                {
                                    flag = true;
                                    continue;
                                }

                                flag = true;
                            }
                        }

                        autoSet.Set();
                    }, token);
                    waitTask.Start();
                    if (!autoSet.WaitOne(timeout))
                    {
                        tokenSource.Cancel();
                        throw new Exception("未能等到元素出现");
                    }
                    /*
                    while (true)
                    {
                        UiElement element = Common.GetValueOrDefault(context, this.Element, null);
                        if (element == null && selStr != null)
                        {
                            element = UiElement.FromSelector(selStr, timeout);
                        }
                        if (element == null)
                        {
                            break;
                        }
                        bool flag = true;
                        if (!WaitActivity && !WaitVisible)
                        {
                            flag = false;
                        }
                        if ((flag & WaitVisible) && element.IsVisible())
                        {
                            flag = false;
                        }
                        if ((flag & WaitActivity) && (bool)UiCommon.IsForeground(element))
                        {
                            flag = false;
                        }
                        if (flag)
                        {
                            break;
                        }
                        if (stopwatch.ElapsedMilliseconds > timeout)
                        {
                            throw new Exception("未能等到元素消失");
                        }
                        Thread.Sleep(perMilliseconds);
                    }
                    */
                    Execute(context);
                }
                catch (Exception e)
                {
                    SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                    if (!ContinueOnError)
                    {
                        throw new ActivityRuntimeException(this.DisplayName, e);
                    }
                }
            }
        }
    }
}
