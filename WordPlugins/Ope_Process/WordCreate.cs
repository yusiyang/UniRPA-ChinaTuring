using System.Activities;
using System.ComponentModel;
using System.IO;
using System;
using Word = Microsoft.Office.Interop.Word;
using Plugins.Shared.Library;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library.Exceptions;

namespace WordPlugins
{
    [Designer(typeof(WordCreateDesigner))]
    public sealed class WordCreate : CodeActivity
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

        InArgument<string> _PathUrl;
        [Category("输入")]
        [DisplayName("文件路径")]
        [Description("要打开的Word文档的全路径。必须将文本放入引号中。")]
        public InArgument<string> PathUrl
        {
            get
            {
                return _PathUrl;
            }
            set
            {
                _PathUrl = value;
            }
        }

        #endregion


        #region 属性分类：选项

        bool _NewDoc;
        [Category("选项")]
        [DisplayName("是否创建新文档")]
        public bool NewDoc
        {
            get
            {
                return _NewDoc;
            }
            set
            {
                _NewDoc = value;
                if (_NewDoc)
                    CommonVariable.isNewFile = true;
                else
                    CommonVariable.isNewFile = false;
            }
        }

        bool _IsVisible = true;
        [Category("选项")]
        [DisplayName("流程是否可见")]
        public bool IsVisible
        {
            get
            {
                return _IsVisible;
            }
            set
            {
                _IsVisible = value;
            }
        }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/word/create.png"; } }

        #endregion


        public WordCreate()
        {
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            if (this.NewDoc == false && this.PathUrl == null)
            {
                metadata.AddValidationError("非创建新文档需要添加有效路径");
            }
        }


        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                string filePath = PathUrl.Get(context);
                int VisibleFlag = _IsVisible == true ? 1 : 0;

                CommonVariable.app = new Word::Application();
                CommonVariable.docs = CommonVariable.app.Documents;
                CommonVariable.app.Visible = IsVisible;
                CommonVariable.app.DisplayAlerts = Word.WdAlertLevel.wdAlertsNone;

                if (_NewDoc == true)
                {
                    CommonVariable.doc = CommonVariable.docs.Add();
                }
                else
                {
                    if (!File.Exists(PathUrl.Get(context)))
                    {
                        SharedObject.Instance.Output(SharedObject.OutputType.Error, "文件不存在，请检查路径有效性!");
                        CommonVariable.realaseProcessExit();
                    }
                    CommonVariable.doc = CommonVariable.docs.Open(filePath);
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
