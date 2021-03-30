using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.Extensions;
using System;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Presentation.Debug;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NuGet.Packaging.Core;
using Plugins.Shared.Library.Nuget;
using UniExecutor.Core.Models;
using UniExecutor.Enums;
using UniExecutor.Output;
using UniExecutor.Service.Interface;
using UniExecutor.Services;
using UniExecutor.Tracking;
using UniExecutor.View;
using UniNamedPipe;
using NuGet.Versioning;
using Newtonsoft.Json;
using Plugins.Shared.Library.UiAutomation;

namespace UniExecutor.Executor
{
    public class WorkflowExecutor
    {
        public ProcessInfo ProcessInfo { get; }

        private IViewOperateService _viewOperateService;

        private Activity _workflow;

        public Activity Workflow => _workflow;

        public IViewOperateService ViewOperateService
        {
            get
            {
                if (_viewOperateService == null)
                {
                    _viewOperateService = ExecutorContext.Current.ViewOperateService;
                }
                return _viewOperateService;
            }
        }

        public WorkflowExecutor(ProcessInfo processInfo)
        {
            ProcessInfo = processInfo;
            if (processInfo.Dependencies.Any())
            {
                InitDependencyAsync(processInfo.Dependencies).Wait();
            }
            var xmalPath = ProcessInfo.MainPath;
            _workflow = ActivityXamlServices.Load(xmalPath);
        }

        private async Task InitDependencyAsync(Dictionary<string, string> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                string packageId = dependency.Key;
                string packageVersion = dependency.Value.Replace('[', ' ').Replace(']', ' ').Trim();
                await NuGetPackageController.Instance.DownloadAndInstall(new PackageIdentity(packageId, NuGetVersion.Parse(packageVersion)));
            }
        }

        public void EnsureValid()
        {
            var result = ActivityValidationServices.Validate(Workflow);
            if (result?.Errors?.Count > 0)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, "过程验证不通过，请检查");
                App.Current.Dispatcher.Invoke(DispatcherPriority.Render
                , (Action)(() =>
                {
                    UniMessageBox.Show("工作流校验错误，请检查参数配置", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                }));
                Stop();
            }
        }

        private WorkflowApplication _workflowApplication;

        public WorkflowApplication WorkflowApplication => _workflowApplication;

        public bool IsRunning { get; private set; }

        public void Run(Dictionary<string, object> inputs, params object[] extensions)
        {
            EnsureValid();
            IsRunning = true;

            if (inputs != null && inputs.Any())
            {
                var sourceInput = new Dictionary<string, object>();
                foreach (var input in inputs)
                {
                    if (input.Key.Contains("|"))
                    {
                        var typeName = input.Key.Split('|').Last();
                        var type = Type.GetType(typeName);
                        if (type == null)
                        {
                            continue;
                        }
                        var inputName = input.Key.Split('|').First();
                        object value;
                        if (type==typeof(UiElement))
                        {
                            if (input.Value==null)
                            {
                                value = null;
                            }
                            else
                            {
                                value = UiElement.FromGlobalId(input.Value.ToString());
                            }
                        }
                        else
                        {
                            value = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(input.Value), type);
                        }
                        
                        sourceInput.Add(inputName, value);
                    }
                    else
                    {
                        sourceInput.Add(input.Key,input.Value);
                    }
                }
                inputs = sourceInput;
            }

            _workflowApplication = inputs == null ? new WorkflowApplication(Workflow) : new WorkflowApplication(Workflow, inputs);

            if (extensions?.Length > 0)
            {
                foreach (var extension in extensions)
                {
                    WorkflowApplication.Extensions.Add(extension);
                }
            }

            WorkflowApplication.Extensions.Add(new OutputTextWriter());

            if (Workflow is DynamicActivity)
            {
                var wr = new WorkflowRuntime();
                wr.RootActivity = Workflow;
                WorkflowApplication.Extensions.Add(wr);
            }

            WorkflowApplication.OnUnhandledException = WorkflowApplicationOnUnhandledException;
            WorkflowApplication.Completed = WorkflowApplicationExecutionCompleted;

            Environment.CurrentDirectory = SharedObject.Instance.ProjectPath;
            WorkflowApplication.Run();
        }

        public void Stop(bool forceStop = true, IDictionary<string, object> outputs = null, Exception exception = null)
        {
            if (outputs!=null &&outputs.Any())
            {
                var newOutputs=new Dictionary<string,object>();
                foreach (var output in outputs)
                {
                    if (output.Value?.GetType() == typeof(UiElement))
                    {
                        newOutputs.Add(output.Key + "|"+output.Value.GetType().AssemblyQualifiedName, (output.Value as UiElement)?.GlobalId);
                    }
                    else
                    {
                        newOutputs.Add(output.Key,output.Value);
                    }
                }

                outputs = newOutputs;
            }

            if (!forceStop)
            {
                var speedType = ExecutorContext.Current.DebuggerManager.SpeedType;
                var nextOperate = ExecutorContext.Current.DebuggerManager.NextOperate;
                _viewOperateService = ExecutorContext.Current.ViewOperateService;
                ExecutorContext.Current.Remove();
                if (ExecutorContext.Current != null)
                {
                    ExecutorContext.Current.DebuggerManager.SetSpeed(speedType);
                    ExecutorContext.Current.DebuggerManager.NextOperate = nextOperate;
                    ExecutorContext.Current.ViewOperateService.InternalEndRun(outputs);
                    return;
                }
            }
            IsRunning = false;
            StoppedType stoppedType = StoppedType.Normal;
            if (forceStop)
            {
                if (exception == null)
                {
                    stoppedType = StoppedType.Force;
                }
                else
                {
                    stoppedType = StoppedType.Exception;
                }
            }

            Task.Run(() =>
            {
                ViewOperateService.EndRun(stoppedType.GetHashCode(), exception);
                NamedPipeServerManager.Clear();
                NamedPipeClientManager.Clear();
            }).ContinueWith(t =>
            {
                try
                {
                    Application.Current.Shutdown();
                }
                finally
                {
                    Process.GetCurrentProcess().Kill();
                }
            });
            
            //Process.GetCurrentProcess().Kill();
        }

        private UnhandledExceptionAction WorkflowApplicationOnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs e)
        {
            var name = e.ExceptionSource.DisplayName;
            SharedObject.Instance.Output(SharedObject.OutputType.Error, string.Format("{0} 执行时出现异常", name), e.UnhandledException.ToString());

            return UnhandledExceptionAction.Terminate;
        }

        private void WorkflowApplicationExecutionCompleted(WorkflowApplicationCompletedEventArgs obj)
        {
            if (obj.TerminationException != null)
            {
                if (!string.IsNullOrEmpty(obj.TerminationException.Message))
                {
                    SharedObject.Instance.Output(SharedObject.OutputType.Error, "运行时执行错误", obj.TerminationException.ToString());
                    App.Current.Dispatcher.Invoke(DispatcherPriority.Render, (Action)(() =>
                    {
                        RuntimeErrorDialogs window;
                        if (obj.TerminationException.GetType().Equals(typeof(ActivityRuntimeException)))
                        {
                            // 是活动 Executor 抛出的异常，特别处理
                            window = new RuntimeErrorDialogs(obj.TerminationException.Message, obj.TerminationException.GetBaseException().Message, obj.TerminationException.GetBaseException().GetType().FullName, obj.TerminationException.ToString());
                        }
                        else
                        {
                            window = new RuntimeErrorDialogs(obj.TerminationException.Source, obj.TerminationException.Message, obj.TerminationException.GetType().FullName, obj.TerminationException.ToString());
                        }
                        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        window.Topmost = true;
                        window.ShowDialog();
                        window.Activate();
                        window.Focus();
                    }));

                }
                Stop(true, null, obj.TerminationException);
            }
            else
            {
                Stop(false, obj.Outputs);
            }
        }
    }
}
