using System;
using System.Activities;
using System.ComponentModel;
using Plugins.Shared.Library;
using System.Threading;
using System.Activities.Statements;
using System.Collections;
using Plugins.Shared.Library.UiAutomation;
using MouseActivity;
using Plugins.Shared.Library.UiAutomation.Browser;
using Plugins.Shared.Library.Exceptions;

namespace BrowserActivity
{
    [Designer(typeof(AttachBrowserDesigner))]
    public sealed class AttachBrowser : NativeActivity
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


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("G1")]
        [DisplayName("浏览器")]
        [Description("要附加到的现有浏览器变量。")]
        public InArgument<IBrowser> currBrowser { get; set; }

        [Category("输入")]
        [DisplayName("浏览器类型")]
        [Description("选择要使用的浏览器类型。可用的选项如下：IE、Firefox、Chrome。")]
        public BrowserTypes BrowserType { get; set; }

        [Category("输入")]
        [OverloadGroup("G1")]
        [DisplayName("超时（毫秒）")]
        [Description("指定最长等待时间（以毫秒为单位），如果超出该时间活动未运行，系统就会报错。默认值为30000毫秒（30秒）。")]
        public InArgument<int> Timeout { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("G2")]
        [DisplayName("选取器")]
        [Description("用于在执行活动时查找特定用户界面元素的“文本”属性。它实际上是XML片段，用于指定您要查找的图形用户界面元素及其一些父元素的属性。必须将文本放入引号中。")]
        public InArgument<string> Selector { get; set; }
        [Browsable(false)]
        public string SelectorOrigin { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("用户界面浏览器")]
        [Description("活动返回的浏览器变量。")]
        public OutArgument<IBrowser> UiBrowser { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public ActivityAction<object> Body { get; set; }

        [Browsable(false)]
        public string SourceImgPath { get; set; }

        private System.Windows.Visibility visi = System.Windows.Visibility.Hidden;
        [Browsable(false)]
        public System.Windows.Visibility visibility
        {
            get
            {
                return visi;
            }
            set
            {
                visi = value;
            }
        }

        [Browsable(false)]
        public SHDocVw.InternetExplorer SelectedIE { get; set; }

        [Browsable(false)]
        public Int32 HWND { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Browser/OpenBrowser.png";
            }
        }

        [Browsable(false)]
        public string DefaultName { get { return "附加浏览器"; } }

        #endregion


        public static string OpenBrowsersPropertyTag { get { return "AttachBrowser"; } }

        public AttachBrowser()
        {
            Body = new ActivityAction<object>
            {
                Argument = new DelegateInArgument<object>(OpenBrowsersPropertyTag),
                Handler = new Sequence()
                {
                    DisplayName = "Do"
                }
            };
        }

        CountdownEvent latch;
        private void refreshData(CountdownEvent latch)
        {
            latch.Signal();
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
                latch = new CountdownEvent(1);
                Thread td = new Thread(() =>
                {
                    if (Selector.Expression == null)
                    {
                        IBrowser browser = currBrowser.Get(context);
                        if (browser != null)
                        {
                            if (UiBrowser != null)
                            {
                                UiBrowser.Set(context, browser);
                            }
                            context.ScheduleAction(Body, browser, OnCompleted, OnFaulted);
                        }
                    }
                    else
                    {
                        var allBrowsers = new SHDocVw.ShellWindows();
                        IBrowser browser = null;
                        switch (BrowserType)
                        {
                            case BrowserTypes.IE:
                                {
                                    var selStr = Selector.Get(context);
                                    int timeout = Common.GetValueOrDefault(context, this.Timeout, 30000);
                                    UiElement element = UiElement.FromSelector(selStr, timeout);
                                    browser =new IeBrowser();
                                    break;
                                }
                            case BrowserTypes.Chrome:
                                {
                                    browser = new ChromeBrowser();
                                    break;
                                }
                            case BrowserTypes.Firefox:
                                {
                                    browser = new FirefoxBrowser();
                                    break;
                                }
                            default:
                                break;
                        }
                        if (UiBrowser != null)
                        {
                            UiBrowser.Set(context, browser);
                        }
                        context.ScheduleAction(Body, browser);
                    }

                    refreshData(latch);
                });
                td.TrySetApartmentState(ApartmentState.STA);
                td.IsBackground = true;
                td.Start();
                latch.Wait();

                Thread.Sleep(delayAfter);
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
