using UniNamedPipe;
using UniNamedPipe.Models;
using UniUpdater.Interfaces;
using UniWorkforce.Process.Interfaces;
using UniWorkforce.Process.Models;

namespace UniStudio.Community.Upgrader
{
    public class UpgradeProxy : IUpgrade
    {
        protected NamedPipeClient PipeClient { get; }
        
        public UpgradeProxy()
        {
            PipeClient = NamedPipeClientManager.Create("localhost", "Upgrade");
        }

        public UniUpdater.Models.Result IsReady()
        {
            var request = new Request("Upgrade", "IsReady", null);
            var response = PipeClient.Send(request);
            if (!response.IsSuccess)
            {
                return UniUpdater.Models.Result.Fail<string>(response.Message);
            }
            return response.GetDataResult<UniUpdater.Models.Result>();
        }

        public UniUpdater.Models.Result DoUpgrade()
        {
            var request = new Request("Upgrade", "DoUpgrade", null);
            var response = PipeClient.Send(request);
            if (!response.IsSuccess)
            {
                return UniUpdater.Models.Result.Fail<string>(response.Message);
            }
            return response.GetDataResult<UniUpdater.Models.Result>();
        }

        public UniUpdater.Models.Result IsNeedUpgrade()
        {
            var request = new Request("Upgrade", "IsNeedUpgrade", null);
            var response = PipeClient.Send(request);
            if (!response.IsSuccess)
            {
                return UniUpdater.Models.Result.Fail<string>(response.Message);
            }
            return response.GetDataResult<UniUpdater.Models.Result>();
        }
    }
}
