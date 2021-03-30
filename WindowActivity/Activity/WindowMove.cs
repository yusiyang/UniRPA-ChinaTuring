using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;

namespace WindowActivity
{
    [Designer(typeof(WindowMoveDesigner))]
    public sealed class WindowMove : AsyncCodeActivity
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
        [Description("要移动和/或调整大小的窗口。仅支持窗口变量。")]
        public InArgument<Window> ActiveWindow { get; set; }

        [Category("输入")]
        [DisplayName("高度")]
        [Description("窗口的新高度。支持正整数和负整数。")]
        public InArgument<Int32> Height { get; set; }

        [Category("输入")]
        [DisplayName("宽度")]
        [Description("窗口的新宽度。支持正整数和负整数。")]
        public InArgument<Int32> Width { get; set; }

        [Category("输入")]
        [DisplayName("X")]
        [Description("窗口的新 X 位置。支持正整数和负整数。")]
        public InArgument<Int32> PosX { get; set; }


        [Category("输入")]
        [DisplayName("Y")]
        [Description("窗口的新 Y 位置。支持正整数和负整数。")]
        public InArgument<Int32> PosY { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public Window currWindow { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Window/WindowMove.png";
            }
        }

        [Browsable(false)]
        public string ClassName { get { return "WindowMove"; } }

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

            Window currWindow = ActiveWindow.Get(context);
            try
            {
                if(currWindow == null)
                {
                    PropertyDescriptor property = context.DataContext.GetProperties()[WindowActive.OpenBrowsersPropertyTag];
                    if (property == null)
                        property = context.DataContext.GetProperties()[WindowAttach.OpenBrowsersPropertyTag];
                    if(property != null)
                        currWindow = property.GetValue(context.DataContext) as Window;
                }
                Win32Api.Rect rect = new Win32Api.Rect();
                Win32Api.GetWindowRect((IntPtr)currWindow.getWindowHwnd(), out rect);
                int oldWidth = rect.Right - rect.Left;
                int oldHeight = rect.Bottom - rect.Top;
                int oldPosX = rect.Left;
                int oldPosY = rect.Top;

                int newPosX = PosX.Get(context);
                int newPosY = PosY.Get(context);
                int newWidth = Width.Get(context);
                int newHeight = Height.Get(context);

                int defPosX = newPosX == 0 ? oldPosX : newPosX;
                int defPosY = newPosY == 0 ? oldPosY : newPosY;
                int defWidth = newWidth == 0 ? oldWidth : newWidth;
                int defHeight = newHeight == 0 ? oldHeight : newHeight;

                Win32Api.MoveWindow(currWindow.getWindowHwnd(), defPosX, defPosY, defWidth, defHeight, true);
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
