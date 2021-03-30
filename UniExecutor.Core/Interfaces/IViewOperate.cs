using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Core.Models;

namespace UniExecutor.Core.Interfaces
{
    public interface IViewOperate
    {
        void OutputMessage(OutputMessageModel outputMessageModel);

        void ShowLocation(string activityId);

        void HideLocation();

        void ShowLocals(LocalsModel localsModel);

        void SetDebuggingPaused(bool paused);

        void EndRun(int stoppedType, Exception exception = null);

        void InvokeWorkflow(string filePath, Dictionary<string, object> inputArguments,int currentOperate);

        void InternalEndRun(IDictionary<string, object> outputs);
    }
}
