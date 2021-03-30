using Plugins.Shared.Library.Librarys;
using Plugins.Shared.Library.ProcessOperation;
using UniWorkforce.Process.Interfaces;
using UniWorkforce.Process.Models;

namespace UniStudio.Community.ProcessOperation
{
    public class ProcessService : IProcessService
    {
        private IProcess _processProxy;

        public ProcessService()
        {
            var processInfo = new ProcessModel("UniWorkforce");
            SingleProcess.Start(processInfo, false);

            _processProxy = new ProcessProxy();
        }

        public Result IsReady()
        {
            return ActionHelper.DoFuncInTime(() => _processProxy.IsReady(), result => result.IsSucess,false,20000);
        }

        public Result IsLogined()
        {
            return _processProxy.IsLogined();
        }

        public Result Login(string loginName, string password)
        {
            return _processProxy.Login(loginName, password);
        }

        public Result<CheckProcessInfo> CheckProcess(string processName)
        {
            return _processProxy.CheckProcess(processName);
        }

        public Result PublishProcess(PublishProcessRequest request)
        {
            return _processProxy.PublishProcess(request);
        }

        public Result ConnectToController()
        {
            return _processProxy.ConnectToController();
        }
    }
}
