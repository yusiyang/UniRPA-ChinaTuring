using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniUpdater.Models;

namespace UniUpdater.Interfaces
{
    public interface IUpgrade
    {
        Result IsReady();

        Result DoUpgrade();

        Result IsNeedUpgrade();
    }
}
