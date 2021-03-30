using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.ComponentModel;
using System.Activities.Presentation.PropertyEditing;
using System.Security;
using System.Windows;
using MouseActivity;
using System.Runtime.InteropServices;
using Plugins.Shared.Library.UiAutomation;
using Plugins.Shared.Library;
using System.Threading;
using System.Windows.Forms;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.WindowsAPI;

namespace KeyboardActivity
{
    [Designer(typeof(SecureTextDesigner))]
    public sealed class SecureTextActivity : CodeActivity
    {
        static SecureTextActivity()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(SecureTextActivity), "ClickType", new EditorAttribute(typeof(MouseClickTypeEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(SecureTextActivity), "MouseButton", new EditorAttribute(typeof(MouseButtonTypeEditor), typeof(PropertyValueEditor)));
            //builder.AddCustomAttributes(typeof(SecureTextActivity), "KeyModifiers", new EditorAttribute(typeof(KeyModifiersEditor), typeof(PropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }


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
        public InArgument<int> offsetX { get; set; }

        [Category("光标位置")]
        [DisplayName("偏移 Y")]
        [Description("光标位置从元素中心点进行的垂直位移。")]
        public InArgument<int> offsetY { get; set; }

        #endregion


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("安全文本")]
        [Description("待写入指定用户界面元素的安全文本。必须将文本放入引号中。")]
        public InArgument<SecureString> SecureText { get; set; }

        #endregion


        #region 属性分类：选项
        [Category("选项")]
        [DisplayName("模拟键入")]
        [Description("如果选中，则通过使用目标应用程序的技术模拟类型。这种输入方法是三种方法中最快的，且可在后台工作。默认情况下，该复选框是未选中状态。如果既未选中该复选框，也未选中“发送窗口消息”复选框，则默认方法通过使用硬件驱动程序执行点击。默认方法速度最慢，且不能在后台工作，但可兼容所有桌面应用程序。")]
        public bool SimulateInput { get; set; }

        [Category("选项")]
        [DisplayName("空字段")]
        [Description("选中该复选框时，系统回再写入文本前清除用户界面元素中所有之前存在的内容。")]
        public bool EmptyText { get; set; }

        [Category("选项")]
        [DisplayName("键之间延迟")]
        [Description("两次击键之间的延迟时间（以毫秒为单位）。默认时间量为 10 毫秒。")]
        public InArgument<int> DelayBetweenInputs { get; set; }

        [Category("选项")]
        [DisplayName("键入前单击")]
        [Description("选中该复选框时，在写入文本之前单击指定用户界面元素。")]
        public bool IsRunClick { get; set; }

        //[Category("选项")]
        //[DisplayName("修饰键")]
        //[Description("使您能够添加修饰键。可用的选项如下：Alt、Ctrl、Shift、Win。")]
        //public string KeyModifiers { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/hotkey/text.png"; } }

        [Browsable(false)]
        public List<string> KeyTypes
        {
            get
            {
                KeyboardTypes key = new KeyboardTypes();
                return key.getKeyTypes;
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
        public string DefaultName { get { return "输入安全文本"; } }

        #endregion

        void ParseStringToList(string inText, List<string> strList)
        {
            string strBuff = "";
            string keyBuff = "";
            bool isKeyFlag = false;
            for (int counter = 0; counter < inText.Length; counter++)
            {
                if (counter < inText.Length - 1)
                {
                    if (inText[counter] == '[' && inText[counter + 1] == 'k')
                    {
                        isKeyFlag = true;
                    }
                    if (inText[counter] == ')' && inText[counter + 1] == ']')
                    {
                        isKeyFlag = false;
                    }
                }
                if (isKeyFlag)
                {
                    keyBuff += inText[counter].ToString();
                    if (strBuff != "")
                    {
                        strBuff = strBuff.Replace("[k(", "");
                        strBuff = strBuff.Replace(")]", "");
                        strList.Add(strBuff);
                        strBuff = "";
                    }
                }
                else
                {
                    strBuff += inText[counter].ToString();

                    if (keyBuff != "")
                    {
                        keyBuff = keyBuff.Replace("[k(", "");
                        keyBuff = keyBuff.Replace("[k(", "");
                        keyBuff = "[(" + keyBuff + ")]";
                        strList.Add(keyBuff);
                        keyBuff = "";
                    }
                }

                if (counter == inText.Length - 1 && inText[counter] != ']')
                {
                    strBuff = strBuff.Replace("[k(", "");
                    strBuff = strBuff.Replace(")]", "");
                    strList.Add(strBuff);
                }
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);
            var delayBetweenInputs = Common.GetValueOrDefault(context, DelayBetweenInputs, 10);

            try
            {
                SecureString secureText = SecureText.Get(context);
                IntPtr inP = Marshal.SecureStringToBSTR(secureText);//inP为secureStr的句柄
                string text = Marshal.PtrToStringBSTR(inP);

                List<string> strList = new List<string>();
                ParseStringToList(text, strList);



                int timeout = Common.GetValueOrDefault(context, this.Timeout, 30000);
                var selStr = Selector.Get(context);
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
                else
                {
                    SharedObject.Instance.Output(SharedObject.OutputType.Error, "有一个错误产生", "查找不到元素");
                    if (ContinueOnError)
                    {
                        return;
                    }
                    else
                    {
                        throw new NotImplementedException("查找不到元素");
                    }
                }
                /*执行鼠标点击事件*/
                if (IsRunClick)
                {
                    UiElementClickParams uiElementClickParams = new UiElementClickParams();
                    Offset offset = new Offset(pointX, pointY);
                    uiElementClickParams.offset = offset;
                    element.MouseClick(uiElementClickParams);
                }
                IntPtr windowHandle = IntPtr.Zero;
                windowHandle = element.MainWindowHandle;

                using (var imeScope = IMEScope.BeginEdit(windowHandle, IMEHelper.HKL_ENGLISH_US))
                {
                    element.Focus();
                    //清空输入内容
                    if (EmptyText)
                    {
                        UiKeyboard.New().Press(VirtualKey.CONTROL).With(VirtualKey.KEY_A).Continue(VirtualKey.DELETE).End();
                    }

                    foreach (string str in strList)
                    {
                        string strValue = str;
                        if (strValue.Contains("[(") && strValue.Contains(")]"))
                        {
                            strValue = strValue.Replace("[(", "");
                            strValue = strValue.Replace(")]", "");
                            if (SimulateInput)
                            {
                                Thread.Sleep(delayBetweenInputs);
                            }

                            if (Common.DealVirtualKeyPress(strValue.ToUpper()))
                            {
                                Common.DealVirtualKeyRelease(strValue.ToUpper());
                            }
                            else
                            {
                                SharedObject.Instance.Output(SharedObject.OutputType.Error, "有一个错误产生", "找不到键值");
                                if (ContinueOnError)
                                {
                                    return;
                                }
                                else
                                {
                                    throw new NotImplementedException("找不到键值");
                                }
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(strValue))
                        {

                            if (!SimulateInput)
                            {
                                foreach (var item in strValue)
                                {
                                    Thread.Sleep(delayBetweenInputs);
                                    SendKeys.SendWait(item.ToString());
                                }
                            }
                            else
                            {
                                element.SimulateTypeText(strValue);
                            }
                        }
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
        }
    }
}
