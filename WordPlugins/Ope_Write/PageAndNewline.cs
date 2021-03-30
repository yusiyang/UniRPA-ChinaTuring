using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;

namespace WordPlugins
{
    [Designer(typeof(PageAndNewlineDesigner))]
    public sealed class PageAndNewline : CodeActivity
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

        InArgument<Int32> _OperateCount = 1;
        [Category("输入")]
        [DisplayName("分页/换行次数")]
        [Description("分页/换行次数。请输入一个整数。")]
        public InArgument<Int32> OperateCount
        {
            get
            {
                return _OperateCount;
            }
            set
            {
                _OperateCount = value;
            }
        }

        #endregion


        #region 属性分类：选项

        bool _PageBreak;
        [Category("选项")]
        [DisplayName("分页")]
        public bool PageBreak
        {
            get
            {
                return _PageBreak;
            }
            set
            {
                _PageBreak = value;
            }
        }

        bool _NewLine;
        [Category("选项")]
        [DisplayName("换行")]
        public bool NewLine
        {
            get
            {
                return _NewLine;
            }
            set
            {
                _NewLine = value;
            }
        }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/word/newline.png"; } }

        #endregion


        public PageAndNewline()
        {
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                Int32 operateCount = OperateCount.Get(context);
                CommonVariable.sel = CommonVariable.app.Selection;

                if (_NewLine)
                {
                    for (int i = 0; i < operateCount; i++)
                    {
                        CommonVariable.sel.TypeParagraph();
                    }
                }

                if (_PageBreak)
                {
                    for (int i = 0; i < operateCount; i++)
                    {
                        CommonVariable.sel.InsertBreak();
                    }
                }
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                CommonVariable.realaseProcessExit();
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            Thread.Sleep(delayAfter);
        }
    }
}
