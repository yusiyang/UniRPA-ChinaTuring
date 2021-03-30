using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.ComponentModel;
using Plugins.Shared.Library;
using Plugins.Shared.Library.UiAutomation;
using System.Threading;
using Plugins.Shared.Library.Exceptions;

namespace MouseActivity
{
    [Designer(typeof(MouseDesigner))]
    public sealed class DoubleClickActivity : CodeActivity
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


        #region 属性分类：输入

        private MouseClickType _clicktype = MouseClickType.双击;
        [Category("输入")]
        [DisplayName("单击类型")]
        [Description("指定点击时所使用的鼠标点击类型（单击、双击、按下、弹起）。默认情况下，选择单击。")]
        public MouseClickType ClickType
        {
            get
            {
                return _clicktype;
            }
            set
            {
                if (_clicktype == value) return;
                _clicktype = value;
            }
        }

        [Category("输入")]
        [DisplayName("鼠标按键")]
        [Description("用于进行点击操作的鼠标键（左键、右键、中键）。默认情况下，选择鼠标左键。")]
        public int MouseButton { get; set; }

        #endregion


        #region 属性分类：光标位置



        [Category("光标位置")]
        [DisplayName("位置")]
        [Description("描述光标的起始点，向其添加“偏移 X”和“偏移 Y”属性的偏移量。可用的选项如下：左上、右上、左下、右下和中间。默认选项为“中间”。")]
        public int ElementPosition { get; set; }

        [Category("光标位置")]
        [DisplayName("偏移 X")]
        [Description("光标位置根据“位置”字段中选择的选项而进行的水平位移。")]
        public InArgument<int> offsetX { get; set; }

        [Category("光标位置")]
        [DisplayName("偏移 Y")]
        [Description("光标位置根据“位置”字段中选择的选项而进行的垂直位移。")]
        public InArgument<int> offsetY { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string SourceImgPath { get; set; }
        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/mouse/mouseclick.png"; } }

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
        public string DefaultName { get { return "双击"; } }

        #endregion


        static DoubleClickActivity()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(ClickActivity), "ClickType", new EditorAttribute(typeof(MouseClickTypeEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(DoubleClickActivity), "MouseButton", new EditorAttribute(typeof(MouseButtonTypeEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(DoubleClickActivity), "KeyModifiers", new EditorAttribute(typeof(KeyModifiersEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(DoubleClickActivity), "ElementPosition", new EditorAttribute(typeof(ElementPositionTypeEditor), typeof(PropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override void Execute(CodeActivityContext context)
        {
            Int32 delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            Int32 delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                int timeout = Common.GetValueOrDefault(context, this.Timeout, 30000);
                var selStr = Selector.Get(context);
                UiElement element = Common.GetValueOrDefault(context, this.Element, null);
                if (element == null && selStr != null)
                {
                    element = UiElement.FromSelector(selStr, timeout);
                }

                Int32 pointX = 0;
                Int32 pointY = 0;
                UiElementClickParams uiElementClickParams = new UiElementClickParams();
                uiElementClickParams.clickType = (ClickType)ClickType;
                uiElementClickParams.mouseButton = (MouseButton)MouseButton;

                pointX = offsetX.Get(context);
                pointY = offsetY.Get(context);
                Offset offset = new Offset(pointX, pointY);
                uiElementClickParams.offset = offset;

                if (element != null)
                {
                    element.SetForeground();
                }
                else
                {
                    throw new NotImplementedException("查找不到元素");
                }
                if (KeyModifiers != null)
                {
                    string[] sArray = KeyModifiers.Split(',');
                    foreach (string i in sArray)
                    {
                        Common.DealKeyBordPress(i);
                    }
                }

                element.MouseClick(uiElementClickParams);

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
        private void onComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {

        }
    }
}
