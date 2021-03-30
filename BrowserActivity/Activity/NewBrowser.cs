using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.UiAutomation.Browser;
using System;
using System.Activities;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace BrowserActivity
{
    [Designer(typeof(NewBrowserDesigner))]
    public sealed class NewBrowser : AsyncCodeActivity
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


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("浏览器")]
        [Description("新浏览器标签页面。该字段仅支持浏览器变量。")]
        public OutArgument<IBrowser> Browser { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Browser/OpenBrowser.png";
            }
        }

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

        #endregion


        public string ClassName => "NewBrowser";

        static NewBrowser()
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

            try
            {
                PropertyDescriptor property = context.DataContext.GetProperties()[OpenBrowser.OpenBrowsersPropertyTag];
                if (property == null)
                    property = context.DataContext.GetProperties()[AttachBrowser.OpenBrowsersPropertyTag];

                var oldBrowser = property.GetValue(context.DataContext) as IBrowser;

                IBrowser browser = null;
                switch (oldBrowser?.Type)
                {
                    case BrowserType.Chrome:
                        browser = new ChromeBrowser();
                        break;
                    case BrowserType.Firefox:
                        browser = new FirefoxBrowser();
                        break;
                    case BrowserType.InternetExplorer:
                        browser = new IeBrowser();
                        break;
                    default:
                        break;
                }
                if (browser != null && browser.Available)
                {
                    Browser.Set(context, browser);
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

            m_Delegate = Run;
            return m_Delegate.BeginInvoke(callback, state);

        }
        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            Thread.Sleep(delayAfter);
        }
    }
}
