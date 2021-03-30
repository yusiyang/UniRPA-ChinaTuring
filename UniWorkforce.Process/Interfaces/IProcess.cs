using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniWorkforce.Process.Models;

namespace UniWorkforce.Process.Interfaces
{
    public interface IProcess
    {
        Result IsReady();

        Result ConnectToController();

        Result IsLogined();

        Result Login(string loginName, string password);

        Result<CheckProcessInfo> CheckProcess(string processName);

        Result PublishProcess(PublishProcessRequest request);
    }
}
