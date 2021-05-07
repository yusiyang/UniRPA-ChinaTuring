using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.Images;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MouseActivity.Activity
{
    [Designer(typeof(ClickImageDesigner))]
    public sealed class ClickImageActivity : CodeActivity
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
        [DisplayName("单击类型")]
        [Description("指定模拟点击事件时所使用的鼠标点击类型（单击、双击、按下、弹起）。默认情况下，选择单击。")]
        public int ClickType { get; set; }

        [Category("输入")]
        [DisplayName("鼠标按键")]
        [Description("用于进行点击操作的鼠标键（左键、右键、中键）。默认情况下，选择鼠标左键。")]
        public int MouseButton { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("修饰键")]
        [Description("使您能够添加修饰键。可用的选项如下：Alt、Ctrl、Shift、Win。")]
        public string KeyModifiers { get; set; }

        [Category("选项")]
        [DisplayName("发送窗口消息")]
        [Description("如果选中，则通过向目标应用程序发送一条特定消息的方式执行点击。这种输入方法可在后台工作，且兼容大多数桌面应用程序，但并不是速度最快的方法。默认情况下，该复选框是未选中状态。如果既未选中该复选框，也未选中“模拟点击”复选框，则默认方法通过使用硬件驱动程序模拟点击。默认方法速度最慢，且不能在后台工作，但可兼容所有桌面应用程序。")]
        public bool SendWindowMessage { get; set; }

        [Category("选项")]
        [DisplayName("模拟单击")]
        [Description("如果选中，则通过使用目标应用程序的技术模拟点击。这种输入方法是三种方法中最快的，且可在后台工作。默认情况下， 该复选框是未选中状态。如果既未选中该复选框，也未选中“发送窗口消息”复选框，则默认方法通过使用硬件驱动程序执行点击。默认方法速度最慢，且不能在后台工作，但可兼容所有桌面应用程序。")]
        public bool SimulateSingleClick { get; set; }

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

        #endregion

        #region 属性分类：光标位置

        [Category("光标位置")]
        [DisplayName("偏移 X")]
        [Description("光标位置从元素中心点进行的水平位移。")]
        public InArgument<int> offsetX { get; set; }

        [Category("光标位置")]
        [DisplayName("偏移 Y")]
        [Description("光标位置从元素中心点进行的垂直位移。")]
        public InArgument<int> offsetY { get; set; }

        #endregion

        #region 属性分类：杂项

        [Browsable(false)]
        public string MainDisplayName => "ClickImage";

        [Browsable(false)]
        public string IcoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/mouse/mouseclick.png";
            }
        }

        [Browsable(false)]
        public Visibility Visibility { get; set; } = Visibility.Hidden;

        [Browsable(false)]
        public string SourceImgPath { get; set; }

        [Browsable(false)]
        public string DefaultName { get { return "单击图片"; } }

        #endregion


        static ClickImageActivity()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(ClickImageActivity), "ClickType", new EditorAttribute(typeof(MouseClickTypeEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(ClickImageActivity), "MouseButton", new EditorAttribute(typeof(MouseButtonTypeEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(ClickImageActivity), "KeyModifiers", new EditorAttribute(typeof(KeyModifiersEditor), typeof(PropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            int timeout = Common.GetValueOrDefault(context, this.Timeout, 30000);
            var selStr = Selector.Get(context);
            var processUiElement = Common.GetValueOrDefault(context, this.Element, null);
            if (processUiElement == null && selStr != null)
            {
                processUiElement = UiElement.FromSelector(selStr, timeout);
            }

            var clickPoint = GetClickPoint(processUiElement, context);

            int pointX = offsetX.Get(context);
            int pointY = offsetY.Get(context);
            clickPoint.X += pointX;
            clickPoint.Y += pointY;

            try
            {
                if (KeyModifiers != null)
                {
                    string[] sArray = KeyModifiers.Split(',');
                    foreach (string i in sArray)
                    {
                        Common.DealKeyBordPress(i);
                    }
                }

                UiElementClickParams uiElementClickParams = new UiElementClickParams();
                uiElementClickParams.clickType = (ClickType)ClickType;
                uiElementClickParams.mouseButton = (MouseButton)MouseButton;

                //如果需要找图片中的元素
                if (SimulateSingleClick || SendWindowMessage)
                {
                    var uiElement = UiCommon.GetUiElementByPoint(clickPoint, processUiElement);

                    if (SimulateSingleClick)
                    {
                        uiElementClickParams.mouseActionType = MouseActionType.Simulate;
                    }

                    if (SendWindowMessage)
                    {
                        uiElementClickParams.mouseActionType = MouseActionType.SendWindowMessage;
                    }
                    uiElement.MouseClick(uiElementClickParams);
                }


                //正常点击,直接点击图片位置
                else
                {
                    UiElement.MouseSetPostion(clickPoint);
                    UiElement.MouseAction((ClickType)ClickType, (MouseButton)MouseButton);
                }

                if (KeyModifiers != null)
                {
                    string[] sArray = KeyModifiers.Split(',');
                    foreach (string i in sArray)
                    {
                        Common.DealKeyBordRelease(i);
                    }
                }
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

        private System.Drawing.Point GetClickPoint(UiElement processUiElement, CodeActivityContext context)
        {
            var screenShot = Image.FromFile(SharedObject.Instance.ProjectPath + @"\.screenshots\" + SourceImgPath);
            var result = new Rectangle();
            var cts = new CancellationTokenSource();
            var autoSet = new AutoResetEvent(false);
            var timeout = Common.GetValueOrDefault(context, this.Timeout, 30000);
            new Task(() =>
            {
                while (!cts.IsCancellationRequested)
                {
                    if (result == Rectangle.Empty)
                    {
                        var processScreenShot = UIAUiNode.UIAAutomation.GetDesktop().Capture();
                        ImageSearchEngine.Initialize(processScreenShot, screenShot);
                        result = ImageSearchEngine.Search();
                        Thread.Sleep(50);
                        continue;
                    }
                    autoSet.Set();
                    break;
                }
            }, cts.Token).Start();
            autoSet.WaitOne(timeout);
            cts.Cancel();

            if (result == default || result.IsEmpty)
            {
                throw new Exception("查找不到图片");
            }

            var centerPoint = result.Center();
            var processRectangle = UiElement.Desktop.BoundingRectangle;
            centerPoint.X += processRectangle.X;
            centerPoint.Y += processRectangle.Y;

            return centerPoint;
        }
    }
}
