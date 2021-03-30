using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Core.Interfaces;
using UniExecutor.Core.Models;
using UniNamedPipe;
using UniNamedPipe.Models;

namespace UniExecutor.Proxy
{
    public class ViewOperateProxy : IViewOperate
    {
        private string _apiName = "ViewOperate";

        public NamedPipeClient PipeClient { get; }
        
        public ViewOperateProxy()
        {
            PipeClient= NamedPipeClientManager.Create("localhost", "ViewOperate");
        }

        public void HideLocation()
        {
            var request = new Request(_apiName, "HideLocation", null);
            PipeClient.Send(request);
        }

        public void OutputMessage(OutputMessageModel outputMessageModel)
        {
            var request = new Request(_apiName, "OutputMessage", new object[] { outputMessageModel });
            PipeClient.Send(request);
        }

        public void ShowLocals(LocalsModel localsModel)
        {
            var request = new Request(_apiName, "ShowLocals", new object[] { localsModel });
            PipeClient.Send(request);
        }

        public void ShowLocation(string activityId)
        {
            var request = new Request(_apiName, "ShowLocation", new object[] { activityId });
            PipeClient.Send(request);
        }

        public void SetDebuggingPaused(bool paused)
        {
            var request = new Request(_apiName, "SetDebuggingPaused", new object[] { paused });
            PipeClient.Send(request);
        }

        public void EndRun(int stoppedType, Exception exception = null)
        {
            var request = new Request(_apiName, "EndRun", new object[] { stoppedType,exception });
            PipeClient.Send(request);
        }

        public void InvokeWorkflow(string filePath, Dictionary<string, object> inputArguments, int currentOperate)
        {
            var request = new Request(_apiName, "InvokeWorkflow", new object[] { filePath, inputArguments,currentOperate });
            PipeClient.Send(request);
        }

        public void InternalEndRun(IDictionary<string, object> outputs)
        {
            var request = new Request(_apiName, "InternalEndRun", new object[] { outputs });
            PipeClient.Send(request);
        }
    }
}
