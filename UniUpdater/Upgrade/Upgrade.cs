using System.IO;
using UniNamedPipe.Attributes;
using UniUpdater.Interfaces;
using UniUpdater.Models;

namespace UniUpdater.Upgrade
{
    [PipeServer("Upgrade", "Upgrade")]
    public class Upgrade : IUpgrade
    {
        private AutoUpgrade _autoUpgrade = new AutoUpgrade();

        public Result IsNeedUpgrade()
        {
            if(_autoUpgrade.IsNeedUpgrade())
            {
                return Result.Success();
            }
            return Result.Fail("不需要更新");
        }

        public Result DoUpgrade()
        {
            _autoUpgrade.DoUpgrade();
            return Result.Success();
        }

        public Result IsReady()
        {
            return Result.Success();
        }
    }
}
