using System;
using System.Activities;
using System.ComponentModel;
using System.Text;
using System.Runtime.InteropServices;
using System.Activities.Statements;
using Plugins.Shared.Library.UiAutomation;
using Plugins.Shared.Library;
using MouseActivity;
using System.Threading;
using Plugins.Shared.Library.Exceptions;

namespace WindowActivity
{
    [Designer(typeof(WindowAttachDesigner))]
    public sealed class WindowAttach : NativeActivity
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

        [RequiredArgument]
        [Category("输入")]
        [OverloadGroup("G1")]
        [DisplayName("选取器")]
        [Description("用于在执行活动时查找特定用户界面元素的“文本”属性。它实际上是XML片段，用于指定您要查找的图形用户界面元素及其一些父元素的属性。必须将文本放入引号中。")]
        public InArgument<string> Selector { get; set; }
        [Browsable(false)]
        public string SelectorOrigin { get; set; }

        [RequiredArgument]
        [Category("输入")]
        [OverloadGroup("G2")]
        [DisplayName("窗口")]
        [Description("要附加的窗口。该字段仅接受窗口变量。")]
        public InArgument<Window> ActiveWindow { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("应用程序窗口")]
        [Description("已查找到的活动窗口。该字段仅支持窗口变量。指定窗口变量时，忽略“搜索范围”和“选取器”属性。")]
        public OutArgument<Window> OutPutWindow { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public ActivityAction<object> Body { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Window/WindowAttach.png";
            }
        }

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
        public string DefaultName { get { return "附加窗口"; } }

        #endregion


        internal static string OpenBrowsersPropertyTag { get { return "WindowAttach"; } }

        public WindowAttach()
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

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetForegroundWindow();

        [DllImport("user32.dll", EntryPoint = "GetWindowText")]
        public static extern int GetWindowText(int hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "GetClassName")]
        public static extern int GetClassName(int hWnd, StringBuilder lpString, int nMaxCont);
        protected override void Execute(NativeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                int timeout = Common.GetValueOrDefault(context, this.Timeout, 30000);
                var selStr = Selector.Get(context);
                UiElement element = UiElement.FromSelector(selStr, timeout);
                if (element != null)
                {
                    element.SetForeground();
                    int hwnd = (int)element.WindowHandle;
                    StringBuilder windowText = new StringBuilder(256);
                    GetWindowText(hwnd, windowText, 256);
                    StringBuilder className = new StringBuilder(256);
                    GetClassName(hwnd, className, 256);

                    Window currWindow = new Window();
                    currWindow.setWindowHwnd(hwnd);
                    if (Body != null)
                        context.ScheduleAction(Body, currWindow, OnCompleted, OnFaulted);

                }
                else
                {
                    throw new NotImplementedException("查找不到元素");
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
