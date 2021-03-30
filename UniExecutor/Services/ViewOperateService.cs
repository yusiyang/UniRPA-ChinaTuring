using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Core.Events;
using UniExecutor.Core.Interfaces;
using UniExecutor.Core.Models;
using UniExecutor.Proxy;
using UniExecutor.Service.Interface;

namespace UniExecutor.Services
{
    internal class ViewOperateService : IViewOperateService
    {
        private IViewOperate _viewOperateProxy;

        public event InternalEndRunEventHandler InternalEnded;

        public ViewOperateService()
        {
            _viewOperateProxy = new ViewOperateProxy();
            ((ViewOperateProxy)_viewOperateProxy).PipeClient.PipeServerEnd += PipeClient_PipeServerEnd;
        }

        private void PipeClient_PipeServerEnd(object sender, UniNamedPipe.Events.PipeServerEndEventArgs e)
        {
            Environment.Exit(-1);
        }

        public void HideLocation()
        {
            _viewOperateProxy.HideLocation();
        }

        public void OutputMessage(OutputMessageModel outputMessageModel)
        {
            _viewOperateProxy.OutputMessage(outputMessageModel);
        }

        public void ShowLocals(LocalsModel localsModel)
        {
            _viewOperateProxy.ShowLocals(localsModel);
        }

        public void ShowLocation(string activityId)
        {
            _viewOperateProxy.ShowLocation(activityId);
        }

        public void SetDebuggingPaused(bool paused)
        {
            _viewOperateProxy.SetDebuggingPaused(paused);
        }

        public void EndRun(int stoppedType, Exception exception = null)
        {
            _viewOperateProxy.EndRun(stoppedType, exception);
        }

        public void InvokeWorkflow(string filePath, Dictionary<string, object> inputArguments)
        {
            var currentOperate = ExecutorContext.Current.DebuggerManager.NextOperate;
            _viewOperateProxy.InvokeWorkflow(filePath,inputArguments,currentOperate.GetHashCode());
        }

        public void InternalEndRun(IDictionary<string, object> outputs)
        {
            _viewOperateProxy.InternalEndRun(outputs);
            InternalEnded?.Invoke(null, new InternalEndRunEventArgs(outputs));
        }
    }
}
