using MouseActivity;
using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace TextActivity
{
    [Designer(typeof(FindTextDesigner))]
    public sealed class FindTextActivity : CodeActivity
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

        //[Category("常见")]
        //[DisplayName("出错时继续")]
        //[Description("指定即使在当前活动失败的情况下，仍继续执行剩余的活动。仅支持布尔值（True,False）。")]
        //public bool ContinueOnError { get; set; }

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
        [RequiredArgument]
        [OverloadGroup("G1")]
        [DisplayName("选取器")]
        [Description("用于在执行活动时查找特定用户界面元素的“文本”属性。它实际上是XML片段，用于指定您要查找的图形用户界面元素及其一些父元素的属性。必须将文本放入引号中。")]
        public InArgument<string> Selector { get; set; }
        [Browsable(false)]
        public string SelectorOrigin { get; set; }

        [Category("目标")]
        [RequiredArgument]
        [OverloadGroup("G2")]
        [DisplayName("元素")]
        [Description("使用另一个活动返回的用户界面元素变量， 该属性不能与“选取器”属性一起使用。该字段仅支持用户界面元素变量。")]
        public InArgument<UIElement> ActiveWindow { get; set; }

        #endregion


        #region 属性分类：输入

        private InArgument<Int32> _Occurrence = 1;
        [Category("输入")]
        [DisplayName("出现次数")]
        [Description("如果“文本”字段中的字符串在指定的用户界面元素中出现了不止一次，则在这里指定要单击的“出现”编号。例如，如果字符串出现了 4 次，而您想要单击第一次出现的字符串，则在该字符中写入 1。默认值为 1。")]
        public InArgument<Int32> Occurrence
        {
            get
            {
                return _Occurrence;
            }
            set
            {
                _Occurrence = value;
            }
        }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("文本")]
        [Description("要单击的字符串。必须将文本放入引号中。")]
        public InArgument<String> Text { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("用户界面元素")]
        [Description("您正在查找的字符串所在的用户界面元素。该字段仅支持用户界面元素变量。")]
        public bool UIElement { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("格式化文本")]
        [Description("如果选中此复选框，则保留选定文本的屏幕布局。")]
        public bool Formatted { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string SourceImgPath { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/ClickText/exist.png";
            }
        }

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
        public string DefaultName { get { return "查找文本"; } }

        #endregion


        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            // Do something...

            Thread.Sleep(delayAfter);
        }
    }
}
