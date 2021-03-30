using Newtonsoft.Json;
using System;
using System.Activities.Debugger;
using System.Activities.Presentation.Debug;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Core.Interfaces;
using UniExecutor.Core.Models;
using UniNamedPipe.Models;

namespace UniExecutor.Proxy
{
    public class DebuggerProxy : ExecutorProxy, IDebugger
    {
        protected override string ApiName => "Debugger";

        public void StepInto()
        {
            var request = new Request(ApiName, "StepInto", null);
            PipeClient.Send(request);
        }

        public void StepOver()
        {
            var request = new Request(ApiName, "StepOver", null);
            PipeClient.Send(request);
        }

        public void SetSpeed(int speedLevel)
        {
            var request = new Request(ApiName, "SetSpeed", new object[] { speedLevel});
            PipeClient.Send(request);
        }

        public void Debug(DebugOptions debugOptions)
        {
            var request = new Request(ApiName, "Debug", new object[] { debugOptions });
            PipeClient.Send(request);
        }

        public void ToggleBreakpoint(IDictionary<ActivityLocation, BreakpointTypes> locationBreakpointDic)
        {
            var request = new Request(ApiName, "ToggleBreakpoint", new object[] { locationBreakpointDic });
            PipeClient.Send(request);
        }

        public void RemoveAllBreakpoints()
        {
            var request = new Request(ApiName, "RemoveAllBreakpoints",null);
            PipeClient.Send(request);
        }

        public void InternalDebug(JobModel jobModel, DebugOptions debugOptions)
        {
            var request = new Request(ApiName, "InternalDebug", new object[] { jobModel, debugOptions });
            PipeClient.Send(request);
        }
    }
}
