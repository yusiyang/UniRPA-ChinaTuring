using UniUpdater.Models;

namespace UniStudio.Community.Upgrader
{
    public interface IUpgradeService
    {
        Result IsReady();

        Result DoUpgrade();

        Result IsNeedUpgrade();
    }
}
