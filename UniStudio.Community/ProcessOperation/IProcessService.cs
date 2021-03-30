using UniWorkforce.Process.Models;

namespace UniStudio.Community.ProcessOperation
{
    public interface IProcessService
    {
        Result IsReady();

        Result ConnectToController();

        Result IsLogined();

        Result Login(string loginName, string password);

        Result<CheckProcessInfo> CheckProcess(string processName);

        Result PublishProcess(PublishProcessRequest request);
    }
}
