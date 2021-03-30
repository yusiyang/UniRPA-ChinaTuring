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
    [Designer(typeof(CreateDirectoryDesigner))]
    public sealed class MoveFileActivity : CodeActivity
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


        #region 属性分类：来源

        [Category("来源")]
        [RequiredArgument]
        [DisplayName("源文件路径")]
        [Description("要移动的文件全路径。必须将文本放入引号中。")]
        public InArgument<string> Path { get; set; }

        #endregion


        #region 属性分类：目标

        [Category("目标")]
        [RequiredArgument]
        [DisplayName("目标文件夹")]
        [Description("要将文件移动到的目标文件夹路径。必须将文本放入引号中。")]
        public InArgument<string> Destination { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("覆盖")]
        [Description("选中后，要复制的文件将覆盖目标文件夹中的文件。")]
        public bool Overwrite { get; set; }

        [Category("选项")]
        [DisplayName("新文件名")]
        [Description("目标文件的文件名，默认为源文件名。必须将文本放入引号中。")]
        public InArgument<string> NewFileName { get; set; }


        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/file/movefile.png";
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
                string filePath = Path.Get(context);
                FileInfo file = new FileInfo(filePath);
                string desFilePath = Destination.Get(context);
                string DestinationPath = desFilePath;
                string newFileName = NewFileName.Get(context);
                FileInfo desFile = new FileInfo(desFilePath);

                if (string.IsNullOrEmpty(newFileName))
                {
                    newFileName = file.Name;
                }

                DestinationPath = desFilePath + '\\' + newFileName;

                if (file.Exists)
                {
                    if(File.Exists(DestinationPath))
                    {
                        if(Overwrite)
                        {
                            File.Delete(DestinationPath);
                        }
                        else
                        {
                            throw new Exception("目标文件已存在");
                        }
                    }
                    file.MoveTo(DestinationPath);    
                }
                else
                {
                    throw new Exception("源文件不存在");
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
