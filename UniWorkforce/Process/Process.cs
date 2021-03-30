using GalaSoft.MvvmLight.Messaging;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UniNamedPipe.Attributes;
using UniWorkforce.Librarys;
using UniWorkforce.Models;
using UniWorkforce.Models.MessageModels;
using UniWorkforce.Process.Interfaces;
using UniWorkforce.Process.Models;
using UniWorkforce.Services;
using UniWorkforce.ViewModel;
using UniWorkforce.Windows;

namespace UniWorkforce.Process
{
    [PipeServer("Process", "Process")]
    public class Process : IProcess
    {
        private RobotService RobotService => Context.Current.RobotService;

        public Result IsReady()
        {
            return Result.Success();
        }

        public Result<CheckProcessInfo> CheckProcess(string processName)
        {
            var response = RobotService.CheckProcess(processName);
            if (response.Code == ResultCode.SuccessCode)
            {
                if (!response.Result.IsExist)
                {
                    return Result.Success(new CheckProcessInfo(response.Code.GetHashCode(), "0.0.0", response.Result.RobotTypeID, response.Result.RobotType));
                }
                return Result.Success(new CheckProcessInfo(response.Code.GetHashCode(), response.Result.Version, response.Result.RobotTypeID, response.Result.RobotType));
            }
            return Result.Success(new CheckProcessInfo() { Code = response.Code.GetHashCode(), Message = response.Message });
        }

        public Result ConnectToController()
        {
            if (Context.Current.ConnectedToController)
            {
                return Result.Success();
            }
            var response = RobotService.CheckRobot();
            if (response.Code == ResultCode.SuccessCode)
            {
                Messenger.Default.Send(new ConnectedToControllerMessage(response.Result.RobotId), "ConnectedToController");
                return Result.Success();
            }
            if (response.Code == ResultCode.UnAuthorizedCode)
            {
                return Result.Fail<string>("未授权");
            }
            return Result.Fail(response.Message);
        }

        public Result IsLogined()
        {
            if (string.IsNullOrWhiteSpace(Context.Current.SessionId))
            {
                return Result.Fail("未登陆");
            }
            return Result.Success();
        }

        public Result Login(string loginName, string password)
        {
            if (!Context.Current.ConnectedToController)
            {
                return Result.Fail("未连接到控制器，不能登陆");
            }

            var request = new LoginRequestModel
            {
                UserName = loginName,
                Password = password
            };
            var response = RobotService.Login(request);

            if (response.Code != 200)
            {
                return Result.Fail(response.Message);
            }

            var loggedInMessage = new LoggedInMessage(response.Data);
            Messenger.Default.Send(loggedInMessage, "LoggedIn");

            return Result.Success();
        }

        public Result PublishProcess(PublishProcessRequest request)
        {
            using (var fileStream = new FileStream(request.FilePath, FileMode.Open))
            {
                var publishRequest = new PublishRequest
                {
                    SessionId = Context.Current.SessionId,
                    ProcessName = request.ProcessName,
                    ProcessDescription = request.ProcessDescription,
                    FileStream = ByteString.FromStream(fileStream),
                    Version = request.Version,
                    PublishTime = request.PublishTime.ToString("yyyy-MM-dd HH:mm:ss"),
                };
                var response = RobotService.PublishProcess(publishRequest);

                if (response.Code == ResultCode.SuccessCode)
                {
                    return Result.Success();
                }
                if (response.Code == ResultCode.LoginOutTimeCode)
                {
                    return Result.Fail<string>("登陆已过期");
                }
                if (response.Code == ResultCode.UnAuthorizedCode)
                {
                    return Result.Fail<string>("未授权");
                }
                return Result.Fail<string>(response.Message);
            }
        }
    }
}
