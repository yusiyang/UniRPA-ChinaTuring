using Plugins.Shared.Library.Extensions;
using System;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Presentation;
using System.Activities.Presentation.Debug;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using UniExecutor;
using UniExecutor.Core.Interfaces;
using UniExecutor.Core.Models;
using UniExecutor.Enums;
using UniExecutor.Executor;
using UniNamedPipe.Attributes;

namespace UniStudio.Executor.Debugger
{
    [PipeServer("Executor", "Debugger")]
    public class Debugger : IDebugger
    {
        private ExecutorContext Context => ExecutorContext.Current;

        private WorkflowExecutor WorkflowExecutor => ExecutorContext.Current.WorkflowExecutor;

        /// <summary>
        /// 开始执行运行流程
        /// </summary>
        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Debug(DebugOptions debugOptions)
        {
            Context.ActivityIdLocationDic = debugOptions.ActivityIdLocationBreakpointList?.ToDictionary(a => a.ActivityId, a => a.Location.ToSourceLocation());
            Context.VariableNames = debugOptions.VariableNames;
            Context.LocationBreakpointDic = debugOptions.ActivityIdLocationBreakpointList.ToDictionary(a => a.Location.ToSourceLocation(), a => a.BreakpointTypes);
            Context.DebuggerManager.SetSpeed((DebugSpeed)debugOptions.SpeedType);
            Context.DebuggerManager.NextOperate = (DebugOperate)debugOptions.DebugOperate;
            Context.DebuggerManager.Debug();
        }

        public void Stop()
        {
            WorkflowExecutor.Stop();
        }

        public void Pause()
        {
            Context.DebuggerManager.Break();
        }

        public void Resume()
        {
            Context.DebuggerManager.Continue();
        }



        public void StepInto()
        {
            Context.DebuggerManager.Continue(DebugOperate.StepInto);
        }

        public void StepOver()
        {
            Context.DebuggerManager.Continue(DebugOperate.StepOver);
        }

        public void SetSpeed(int speedLevel)
        {
            Context.DebuggerManager.SetSpeed((DebugSpeed)speedLevel);
        }

        public void ToggleBreakpoint(IDictionary<ActivityLocation, BreakpointTypes> locationBreakpointDic)
        {
            Context.LocationBreakpointDic = locationBreakpointDic.ToDictionary(d=>d.Key.ToSourceLocation(),d=>d.Value);
        }

        public void RemoveAllBreakpoints()
        {
            Context.LocationBreakpointDic = null;
        }

        public void InternalDebug(JobModel jobModel, DebugOptions debugOptions)
        {
            var nextOperate = ExecutorContext.Current.DebuggerManager.NextOperate;
            new ExecutorContext(jobModel);
            Context.ActivityIdLocationDic = debugOptions.ActivityIdLocationBreakpointList?.ToDictionary(a => a.ActivityId, a => a.Location.ToSourceLocation());
            Context.VariableNames = debugOptions.VariableNames;
            Context.LocationBreakpointDic = debugOptions.ActivityIdLocationBreakpointList?.ToDictionary(a => a.Location.ToSourceLocation(), a => a.BreakpointTypes);
            Context.DebuggerManager.SetSpeed((DebugSpeed)debugOptions.SpeedType);
            Context.InputArguments = debugOptions.InputArguments;
            Context.DebuggerManager.NextOperate = nextOperate;
            Context.NeedStepInto = debugOptions.NeedStepInto;
            Context.IsInternalExecutor = true;
            Context.DebuggerManager.Debug();
        }

        public void InternalStart(JobModel jobModel, Dictionary<string, object> inputArguments)
        {
            throw new NotImplementedException();
        }
    }
}
