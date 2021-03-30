using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;

namespace WindowActivity
{
    [Designer(typeof(WindowMaxiDesigner))]
    public sealed class WindowMaxi : AsyncCodeActivity
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
        [DisplayName("窗口")]
        [Description("要最大化的窗口。仅支持窗口变量。")]
        public InArgument<Window> ActiveWindow { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public Window currWindow { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Window/WindowMaxi.png";
            }
        }

        [Browsable(false)]
        public string ClassName { get { return "WindowMaxi"; } }

        #endregion


        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return ClassName;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            Window Windowmaxi = ActiveWindow.Get(context);
            try
            {
                if (Windowmaxi != null)
                {
                    Win32Api.SendMessage((IntPtr)Windowmaxi.getWindowHwnd(), Win32Api.WM_SYSCOMMAND, (IntPtr)Win32Api.SC_MAXIMIZE, IntPtr.Zero);
                }
                else
                {
                    PropertyDescriptor property = context.DataContext.GetProperties()[WindowActive.OpenBrowsersPropertyTag];
                    if (property == null)
                        property = context.DataContext.GetProperties()[WindowAttach.OpenBrowsersPropertyTag];
                    if (property != null)
                    {
                        Window getBrowser = property.GetValue(context.DataContext) as Window;
                        Win32Api.SendMessage((IntPtr)getBrowser.getWindowHwnd(), Win32Api.WM_SYSCOMMAND, (IntPtr)Win32Api.SC_MAXIMIZE, IntPtr.Zero);
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
