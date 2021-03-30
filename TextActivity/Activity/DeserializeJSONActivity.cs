using Plugins.Shared.Library;
using System;
using System.Activities;
using System.ComponentModel;
using MouseActivity;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Plugins.Shared.Library.Exceptions;

namespace TextActivity
{
    /// <summary>
    /// 反序列化Json
    /// </summary>
    [Designer(typeof(DeserializeJSONDesigner))]
    public sealed class DeserializeJSONActivity : CodeActivity
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
        [Browsable(true)]
        [DisplayName("Json字符串")]
        [Description("需要被反序列化的Json字符串。必须将文本放入引号中。")]
        public InArgument<string> JsonString { get; set; }
        #endregion

        #region 属性分类：输出

        [Category("输出")]
        [Browsable(true)]
        [DisplayName("JObject对象")]
        [Description("输入字符串的反序列化结果。")]
        public OutArgument<JObject> JsonObject { get; set; }

        #endregion

        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Text/DeserializeJSON.png";
            }
        }

        #endregion

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);
            string jsonStr = JsonString.Get(context);
            try
            {
                JObject jObject = JObject.Parse(jsonStr);
                JsonObject.Set(context, jObject);
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