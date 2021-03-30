using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.UiAutomation;

namespace WindowActivity
{
    [Designer(typeof(WindowCloseDesigner))]
    public sealed class WindowClose : AsyncCodeActivity
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
        [OverloadGroup("G1")]
        [DisplayName("超时（毫秒）")]
        [Description("指定最长等待时间（以毫秒为单位），如果超出该时间活动未运行，系统就会报错。默认值为30000毫秒（30秒）。")]
        public InArgument<int> Timeout { get; set; }

        [Category("输入")]
        [DisplayName("选取器")]
        [Description("用于在执行活动时查找特定用户界面元素的“文本”属性。它实际上是XML片段，用于指定您要查找的图形用户界面元素及其一些父元素的属性。必须将文本放入引号中。")]
        public InArgument<string> Selector { get; set; }
        [Browsable(false)]
        public string SelectorOrigin { get; set; }

        [Category("输入")]
        [DisplayName("使用窗口")]
        [Description("要关闭的窗口。仅支持窗口变量。")]
        public InArgument<Window> ActiveWindow { get; set; }

        [Category("输入")]
        [DisplayName("窗口句柄")]
        [Description("要关闭的窗口句柄。")]
        public InArgument<IntPtr> handle { get; set; }

        [Category("输入")]
        [DisplayName("等待准备就绪")]
        [Description("执行操作之前，等待目标准备就绪。可用的选项如下：无 - 不等待目标准备就绪；交互 - 等到只加载了应用程序的一部分；完成 - 等到加载了整个应用程序。")]
        public InArgument<Int32> TimeoutMS { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string SourceImgPath{ get;set;}

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

        [Browsable(false)]
        public string ClassName { get { return "WindowClose"; } }


        [Browsable(false)]
        public string DefaultName { get { return "关闭窗口"; } }

        #endregion


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

            Window currWindow = ActiveWindow.Get(context);
            IntPtr _handle = handle.Get(context);
            if(currWindow == null && _handle!=IntPtr.Zero)
            {
                currWindow = new Window();
                currWindow.setWindowHwnd((int)_handle);
            }
            try
            {
                int timeout = Common.GetValueOrDefault(context, this.Timeout, 30000);
                var selStr = Selector.Get(context);
                UiElement element = UiElement.FromSelector(selStr,timeout);

                if (currWindow != null)
                {
                    Win32Api.SendMessage((IntPtr)currWindow.getWindowHwnd(), Win32Api.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
                else if(element != null)
                {
                    Win32Api.SendMessage(element.WindowHandle, Win32Api.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
                else
                {
                    PropertyDescriptor property = context.DataContext.GetProperties()[WindowActive.OpenBrowsersPropertyTag];
                    if (property == null)
                        property = context.DataContext.GetProperties()[WindowAttach.OpenBrowsersPropertyTag];
                    if(property != null)
                    {
                        Window getBrowser = property.GetValue(context.DataContext) as Window;
                        Win32Api.SendMessage((IntPtr)getBrowser.getWindowHwnd(), Win32Api.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
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
