using MouseActivity;
using Plugins.Shared.Library;
using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;
using Plugins.Shared.Library.UiAutomation;
using Plugins.Shared.Library.UiAutomation.Browser;
using Plugins.Shared.Library.Exceptions;

namespace BrowserActivity
{
    [Designer(typeof(CloseTabDesigner))]
    public sealed class CloseTab : AsyncCodeActivity
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
        [DisplayName("浏览器")]
        [Description("要关闭的浏览器页面。该字段仅支持浏览器变量。")]
        public InArgument<IBrowser> currBrowser { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string guid { get { return classID; } }

        [Browsable(false)]
        public string SourceImgPath { get; set; }

        [Browsable(false)]
        public string ClassName { get { return "CloseTab"; } }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Window/WindowClose.png";
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


        private string classID = Guid.NewGuid().ToString("N");

        static CloseTab()
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
                IBrowser browser= currBrowser.Get(context);
                //先走本地Browser逻辑 其次走流程
                if (browser == null)
                {
                    PropertyDescriptor property = context.DataContext.GetProperties()[OpenBrowser.OpenBrowsersPropertyTag];
                    if (property == null)
                        property = context.DataContext.GetProperties()[AttachBrowser.OpenBrowsersPropertyTag];
                    if (property == null)
                    {
                        SharedObject.Instance.Output(SharedObject.OutputType.Error, "活动流程传递的浏览器变量为空，请检查！");
                        m_Delegate = new runDelegate(Run);
                        return m_Delegate.BeginInvoke(callback, state);
                    }
                    browser = property.GetValue(context.DataContext) as IBrowser;
                    browser?.Close();
                }
                else
                {
                    browser.Close();
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

            m_Delegate = new runDelegate(Run);
            return m_Delegate.BeginInvoke(callback, state);

        }
        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            Thread.Sleep(delayAfter);
        }
    }
}
