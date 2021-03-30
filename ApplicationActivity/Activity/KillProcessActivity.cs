using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ApplicationActivity
{
    [Designer(typeof(ApplicationDesigner))]
    public sealed class KillProcessActivity : CodeActivity
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


        #region 属性分类：目标

        [Category("目标")]
        [DisplayName("进程")]
        [Description("进程类型对象，用于描述要关闭的进程。")]
        public InArgument<Process> Processes { get; set; }

        [Category("目标")]
        [DisplayName("进程名称")]
        [Description("要关闭的进程的名称。必须将文本放入引号中。")]
        public InArgument<string> ProcessName { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Application/kill-process.png";
            }
        }

        #endregion

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            if (Processes == null && ProcessName == null)
            {
                metadata.AddValidationError("进程和进程名称都为空，至少需要一个不为空。");
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);
            
            try
            {
                Process ps = Processes.Get(context);
                string psName = ProcessName.Get(context);
                if (ps != null) ps.Kill();

                if (psName != null)
                {
                    foreach (Process p in Process.GetProcesses())
                    {
                        if (Equals(p.ProcessName, Path.GetFileNameWithoutExtension(psName)))
                        {
                            p.Kill();
                        }
                    }
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
