using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace EnvironmentActivity
{
    [Designer(typeof(GetEnvVarDesigner))]
    public sealed class GetEnvVar : CodeActivity
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
        [RequiredArgument]
        [DisplayName("变量")]
        [Description("要检索其值的环境变量的名称。必须将文本放入引号中。")]
        public InArgument<string> EnvVarName { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [RequiredArgument]
        [DisplayName("变量值")]
        [Description("所选环境变量的值。")]
        public OutArgument<string> EnvVarValue { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Environment/envvariable.png"; } }

        [Browsable(false)]
        public IEnumerable<EnvVarEnums> EnvVarPro
        {
            get
            {
                return Enum.GetValues(typeof(EnvVarEnums)).Cast<EnvVarEnums>();
            }
        }

        #endregion


        //系统变量例举
        public enum EnvVarEnums
        {
            TickCount,
            ExitCode,
            CommandLine,
            CurrentDirectory,
            SystemDirectory,
            MachineName,
            ProcessorCount,
            SystemPageSize,
            NewLine,
            Version,
            WorkingSet,
            OSVersion,
            StackTrace,
            Is64BitProcess,
            Is64BitOperatingSystem,
            HasShutdownStarted,
            UserName,
            UserInteractive,
            UserDomainName,
            CurrentManagedThreadId
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                string envVar = EnvVarName.Get(context);
                string envVarValue = "";
                switch (envVar)
                {
                    case "TickCount":
                        {
                            envVarValue = Environment.TickCount.ToString();
                            break;
                        }
                    case "ExitCode":
                        {
                            envVarValue = Environment.ExitCode.ToString();
                            break;
                        }
                    case "CommandLine":
                        {
                            envVarValue = Environment.CommandLine.ToString();
                            break;
                        }
                    case "CurrentDirectory":
                        {
                            envVarValue = Environment.CurrentDirectory.ToString();
                            break;
                        }
                    case "SystemDirectory":
                        {
                            envVarValue = Environment.SystemDirectory.ToString();
                            break;
                        }
                    case "MachineName":
                        {
                            envVarValue = Environment.MachineName.ToString();
                            break;
                        }
                    case "ProcessorCount":
                        {
                            envVarValue = Environment.ProcessorCount.ToString();
                            break;
                        }
                    case "SystemPageSize":
                        {
                            envVarValue = Environment.SystemPageSize.ToString();
                            break;
                        }
                    case "NewLine":
                        {
                            envVarValue = Environment.NewLine.ToString();
                            break;
                        }
                    case "Version":
                        {
                            envVarValue = Environment.Version.ToString();
                            break;
                        }
                    case "WorkingSet":
                        {
                            envVarValue = Environment.WorkingSet.ToString();
                            break;
                        }
                    case "OSVersion":
                        {
                            envVarValue = Environment.OSVersion.ToString();
                            break;
                        }
                    case "StackTrace":
                        {
                            envVarValue = Environment.StackTrace.ToString();
                            break;
                        }
                    case "Is64BitProcess":
                        {
                            envVarValue = Environment.Is64BitProcess.ToString();
                            break;
                        }
                    case "Is64BitOperatingSystem":
                        {
                            envVarValue = Environment.Is64BitOperatingSystem.ToString();
                            break;
                        }
                    case "HasShutdownStarted":
                        {
                            envVarValue = Environment.HasShutdownStarted.ToString();
                            break;
                        }
                    case "UserName":
                        {
                            envVarValue = Environment.UserName.ToString();
                            break;
                        }
                    case "UserInteractive":
                        {
                            envVarValue = Environment.UserInteractive.ToString();
                            break;
                        }
                    case "UserDomainName":
                        {
                            envVarValue = Environment.UserDomainName.ToString();
                            break;
                        }
                    case "CurrentManagedThreadId":
                        {
                            envVarValue = Environment.CurrentManagedThreadId.ToString();
                            break;
                        }
                    default:
                        {
                            envVarValue = Environment.GetEnvironmentVariable(envVar);
                            break;
                        }
                }
                EnvVarValue.Set(context, envVarValue);
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
