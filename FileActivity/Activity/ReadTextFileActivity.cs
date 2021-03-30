using MouseActivity;
using Plugins.Shared.Library;
using System;
using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.ComponentModel;
using System.IO;
using System.Threading;
using FileActivity.Activity;
using Plugins.Shared.Library.Exceptions;

namespace FileActivity
{
    [Designer(typeof(ReadTextFileDesigner))]
    public sealed class ReadTextFileActivity : CodeActivity
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


        #region 属性分类：文件

        [Category("文件")]
        [DisplayName("编码")]
        [Editor(typeof(PropertyEncodingEditor),typeof(PropertyValueEditor))]
        //public InArgument<string> Encoding { get; set; }
        public string Encoding { get; set; }
        
        [Category("文件")]
        [RequiredArgument]
        [DisplayName("文件路径")]
        [Description("要读取的文件的全路径。必须将文本放入引号中。")]
        public InArgument<string> FileName { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("内容")]
        [Description("将从文件中提取的文本存储到字符串变量。")]
        public OutArgument<string> Content { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/file/read.png";
            }
        }

        #endregion


        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            string filePath = FileName.Get(context);
            string EncodingName = Encoding;

            if (string.IsNullOrWhiteSpace(EncodingName))
            {
                EncodingName = "UTF-8";
            }
            try
            {
                using (StreamReader sr = new StreamReader(filePath, System.Text.Encoding.GetEncoding(EncodingName)))
                {
                    string fileContent = sr.ReadToEnd();
                    Content.Set(context,fileContent);
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
