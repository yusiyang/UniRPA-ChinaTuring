using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;

namespace DialogActivity.Activity
{
    [Designer(typeof(DialogActivity.Designer.CalloutActivityDesigner))]
    public sealed class CalloutActivity : CodeActivity
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
        [OverloadGroup("G1")]
        [DisplayName("选取器")]
        [Description("用于在执行活动时查找特定用户界面元素的“文本”属性。它实际上是XML片段，用于指定您要查找的图形用户界面元素及其一些父元素的属性。必须将文本放入引号中。")]
        public InArgument<string> Selector { get; set; }
        [Browsable(false)]
        public string SelectorOrigin { get; set; }

        #endregion


        #region 属性分类：光标位置

        [Category("光标位置")]
        [DisplayName("偏移 X")]
        [Description("光标位置从元素中心点进行的水平位移。")]
        public InArgument<Int32> offsetX { get; set; }

        [Category("光标位置")]
        [DisplayName("偏移 Y")]
        [Description("光标位置从元素中心点进行的垂直位移。")]
        public InArgument<Int32> offsetY { get; set; }

        #endregion


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("内容")]
        [Description("弹出窗口的文本。必须将文本放入引号中。")]
        public InArgument<string> Content { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("标题")]
        [Description("弹出窗口的标题。必须将文本放入引号中。")]
        public InArgument<string> Title { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("计时器")]
        [Description("持续时间，直到弹出窗口自动关闭。默认情况下，弹出窗口不会关闭。")]
        public InArgument<Int32> Timers { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Dialog/callout.png";
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
        public string DefaultName { get { return "标注"; } }

        #endregion


        CountdownEvent latch;
        private void refreshData(CountdownEvent latch)
        {
            latch.Signal();
        }

        public static T GetValueOrDefault<T>(ActivityContext context, InArgument<T> source, T defaultValue)
        {
            T result = defaultValue;
            if (source != null && source.Expression != null)
            {
                result = source.Get(context);
            }
            return result;
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                int timeout = Common.GetValueOrDefault(context, this.Timeout, 30000);
                var selStr = Selector.Get(context);
                UiElement element = GetValueOrDefault(context, this.Element, null);
                if (element == null && selStr != null)
                {
                    element = UiElement.FromSelector(selStr, timeout);
                }
                int pointX = 0;
                int pointY = 0;
                if (element != null)
                {
                    element.SetForeground();
                    pointX = element.GetClickablePoint().X + offsetX.Get(context);
                    pointY = element.GetClickablePoint().Y + offsetY.Get(context);
                }
                else
                {
                    throw new NotImplementedException("查找不到元素");
                }

                Int32 _timers = Timers.Get(context);
                string _content = Content.Get(context);
                string _title = Title.Get(context);
                latch = new CountdownEvent(1);
                Thread td = new Thread(() =>
                {
                    DialogActivity.Windows.CalloutWindow dlg = new DialogActivity.Windows.CalloutWindow(_timers);
                    dlg.setContent(_content);
                    dlg.setTitle(_title);
                    dlg.Title = _title;
                    double _height = dlg.getCanvasHeight();
                    double _width = dlg.getCanvasWidth() / 2;
                    dlg.Top = pointY - _height;
                    dlg.Left = pointX - _width;
                    dlg.ShowDialog();
                    refreshData(latch);
                });
                td.TrySetApartmentState(ApartmentState.STA);
                td.IsBackground = true;
                td.Start();
                latch.Wait();
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
