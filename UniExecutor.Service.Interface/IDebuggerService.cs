using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Core.Interfaces;

namespace UniExecutor.Service.Interface
{
    public interface IDebuggerService:IExecutorService
    {
        void StepInto();

        void StepOver();

        void SetSpeed();

        void ToggleBreakpoint();

        void RemoveAllBreakpoints();
    }
}
