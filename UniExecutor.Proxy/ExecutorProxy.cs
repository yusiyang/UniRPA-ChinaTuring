using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Core;
using UniExecutor.Core.Interfaces;
using UniExecutor.Core.Models;
using UniNamedPipe;
using UniNamedPipe.Models;

namespace UniExecutor.Proxy
{
    public class ExecutorProxy : IExecutor
    {
        public NamedPipeClient PipeClient { get; }

        public ExecutorProxy()
        {
            PipeClient = NamedPipeClientManager.Create("localhost", "Executor");
        }

        protected virtual string ApiName => "Executor";

        public void Pause()
        {
            var request = new Request(ApiName, "Pause",null);
            PipeClient.Send(request);
        }

        public void Resume()
        {
            var request = new Request(ApiName, "Resume", null);
            PipeClient.Send(request);
        }

        public void Start()
        {
            var request = new Request(ApiName, "Start", null);
            PipeClient.Send(request);
        }

        public void Stop()
        {
            var request = new Request(ApiName, "Stop", null);
            PipeClient.Send(request);
        }

        public void InternalStart(JobModel jobModel, Dictionary<string, object> inputArguments)
        {
            var request = new Request(ApiName, "InternalStart", new object[] { jobModel,inputArguments });
            PipeClient.Send(request);
        }
    }
}
