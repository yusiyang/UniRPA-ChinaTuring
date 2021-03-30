using GalaSoft.MvvmLight.Messaging;
using log4net;
using Newtonsoft.Json;
using Plugins.Shared.Library;
using Plugins.Shared.Library.ProcessOperation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Core.Interfaces;
using UniExecutor.Core.Models;
using UniExecutor.Proxy;
using UniExecutor.Service.Interface;
using UniNamedPipe;
using UniWorkforce.Librarys;
using UniWorkforce.ViewModel;

namespace UniWorkforce.Executor
{
    public class ExecutorService:IExecutorService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private JobModel _jobModel;

        private IExecutor _ExecutorProxy;

        public ExecutorService(JobModel jobModel)
        {
            _jobModel = jobModel;
        }

        public bool IsExecutorRunning { get; private set; }

        public void InternalStart(Dictionary<string, object> inputArguments, JobModel jobModel = null)
        {
            _ExecutorProxy.InternalStart(jobModel, inputArguments);
        }

        public void Pause()
        {
            _ExecutorProxy.Pause();
        }

        public void Resume()
        {
            _ExecutorProxy.Resume();
        }

        public void Start()
        {
            try
            {
                Messenger.Default.Send(this, "BeginRun");

                var processInfo = new ProcessModel("UniExecutor", _jobModel);
                SingleProcess.Start(processInfo);
                Configure.ConfigureServer();

                _ExecutorProxy = new ExecutorProxy();
                ((ExecutorProxy)_ExecutorProxy).PipeClient.PipeServerEnd += PipeClient_PipeServerEnd;

                _ExecutorProxy.Start();
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, logger);
                Messenger.Default.Send(new ViewOperate(), "EndRun");
            }
        }

        private void PipeClient_PipeServerEnd(object sender, UniNamedPipe.Events.PipeServerEndEventArgs e)
        {
            Messenger.Default.Send(new ViewOperate(), "EndRun");
        }

        public void Stop()
        {
            _ExecutorProxy.Stop();
        }
    }
}
