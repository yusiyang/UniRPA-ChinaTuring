using System;
using System.Activities.Debugger;
using System.Activities.Presentation.Debug;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Core.Models;

namespace UniExecutor.Core.Interfaces
{
    public interface IDebugger:IExecutor
    {
        void Debug(DebugOptions debugOptions);

        void InternalDebug(JobModel jobModel,DebugOptions debugOptions);

        void StepInto();

        void StepOver();

        void SetSpeed(int speedLevel);

        void ToggleBreakpoint(IDictionary<ActivityLocation, BreakpointTypes> locationBreakpointDic);

        void RemoveAllBreakpoints();
    }
}
