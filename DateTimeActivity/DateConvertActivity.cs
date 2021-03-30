using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace DateTimeActivity
{
    [Designer(typeof(GetDateTimeDesigner))]
    public sealed class DateConvertActivity : CodeActivity
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
        [DisplayName("日期时间")]
        public InArgument<string> SDate { get; set;}

        public enum ConvertType
        {
            日期时间,
            日期,
            时间,
        }
        [Category("输入")]
        [DisplayName("转换类型")]
        public ConvertType convertType { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("时间")]
        public OutArgument<DateTime> Date { get; set; }

        [Category("输出")]
        [DisplayName("时间字符串")]
        public OutArgument<string> DateStr { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DateTime/date.png";
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
                string _SDate = SDate.Get(context);
                DateTime _dateTime = new DateTime();
                DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo();
                string _DateStr = "";
                switch (convertType)
                {
                    case ConvertType.日期时间:
                        //dtFormat.ShortDatePattern = "yyyy/MM/dd HH:mm:ss";
                        _DateStr = Convert.ToDateTime(_SDate).ToString("yyyy-MM-dd HH:mm:ss");
                        break;
                    case ConvertType.日期:
                        //dtFormat.ShortDatePattern = "yyyy/MM/dd";
                        _DateStr = Convert.ToDateTime(_SDate).ToString("yyyy-MM-dd");

                        break;
                    case ConvertType.时间:
                        //dtFormat.ShortTimePattern = "HH:mm:ss";
                        _DateStr = Convert.ToDateTime(_SDate).ToString("HH:mm:ss");
                        break;
                    default:
                        break;
                }
                _dateTime = Convert.ToDateTime(_SDate, dtFormat);
                Date.Set(context,_dateTime);
                DateStr.Set(context, _DateStr);
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
