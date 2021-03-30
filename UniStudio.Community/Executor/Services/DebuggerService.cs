using System;
using System.Activities.Presentation.Debug;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.ProcessOperation;
using UniExecutor.Core.Interfaces;
using UniExecutor.Core.Models;
using UniExecutor.Proxy;
using UniExecutor.Service.Interface;
using UniNamedPipe;
using UniStudio.Community.Executor.Validation;
using UniStudio.Community.Librarys;
using UniStudio.Community.ViewModel;

namespace UniStudio.Community.Executor.Services
{
    public class DebuggerService: IDebuggerService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private JobModel _jobModel;

        private IDebugger _debuggerProxy;

        private DebuggerManager DebuggerManager => DocumentContext.Current.DebuggerManager;

        private bool IsDebugging => ViewModelLocator.instance.Main.IsWorkflowDebugging;

        public bool IsExecutorRunning { get; private set; }

        public DebuggerService()
        {
            var processInfo = new ProcessInfo
            {
                ProcessName = ViewModelLocator.instance.Project.ProjectName,
                ProcessDir = ViewModelLocator.instance.Project.ProjectPath,
                Main = ViewModelLocator.instance.Dock.ActiveDocument.RelativeXamlPath
            };
            _jobModel = new JobModel
            {
                JobName = processInfo.ProcessName,
                ProcessInfo = processInfo
            };
            DebuggerManager.Reset();
        }

        private void StartExecutor()
        {
            var processInfo = new ProcessModel("UniExecutor", _jobModel);
            SingleProcess.Start(processInfo);
            Configure.ConfigureServer();

            //UniMessageBox.Show("after StartExecutor");
            IsExecutorRunning = true;
            _debuggerProxy = new DebuggerProxy();
            ((DebuggerProxy)_debuggerProxy).PipeClient.PipeServerEnd += PipeClient_PipeServerEnd;
        }

        private void PipeClient_PipeServerEnd(object sender, UniNamedPipe.Events.PipeServerEndEventArgs e)
        {
            Messenger.Default.Send(new ViewOperate(), "EndRun");
        }

        public void Pause()
        {
            if (IsDebugging)
            {
                _debuggerProxy?.Pause();
            }
        }

        public void Resume()
        {
            if (IsDebugging)
            {
                _debuggerProxy.Resume();
            }
        }

        public void SetSpeed()
        {
            if (IsDebugging)
            {
                var speedType = ViewModelLocator.instance.Main.SlowStepSpeed.GetHashCode();
                _debuggerProxy.SetSpeed(speedType);
            }
        }

        public void Start()
        {
            Start(StartTypeEnum.Start);
        }

        private void Start(StartTypeEnum startType)
        {
            try
            {
                Messenger.Default.Send((IDebuggerService)this, "BeginRun");
                //授权检测
                if (!ViewModelLocator.instance.SplashScreen.DoAuthorizationCheck())
                {
                    Messenger.Default.Send(new ViewOperate(), "EndRun");
                    return;
                }
                //验证工作流
                if (!WorkflowValidation.Validate(DocumentContext.Current.WorkflowDesigner))
                {
                    Messenger.Default.Send(new ViewOperate(), "EndRun");
                    return;
                }

                DebuggerManager.DebugOperate debugOperate;
                switch (startType)
                {
                    case StartTypeEnum.Start:
                        debugOperate = DebuggerManager.DebugOperate.Null;
                        break;
                    case StartTypeEnum.StepInto:
                        debugOperate = DebuggerManager.DebugOperate.StepInto;
                        break;
                    case StartTypeEnum.StepOver:
                        debugOperate = DebuggerManager.DebugOperate.StepOver;
                        break;
                    default:
                        debugOperate = DebuggerManager.DebugOperate.Null;
                        break;
                }

                StartExecutor();

                var activityIdLocationMapping = DebuggerManager.GetActivityIdLocationMapping(true).ToDictionary(d => d.Key, d => ActivityLocation.FromSourceLocation(d.Value));
                var variableNames = DebuggerManager.GetVariableNames();
                var breakpointLocations = BreakpointsManager.GetBreakpointLocations().ToDictionary(d => ActivityLocation.FromSourceLocation(d.Key), d => d.Value);

                var activityIdLocationBreakpointList = new List<ActivityIdLocationBreakpoint>();
                foreach (var item in activityIdLocationMapping)
                {
                    if (!breakpointLocations.TryGetValue(item.Value, out var breakpointTypes))
                    {
                        breakpointTypes = BreakpointTypes.None;
                    }
                    activityIdLocationBreakpointList.Add(new ActivityIdLocationBreakpoint
                    {
                        ActivityId = item.Key,
                        Location = item.Value,
                        BreakpointTypes = breakpointTypes
                    });
                }
                var debugOptions = new DebugOptions()
                {
                    ActivityIdLocationBreakpointList = activityIdLocationBreakpointList,
                    SpeedType = ViewModelLocator.instance.Main.SlowStepSpeed.GetHashCode(),
                    DebugOperate = debugOperate.GetHashCode(),
                    VariableNames = variableNames
                };

                _debuggerProxy.Debug(debugOptions);
            }
            catch(Exception ex)
            {
                Logger.Debug(ex, logger);
                UniMessageBox.Show(App.Current.MainWindow, ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Messenger.Default.Send(new ViewOperate(), "EndRun");
            }
        }

        public void StepInto()
        {
            if(!IsDebugging)
            {
                Start(StartTypeEnum.StepInto);
            }
            else
            {
                _debuggerProxy.StepInto();
            }
        }

        public void StepOver()
        {
            if (!IsDebugging)
            {
                Start(StartTypeEnum.StepOver);
            }
            else
            {
                _debuggerProxy.StepOver();
            }
        }

        public void Stop()
        {
            if (IsDebugging)
            {
                _debuggerProxy.Stop();
                DebuggerManager.Stop();
            }
        }

        public void ToggleBreakpoint()
        {
            BreakpointsManager.ToggleBreakpoint(ViewModelLocator.instance.Dock.ActiveDocument);
            if (IsDebugging)
            {
                var breakpointLocations = BreakpointsManager.GetBreakpointLocations().ToDictionary(d => ActivityLocation.FromSourceLocation(d.Key), d => d.Value);
                _debuggerProxy.ToggleBreakpoint(breakpointLocations);
            }
        }

        public void RemoveAllBreakpoints()
        {
            BreakpointsManager.RemoveAllBreakpoints(ViewModelLocator.instance.Dock.ActiveDocument);
            if (IsDebugging)
            {
                _debuggerProxy.RemoveAllBreakpoints();
            }
        }

        public void InternalStart(Dictionary<string, object> inputArguments, JobModel jobModel = null)
        {
            if (jobModel == null)
            {
                var xmalPath = ViewModelLocator.instance.Dock.ActiveDocument.XamlPath;
                var processInfo = new ProcessInfo
                {
                    ProcessDir = Path.GetDirectoryName(xmalPath),
                    Main = Path.GetFileName(xmalPath)
                };
                jobModel = new JobModel
                {
                    ProcessInfo = processInfo
                };
            }
            DebugOptions debugOptions;
            if (ViewModelLocator.instance.Dock.ActiveDocument.XamlPath == jobModel.ProcessInfo.MainPath)
            {
                var activityIdLocationMapping = DebuggerManager.GetActivityIdLocationMapping(true).ToDictionary(d => d.Key, d => ActivityLocation.FromSourceLocation(d.Value));
                var variableNames = DebuggerManager.GetVariableNames();
                var breakpointLocations = BreakpointsManager.GetBreakpointLocations().ToDictionary(d => ActivityLocation.FromSourceLocation(d.Key), d => d.Value);

                var activityIdLocationBreakpointList = new List<ActivityIdLocationBreakpoint>();
                foreach (var item in activityIdLocationMapping)
                {
                    if (!breakpointLocations.TryGetValue(item.Value, out var breakpointTypes))
                    {
                        breakpointTypes = BreakpointTypes.None;
                    }
                    activityIdLocationBreakpointList.Add(new ActivityIdLocationBreakpoint
                    {
                        ActivityId = item.Key,
                        Location = item.Value,
                        BreakpointTypes = breakpointTypes
                    });
                }
                debugOptions = new DebugOptions()
                {
                    ActivityIdLocationBreakpointList = activityIdLocationBreakpointList,
                    SpeedType = ViewModelLocator.instance.Main.SlowStepSpeed.GetHashCode(),
                    VariableNames = variableNames,
                    InputArguments = inputArguments
                };
            }
            else
            {
                debugOptions = new DebugOptions()
                {
                    InputArguments = inputArguments,
                    NeedStepInto = false
                };
            }
            _debuggerProxy.InternalDebug(jobModel, debugOptions);
        }
    }

    public enum StartTypeEnum
    {
        Start,
        StepInto,
        StepOver
    }
}
