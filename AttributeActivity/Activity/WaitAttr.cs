using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Identifiers;
using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AttributeActivity
{
    [Designer(typeof(WaitAttrDesigner))]
    public sealed class WaitAttr : CodeActivity
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
        [RequiredArgument]
        [DisplayName("元素")]
        [Description("使用另一个活动返回的用户界面元素变量， 该属性不能与“选取器”属性一起使用。该字段仅支持用户界面元素变量。")]
        public InArgument<UiElement> Element { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("属性")]
        [Description("要等待的属性名称。必须将文本放入引号中。")]
        public InArgument<string> AttrName { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("属性值")]
        [Description("指定属性的预期值。必须将文本放入引号中。")]
        public InArgument<object> AttrValue { get; set; }

        InArgument<Int32> _TimeOut = 10000;
        [Category("输入")]
        [DisplayName("超时（毫秒）")]
        [Description("指定最长等待时间（以毫秒为单位），如果超出该时间活动未运行，系统就会报错。默认值为30000毫秒（30秒）。")]
        public InArgument<int> TimeOut
        {
            get
            {
                return _TimeOut;
            }
            set
            {
                _TimeOut = value;
            }
        }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public IEnumerable<AttributeEnums> AttrEnums
        {
            get
            {
                return Enum.GetValues(typeof(AttributeEnums)).Cast<AttributeEnums>();
            }
        }
        
        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Attribute/WaitAttr.png";
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

        #endregion


        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                string attrName = AttrName.Get(context);
                object attrValue = AttrValue.Get(context);
                int timeOut = TimeOut.Get(context);
                bool flag = false;

                UiElement element = Common.GetValueOrDefault(context, this.Element, null);
                if (element==null&&!ContinueOnError)
                {
                    SharedObject.Instance.Output(SharedObject.OutputType.Error, "未找到相应元素");
                    throw new Exception("未找到相应元素，过程中断");
                }

                var autoSet = new AutoResetEvent(false);
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;
                Task.Run(() =>
                {
                    while (!tokenSource.IsCancellationRequested)
                    {
                        var value = element.GetAttributeValue(attrName);
                        if (attrValue.ToString()==value.ToString())
                        {
                            flag = true;
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                    autoSet.Set();
                }, token);
                autoSet.WaitOne(timeOut);
                tokenSource.Cancel();

                if (!flag && !ContinueOnError)
                {
                    SharedObject.Instance.Output(SharedObject.OutputType.Error, "相应元素的属性值未匹配，获取属性失败");
                    throw new Exception("相应元素的属性值未匹配，获取属性失败，过程中断");
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
    }
}
