using ApplicationActivity.Properties;
using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.Activities.Statements;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;


namespace ApplicationActivity
{
    [Designer(typeof(OpenApplicationDesigner))]
    public sealed class OpenApplicationActivity : NativeActivity
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
        [OverloadGroup("G1")]
        [DisplayName("超时（毫秒）")]
        [Description("指定最长等待时间（以毫秒为单位），如果超出该时间活动未运行，系统就会报错。默认值为30000毫秒（30秒）。")]
        public InArgument<int> Timeout { get; set; }

        [Category("输入")]
        [DisplayName("选取器")]
        [Description("用于在执行活动时查找特定用户界面元素的“文本”属性。它实际上是XML片段，用于指定您要查找的图形用户界面元素及其一些父元素的属性。必须将文本放入引号中。")]
        [RequiredArgument]
        public InArgument<string> Selector { get; set; }
        [Browsable(false)]
        public string SelectorOrigin { get; set; }

        [Category("输入")]
        [DisplayName("参数")]
        [Description("可以在启动时传递给应用程序的参数。必须将文本放入引号中。")]
        public InArgument<string> Arguments { get; set; }

        [Category("输入")]
        [DisplayName("文件路径")]
        [Description("可以找到要打开的应用程序的可执行文件的全路径。必须将文本放入引号中。")]
        public InArgument<string> ProcessPath { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("工作目录")]
        [Description("当前工作目录的路径。这个字段只接受字符串变量。必须将文本放入引号中。")]
        public InArgument<string> WorkingDirectory { get; set; }

        #endregion

        #region 属性分类：杂项

        [Browsable(false)]
        public string DefaultName { get { return "打开应用程序"; } }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public ActivityAction<object> Body { get; set; }

        [Browsable(false)]
        public string SourceImgPath { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Application/open-application.png";
            }
        }

        #endregion


        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            if (Arguments == null && ProcessPath == null)
            {
                metadata.AddValidationError("文件名和参数都为空，至少需要一个不为空。");
            }
        }

        public OpenApplicationActivity()
        {
            Body = new ActivityAction<object>
            {
                Handler = new Sequence()
                {
                    DisplayName = Resources.Do
                }
            };
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

        protected override void Execute(NativeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                string _arguments = Arguments.Get(context);
                string _fileName = ProcessPath.Get(context);
                string _workingDirectory = WorkingDirectory.Get(context);
                Int32 _timeout = Timeout.Get(context);
                Process p = new System.Diagnostics.Process();
                if (_arguments != null)
                {
                    p.StartInfo.Arguments = _arguments;
                }
                if (_workingDirectory != null)
                {
                    p.StartInfo.WorkingDirectory = _workingDirectory;
                }
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = _fileName;
                p.Start();
                Thread.Sleep(_timeout);
                context.ScheduleAction(Body, "", OnCompleted, OnFaulted);
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

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            //  faultContext.CancelChildren();
            // Cleanup();
        }

        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            //Cleanup();
        }
    }
}
