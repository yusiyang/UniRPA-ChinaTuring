using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.ComponentModel;
using System.Activities.Presentation.PropertyEditing;
using Plugins.Shared.Library;
using Plugins.Shared.Library.UiAutomation;
using MouseActivity;
using System.Threading;
using Plugins.Shared.Library.Exceptions;

namespace KeyboardActivity
{
    [Designer(typeof(HotKeyDesigner))]
    public sealed class HotKeyActivity : CodeActivity
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
        [DisplayName("键入前单击")]
        [Description("选中该复选框时，在写入文本之前单击指定用户界面元素。")]
        public bool IsRunClick { get; set; }

        #endregion


        #region 属性分类：修饰键

        [Category("修饰键")]
        [DisplayName("Alt")]
        [Description("使您能够添加修饰键：Alt。")]
        public bool Alt { get; set; }

        [Category("修饰键")]
        [DisplayName("Ctrl")]
        [Description("使您能够添加修饰键：Ctrl。")]
        public bool Ctrl { get; set; }

        [Category("修饰键")]
        [DisplayName("Shift")]
        [Description("使您能够添加修饰键：Shift。")]
        public bool Shift { get; set; }

        [Category("修饰键")]
        [DisplayName("Win")]
        [Description("使您能够添加修饰键：Win。")]
        public bool Win { get; set; }

        #endregion


        #region 属性分类：输入

        [Category("输入")]
        [DisplayName("单击类型")]
        [Description("指定点击时所使用的鼠标点击类型（单击、双击、按下、弹起）。默认情况下，选择单击。")]
        public int ClickType { get; set; }

        [Category("输入")]
        [DisplayName("鼠标按键")]
        [Description("用于进行点击操作的鼠标键（左键、右键、中键）。默认情况下，选择鼠标左键。")]
        public int MouseButton { get; set; }

        [Category("输入")]
        [DisplayName("键值")]
        [Description("组成已发送热键的按键。")]
        public string SelectedKey { get; set; }

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
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/hotkey/hotkey.png"; } }

        [Browsable(false)]
        public List<string> KeyTypes
        {
            get
            {
                KeyboardTypes key = new KeyboardTypes();
                return key.getKeyTypes;
            }
            set
            {

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
        public string DefaultName { get { return "发送热键"; } }

        #endregion


        static HotKeyActivity()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(HotKeyActivity), "ClickType", new EditorAttribute(typeof(MouseClickTypeEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(HotKeyActivity), "MouseButton", new EditorAttribute(typeof(MouseButtonTypeEditor), typeof(PropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        private void DealBaseKeyBordPress()
        {
            if (Alt)
                Common.DealKeyBordPress("Alt");
            if (Ctrl)
                Common.DealKeyBordPress("Ctrl");
            if (Shift)
                Common.DealKeyBordPress("Shift");
            if (Win)
                Common.DealKeyBordPress("Win");
        }

        private void DealBaseKeyBordRelease()
        {
            if (Alt)
                Common.DealKeyBordRelease("Alt");
            if (Ctrl)
                Common.DealKeyBordRelease("Ctrl");
            if (Shift)
                Common.DealKeyBordRelease("Shift");
            if (Win)
                Common.DealKeyBordRelease("Win");
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

                if (string.IsNullOrWhiteSpace(selStr))
                {
                    DealBaseKeyBordPress();
                    if (Common.DealVirtualKeyPress(SelectedKey.ToUpper()))
                    {
                        Common.DealVirtualKeyRelease(SelectedKey.ToUpper());
                        DealBaseKeyBordRelease();

                        Thread.Sleep(delayAfter);
                    }
                    else
                    {
                        DealBaseKeyBordRelease();
                        SharedObject.Instance.Output(SharedObject.OutputType.Error, "有一个错误产生", "找不到键值");
                        if (ContinueOnError)
                        {
                            Thread.Sleep(delayAfter);
                            return;
                        }
                        else
                        {
                            Thread.Sleep(delayAfter);
                            throw new NotImplementedException("找不到键值");
                        }
                    }
                }

                else
                {
                    UiElement element = Common.GetValueOrDefault(context, this.Element, null);
                    if (element == null && selStr != null)
                    {
                        element = UiElement.FromSelector(selStr, timeout);
                    }

                    int pointX = offsetX.Get(context);
                    int pointY = offsetY.Get(context);
                    if (element != null)
                    {
                        element.SetForeground();
                    }
                    else if (!string.IsNullOrEmpty(selStr))
                    {
                        if (ContinueOnError)
                        {
                            return;
                        }
                        else
                        {
                            throw new NotImplementedException("查找不到元素");
                        }
                    }
                    DealBaseKeyBordPress();
                    if (IsRunClick)
                    {
                        UiElementClickParams uiElementClickParams = new UiElementClickParams();
                        Offset offset = new Offset(pointX, pointY);
                        uiElementClickParams.offset = offset;
                        element.MouseClick(uiElementClickParams);
                    }
                    if (Common.DealVirtualKeyPress(SelectedKey.ToUpper()))
                    {
                        Common.DealVirtualKeyRelease(SelectedKey.ToUpper());
                        DealBaseKeyBordRelease();

                        Thread.Sleep(delayAfter);
                    }
                    else
                    {
                        DealBaseKeyBordRelease();
                        SharedObject.Instance.Output(SharedObject.OutputType.Error, "有一个错误产生", "找不到键值");
                        if (ContinueOnError)
                        {
                            Thread.Sleep(delayAfter);
                            return;
                        }
                        else
                        {
                            Thread.Sleep(delayAfter);
                            throw new NotImplementedException("找不到键值");
                        }
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
        }
    }
}
