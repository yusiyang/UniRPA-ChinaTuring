using System;
using System.Collections.Generic;
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
    public class ExecutorService:IExecutorService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private JobModel _jobModel;

        private IExecutor _ExecutorProxy;

        public bool IsExecutorRunning { get; private set; }

        public ExecutorService()
        {
            var processInfo = new ProcessInfo
            {
                ProcessName = ViewModelLocator.instance.Project.ProjectName,
                ProcessDir = ViewModelLocator.instance.Project.ProjectPath,
                Main = ViewModelLocator.instance.Dock.ActiveDocument.RelativeXamlPath,
                Dependencies = ViewModelLocator.instance.Project.ProjectDependencies
            };
            _jobModel = new JobModel
            {
                JobName = processInfo.ProcessName,
                ProcessInfo = processInfo
            };
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
                Messenger.Default.Send((IExecutorService)this, "BeginRun");
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

                var processInfo = new ProcessModel("UniExecutor", _jobModel);
                SingleProcess.Start(processInfo);
                Configure.ConfigureServer();
                //UniMessageBox.Show("after StartExecutor");
                IsExecutorRunning = true;

                _ExecutorProxy = new ExecutorProxy();
                ((ExecutorProxy)_ExecutorProxy).PipeClient.PipeServerEnd += PipeClient_PipeServerEnd;

                _ExecutorProxy.Start();
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, logger);
                UniMessageBox.Show(App.Current.MainWindow, ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public void InternalStart(Dictionary<string, object> inputArguments, JobModel jobModel = null)
        {
            _ExecutorProxy.InternalStart(jobModel,inputArguments);
        }
    }
}
