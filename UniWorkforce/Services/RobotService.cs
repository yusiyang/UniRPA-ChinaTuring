using Google.Protobuf;
using Grpc.Core;
using log4net;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.Librarys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using UniWorkforce.Config;
using UniWorkforce.Exceptions;
using UniWorkforce.Librarys;
using UniWorkforce.Models;
using UniWorkforce.Services;
using static UniWorkforce.Services.RobotSDK;

namespace UniWorkforce.Services
{
    public class RobotService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly static string _loginUrl = ControllerSettings.Instance.LoginUrl;
        public readonly static string host = ControllerSettings.Instance.ControllerUrl;
        public readonly static string port = ControllerSettings.Instance.ControllerPort;
        public static Channel channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
        public RobotSDKClient client = new RobotSDKClient(channel);

        #region 发布流程

        public LoginResponse DesignerLogin(LoginRequest request)
        {
            return client.DesignerLogin(request);
        }

        public LoginResponseModel Login(LoginRequestModel request)
        {
            return HttpClientHelper.Post<LoginResponseModel>(_loginUrl, request);
        }

        public CheckProcessResponse CheckProcess(string processName)
        {
            var request = new CheckProcessRequest
            {
                ProcessName = processName,
                SessionId = Context.Current.SessionId
            };
            return client.CheckProcess(request);
        }

        public PublishResponse PublishProcess(PublishRequest request)
        {
            return client.PublishProcess(request);
        }

        #endregion

        /// <summary>
        /// 验证机器码是否存在
        /// </summary>
        /// <returns></returns>
        public CheckRobotResponse CheckRobot()
        {
            var checkRobotRequest = new CheckRobotRequest()
            {
                RobotUniqueNo = Context.Current.RobotUniqueNo,
                LicenseKey = Librarys.Common.GetLisenceKey()
            };
            return client.CheckRobot(checkRobotRequest);
        }

        /// <summary>
        /// 验证机器码是否存在
        /// </summary>
        /// <returns></returns>
        public async Task<CheckRobotResponse> CheckRobotAsync()
        {
            var checkRobotRequest = new CheckRobotRequest()
            {
                RobotUniqueNo = Context.Current.RobotUniqueNo,
                LicenseKey = Librarys.Common.GetLisenceKey()
            };
            return await client.CheckRobotAsync(checkRobotRequest);
        }

        /// <summary>
        /// 批量写入task日志
        /// </summary>
        /// <returns></returns>
        public bool InsertTaskLogList()
        {
            var list = Context.Current.CurrentTaskContext.TaskLogs;
            JobLogRequest jobLogList = new JobLogRequest();
            if (list == null || list.Count < 1)
            {
                return false;
            }
            foreach (var model in list)
            {
                jobLogList.Logs.Add(model);
            }
            var response= client.SendJobLog(jobLogList);
            if(response.Code==ResultCode.SuccessCode)
            {
                return true;
            }
            if (response.Code == ResultCode.UnAuthorizedCode)
            {
                UniMessageBox.Show(App.Current.MainWindow, "未授权", "发送日志失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UniMessageBox.Show(App.Current.MainWindow, response.Message, "发送日志失败", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        /// <summary>
        /// 发送心跳
        /// </summary>
        public bool SendHeartBeat()
        {
            var request = new HeartBeatRequest
            {
                RobotState = Context.Current.RobotState,
                RobotIP = Context.Current.Ip,
                RobotTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                RobotUniqueNo = Context.Current.RobotUniqueNo,
                TaskID=Context.Current.CurrentTaskId??""
            };
            var response = client.HeartBeat(request);
            if (response.Code == ResultCode.SuccessCode)
            {
                if(response.Action=="Stop")
                {
                    Librarys.Common.RunInUI(() =>
                    {
                        Context.Current.WorkforceController.StopProcess();
                    });
                }
                return true;
            }
            if (response.Code == ResultCode.UnAuthorizedCode)
            {
                UniMessageBox.Show(App.Current.MainWindow, "未授权", "发送心跳失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UniMessageBox.Show(App.Current.MainWindow, response.Message, "发送心跳失败", MessageBoxButton.OK, MessageBoxImage.Error);
            Context.Current.StopHeartBeat();
            return false;
        }

        /// <summary>
        /// 获取任务
        /// </summary>
        /// <returns></returns>
        public async Task GetTask()
        {
            Context.Current.StopGetTask();
            var taskFunc = client.GetTask();
            await taskFunc.RequestStream.WriteAsync(new TaskRequest() { RobotUniqueNo = Context.Current.RobotUniqueNo });
            await taskFunc.RequestStream.CompleteAsync();
            while (await taskFunc.ResponseStream.MoveNext())
            {
                var response = taskFunc.ResponseStream.Current;
                if (response.Code == ResultCode.SuccessCode && !string.IsNullOrWhiteSpace(response.Result?.TaskID))
                {
                    Context.Current.CurrentTaskContext = new TaskContext(response.Result.TaskID, response.Result.TaskNo, response.Result.PackageName);
                    Plugins.Shared.Library.Librarys.Common.RunInUI(() =>
                    {
                        Context.Current.WorkforceController.PublishProcess(response.Result);
                        Context.Current.WorkforceController.RunProcess(Context.Current.CurrentTaskContext.ProcessName);
                    });
                    break;
                }
                if (response.Code == ResultCode.SuccessCode)
                {
                    Context.Current.StartGetTask();
                    break;
                }

                if (response.Code == ResultCode.UnAuthorizedCode)
                {
                    UniMessageBox.Show(App.Current.MainWindow, "未授权", "获取任务失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }

                UniMessageBox.Show(App.Current.MainWindow, response.Message, "获取任务失败", MessageBoxButton.OK, MessageBoxImage.Error);
                break;
            }
        }

        /// <summary>
        /// 任务状态变化
        /// </summary>
        public void SendTaskStatus()
        {
            client.SendTaskStatus(new TaskStatusRequest()
            {
                TaskStatus = Context.Current.CurrentTaskContext.TaskStatus,
                TaskID = Context.Current.CurrentTaskId,
                RobotUniqueNo = Context.Current.RobotUniqueNo
            });
        }
    }
}
