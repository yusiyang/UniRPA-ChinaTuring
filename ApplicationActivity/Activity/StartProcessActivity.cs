using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace ApplicationActivity
{
    [Designer(typeof(StartProcessDesigner))]
    public sealed class StartProcessActivity : CodeActivity
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
        [DisplayName("参数")]
        [Description("应用程序启动参数。必须将文本放入引号中。")]
        public InArgument<string> Arguments { get; set; }

        [Category("输入")]
        [DisplayName("文件名")]
        [Description("待打开应用程序的可执行文件的文件名。必须将文本放入引号中。")]
        public InArgument<string> FileName { get; set; }

        [Category("输入")]
        [DisplayName("工作目录")]
        [Description("当前工作目录的路径。必须将文本放入引号中。")]
        public InArgument<string> WorkingDirectory { get; set; }

        [Category("输入")]
        [DisplayName("是否使用系统 Shell 启动")]
        [Description("指示是否使用操作系统 Shell 启动进程。")]
        public bool Default { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Application/start-process.png";
            }
        }

        #endregion


        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            if (Arguments == null && FileName == null)
            {
                metadata.AddValidationError("文件名和参数都为空，至少需要一个不为空。");
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                string _arguments = Arguments.Get(context);
                string _fileName = FileName.Get(context);
                string _workingDirectory = WorkingDirectory.Get(context);
                Process p = new System.Diagnostics.Process();
                if (_arguments != null)
                {
                    p.StartInfo.Arguments = _arguments;
                }
                if (_workingDirectory != null)
                {
                    p.StartInfo.WorkingDirectory = _workingDirectory;
                }
                p.StartInfo.UseShellExecute = Default;
                p.StartInfo.Verb = "Open";
                p.StartInfo.FileName = _fileName;

                p.Start();
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
