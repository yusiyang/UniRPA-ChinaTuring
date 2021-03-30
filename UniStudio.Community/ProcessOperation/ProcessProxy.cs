using UniNamedPipe;
using UniNamedPipe.Models;
using UniWorkforce.Process.Interfaces;
using UniWorkforce.Process.Models;

namespace UniStudio.Community.ProcessOperation
{
    public class ProcessProxy:IProcess
    {
        protected NamedPipeClient PipeClient { get; }
        
        public ProcessProxy()
        {
            PipeClient = NamedPipeClientManager.Create("localhost", "Process");
        }

        public Result IsReady()
        {
            var request = new Request("Process", "IsReady", null);
            var response = PipeClient.Send(request);
            if (!response.IsSuccess)
            {
                return Result.Fail<string>(response.Message);
            }
            return response.GetDataResult<Result>();
        }

        public Result<CheckProcessInfo> CheckProcess(string processName)
        {
            var request = new Request("Process", "CheckProcess", new object[] { processName });
            var response = PipeClient.Send(request);
            return response.GetDataResult<Result<CheckProcessInfo>>();
        }

        public Result PublishProcess(PublishProcessRequest request)
        {
            var requestObj = new Request("Process", "PublishProcess", new object[] { request });
            var response = PipeClient.Send(requestObj);
            if (!response.IsSuccess)
            {
                return Result.Fail<string>(response.Message);
            }
            return response.GetDataResult<Result>();
        }

        public Result IsLogined()
        {
            var request = new Request("Process", "IsLogined",null);
            var response = PipeClient.Send(request);
            if (!response.IsSuccess)
            {
                return Result.Fail<string>(response.Message);
            }
            return response.GetDataResult<Result>();
        }

        public Result Login(string loginName, string password)
        {
            var request = new Request("Process", "Login", new object[] { loginName,password });
            var response = PipeClient.Send(request);
            if (!response.IsSuccess)
            {
                return Result.Fail<string>(response.Message);
            }
            return response.GetDataResult<Result>();
        }

        public Result ConnectToController()
        {
            var request = new Request("Process", "ConnectToController", null);
            var response = PipeClient.Send(request);
            if (!response.IsSuccess)
            {
                return Result.Fail<string>(response.Message);
            }
            return response.GetDataResult<Result>();
        }
    }
}
