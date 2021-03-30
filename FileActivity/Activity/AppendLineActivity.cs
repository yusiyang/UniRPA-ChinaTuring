using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace FileActivity
{
    [Designer(typeof(AppendLineDesigner))]
    public sealed class AppendLineActivity : CodeActivity
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
        public InArgument<string> Encoding { get; set; }

        [Category("文件")]
        [RequiredArgument]
        [DisplayName("文件路径")]
        [Description("文件的全路径。必须将文本放入引号中。")]
        public InArgument<string> FileName { get; set; }

        #endregion


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("文本")]
        [Description("要写入文件的文本。必须将文本放入引号中。")]
        public InArgument<string> Text { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/file/appline.png";
            }
        }

        #endregion


        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            string filePath = FileName.Get(context);
            string fileContent = Text.Get(context);
            string EncodingName = Encoding.Get(context);

            if(string.IsNullOrWhiteSpace(EncodingName))
            {
                EncodingName = "UTF-8";
            }
            try
            {
                //  byte[] contentByte = System.Text.Encoding.UTF8.GetBytes(fileContent);
                using (FileStream fsWrite = new FileStream(filePath, FileMode.Append))
                {
                    StreamWriter sw = new StreamWriter(fsWrite, System.Text.Encoding.GetEncoding(EncodingName));
                    sw.WriteLine(fileContent);
                    sw.Flush();
                    sw.Close();
                    fsWrite.Close();     
                };
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
