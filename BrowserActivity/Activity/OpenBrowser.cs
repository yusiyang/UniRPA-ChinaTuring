using System;
using System.Activities;
using System.ComponentModel;
using Plugins.Shared.Library;
using System.Threading;
using System.Activities.Statements;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Collections;
using MouseActivity;
using Plugins.Shared.Library.UiAutomation;
using Plugins.Shared.Library.UiAutomation.Browser;
using Plugins.Shared.Library.Exceptions;

namespace BrowserActivity
{
    [Designer(typeof(OpenBrowserDesigner))]
    public class OpenBrowser : NativeActivity
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
        [DisplayName("URL")]
        [Description("指定要在浏览器中打开的URL。必须将文本放入引号中。")]
        public InArgument<string> Url { get; set; }

        [Category("输入")]
        [DisplayName("浏览器类型")]
        [Description("选择要使用的浏览器类型。可用的选项如下：IE、Firefox、Chrome。")]
        public BrowserType BrowserType { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("用户界面浏览器")]
        [Description("活动结果为用户界面浏览器对象。存储所有与浏览器会话有关的信息。仅支持浏览器变量。")]
        public OutArgument<IBrowser> Browser { get; set; }

        #endregion


        #region 属性分类：选项

        /* 为等待浏览器页面显示，如果未显示无法获得对应句柄或InternetExplorer变量 */
        InArgument<Int32> _OverTime = 10 * 1000;
        [Category("选项")]
        [DisplayName("浏览器响应超时")]
        [Description("指定浏览器响应超时时间（毫秒）。为等待浏览器页面显示，如果未显示无法获得对应句柄或 InternetExplorer 变量。")]
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

        [Category("选项")]
        [DisplayName("私有")]
        [Description("打开一个私人/匿名会话。")]
        public bool Private { get; set; }


        [Category("选项")]
        [DisplayName("隐藏")]
        [Description("打开隐藏的浏览器。")]
        public bool Hidden { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public ActivityAction<object> Body { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Browser/OpenBrowser.png";
            }
        }

        #endregion


        public static string OpenBrowsersPropertyTag { get { return "OpenBrowser"; } }


        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        public OpenBrowser()
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

        protected override void Execute(NativeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            int overTime = Common.GetValueOrDefault(context, OverTime, 30000);
            Thread.Sleep(delayBefore);

            string url = Url.Get(context);
            IBrowser browser = null;
            try
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;
                }
                switch (BrowserType)
                {
                    case BrowserType.Chrome:
                        {
                            browser = new ChromeBrowser();
                            var args = "";
                            if (Hidden)
                            {
                                args += " --headless";
                            }

                            if (Private)
                            {
                                args += " --incognito";
                            }
                            browser.Open(new Uri(url), args, overTime);
                            break;
                        }
                    case BrowserType.Firefox:
                        {
                            browser = new FirefoxBrowser();
                            var args = "";
                            if (Hidden)
                            {
                                args += " -headless";
                            }

                            if (Private)
                            {
                                args += " -private-window";
                            }
                            browser.Open(new Uri(url), args, overTime);
                            break;
                        }
                    case BrowserType.InternetExplorer:
                        {
                            browser = new IeBrowser();
                            var args = " -new";
                            //if (Hidden)
                            //{
                            //    args += " -headless";
                            //}

                            if (Private)
                            {
                                args += " -private";
                            }
                            browser.Open(new Uri(url), args, overTime);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

            }
            catch (System.ComponentModel.Win32Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, "未安装相应浏览器程序:" + BrowserType);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, "打开浏览器失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }
            if (Browser != null)
            {
                Browser.Set(context, browser);
            }
            if (Body != null)
                context.ScheduleAction(Body, browser);
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
