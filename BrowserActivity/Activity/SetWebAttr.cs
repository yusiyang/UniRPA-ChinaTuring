using System;
using System.Activities;
using System.ComponentModel;
using Plugins.Shared.Library;
using System.Threading;
using System.Windows;
using Plugins.Shared.Library.UiAutomation;
using MouseActivity;
using Plugins.Shared.Library.Exceptions;

namespace BrowserActivity
{
    [Designer(typeof(SetWebAttrDesigner))]
    public sealed class SetWebAttr : CodeActivity
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


        #region 属性分类：输入

        InArgument<string> _Attribute;
        [Category("输入")]
        [RequiredArgument]
        [DisplayName("HTML属性")]
        [Description("待更改的 HTML 属性名称。必须将文本放入引号中。")]
        public InArgument<string> Attribute
        {
            get
            {
                return _Attribute;
            }
            set
            {
                _Attribute = value;
            }
        }

        InArgument<string> _Value;
        [Category("输入")]
        [RequiredArgument]
        [DisplayName("值")]
        [Description("将设置为指定属性的值。必须将文本放入引号中。")]
        public InArgument<string> Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
            }
        }

        #endregion


        #region 属性分类：杂项

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
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Browser/SetWebAttr.png";
            }
        }

        [Browsable(false)]
        public string DefaultName { get { return "设置Web属性"; } }

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
                string attribute_str = Attribute.Get(context);
                string attribute_value = Value.Get(context);
                //Int32 _timeout = TimeoutMS.Get(context);
                //Thread.Sleep(_timeout);
                latch = new CountdownEvent(1);
                Thread td = new Thread(() =>
                {
                    if (Selector.Expression == null)
                    {
                        //ActiveElement处理
                    }
                    else
                    {
                        var selStr = Selector.Get(context);
                        UiElement element = GetValueOrDefault(context, this.Element, null);
                        int timeout = Common.GetValueOrDefault(context, this.Timeout, 30000);
                        if (element == null && selStr != null)
                        {
                            element = UiElement.FromSelector(selStr,timeout);
                        }
                        if(element != null)
                        {
                            //element.SetForeground();
                            //MSHTML.IHTMLDocument2 currDoc = null;
                            //SHDocVw.InternetExplorer ieBrowser = GetIEFromHWndClass.GetIEFromHWnd((int)element.WindowHandle, out currDoc);
                            //MSHTML.IHTMLElement currEle = GetIEFromHWndClass.GetEleFromDoc(
                            //    element.GetClickablePoint(), (int)element.WindowHandle, currDoc);
                            //currEle.setAttribute(attribute_str, attribute_value);
                            element.SetWebAttribute(attribute_str,attribute_value);
                        }
                        else
                        {
                            throw new NotImplementedException("查找不到元素");
                        }
                    }
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
