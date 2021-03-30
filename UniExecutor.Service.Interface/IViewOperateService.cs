using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Core.Events;
using UniExecutor.Core.Models;

namespace UniExecutor.Service.Interface
{
    public interface IViewOperateService
    {
        event InternalEndRunEventHandler InternalEnded; 

        void OutputMessage(OutputMessageModel outputMessageModel);

        void ShowLocation(string activityId);

        void HideLocation();

        void ShowLocals(LocalsModel localsModel);

        void SetDebuggingPaused(bool paused);

        void EndRun(int stoppedType, Exception exception = null);

        void InvokeWorkflow(string filePath, Dictionary<string, object> inputArguments);

        void InternalEndRun(IDictionary<string, object> outputs);
    }
}
