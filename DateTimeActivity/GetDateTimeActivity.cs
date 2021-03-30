using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;

namespace DateTimeActivity
{
    [Designer(typeof(GetDateTimeDesigner))]
    public sealed class GetDateTimeActivity : CodeActivity
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

        DateTimeType _dateTimeType;
        [Category("输入")]
        [DisplayName("时间类型")]
        public DateTimeType DateType
        {
            get { return _dateTimeType; }
            set { _dateTimeType = value; }
        }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("时间")]
        public OutArgument<string> Date { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DateTime/convert.png";
            }
        }

        #endregion


        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);
            try
            {
                string strDate = "";
                switch (DateType)
                {
                    case DateTimeType.日期时间:
                        strDate = DateTime.Now.ToString();
                        break;
                    case DateTimeType.日期:
                        strDate = DateTime.Now.ToShortDateString().ToString();
                        break;
                    case DateTimeType.时间:
                        strDate = DateTime.Now.ToLongTimeString().ToString();
                        break;
                    case DateTimeType.年:
                        strDate = DateTime.Now.Year.ToString();
                        break;
                    case DateTimeType.月:
                        strDate = DateTime.Now.Month.ToString();
                        break;
                    case DateTimeType.日:
                        strDate = DateTime.Now.Day.ToString();
                        break;
                    case DateTimeType.时:
                        strDate = DateTime.Now.Hour.ToString();
                        break;
                    case DateTimeType.分:
                        strDate = DateTime.Now.Minute.ToString();
                        break;
                    case DateTimeType.秒:
                        strDate = DateTime.Now.Second.ToString();
                        break;
                    default:
                        break;
                }
                Date.Set(context, strDate);
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
