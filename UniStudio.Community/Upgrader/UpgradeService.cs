using Plugins.Shared.Library.Librarys;
using Plugins.Shared.Library.ProcessOperation;
using UniUpdater.Interfaces;
using UniUpdater.Models;

namespace UniStudio.Community.Upgrader
{
    public class UpgradeService : IUpgradeService
    {
        private IUpgrade _upgradeProxy;

        public UpgradeService()
        {
            var processInfo = new ProcessModel("UniUpdater");
            SingleProcess.Start(processInfo, false);

            _upgradeProxy = new UpgradeProxy();
        }

        public Result DoUpgrade()
        {
            var result= _upgradeProxy.DoUpgrade();
            return result;
        }

        public Result IsNeedUpgrade()
        {
            var result= _upgradeProxy.IsNeedUpgrade();
            return result;
        }

        public Result IsReady()
        {
            var response= ActionHelper.DoFuncInTime(() => _upgradeProxy.IsReady(), result => result.IsSucess, false, 20000);
            return response;
        }
    }
}
