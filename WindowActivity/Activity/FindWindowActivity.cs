using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;

namespace WindowActivity
{
    [Designer(typeof(FindWindowDesigner))]
    public sealed class FindWindowActivity : CodeActivity
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
        [DisplayName("窗口标题")]
        [Description("指定的窗口标题。必须将文本放入引号中。")]
        public InArgument<string> Title { get; set; }

        [Category("输入")]
        [DisplayName("窗口类名")]
        [Description("指定的窗口类名。必须将文本放入引号中。")]
        public InArgument<string> ClassName { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("窗口句柄")]
        [Description("查找到的窗口句柄。")]
        public OutArgument<IntPtr> Result { get; set; }

        [Category("输出")]
        [DisplayName("窗口变量")]
        [Description("查找到的窗口变量。")]
        public OutArgument<Window> Window { get; set; }
        #endregion


        #region 输出分类：杂项

        [Browsable(false)]
        public string SourceImgPath { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Window/findwindow.png";
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
        public string DefaultName { get { return "查找窗口"; } }

        #endregion


        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                string _Title = Title.Get(context);
                string _ClassName = ClassName.Get(context);
                IntPtr _result = IntPtr.Zero;
                var currWindow = new Window();
                _result = Win32Api.FindWindow(_ClassName, _Title);
                currWindow.setWindowHwnd((int)_result);
                currWindow.setWindowText("" + _Title);
                currWindow.setWindowClass("" + _ClassName);
                Result.Set(context, _result);
                Window.Set(context,currWindow);
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
    }
}
