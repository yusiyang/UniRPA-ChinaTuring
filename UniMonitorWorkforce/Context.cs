using GalaSoft.MvvmLight.Messaging;
using Plugins.Shared.Library.Librarys;
using System;
using System.Activities.Presentation;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UniNamedPipe;
using UniWorkforce.Config;
using UniWorkforce.Events;
using UniWorkforce.Models.MessageModels;
using UniWorkforce.Services;

namespace UniWorkforce
{
    public enum ConnectedState
    {
        Disconnected,
        Connected
    }

    public class Context
    {
        private static bool _isCreated = false;

        private static Context _instance;

        public static Context Current => _instance;

        public event ConnectedStateChangedEventHandler ConnectedStateChanged;

        private RobotService _robotService;

        /// <summary>
        /// 服务
        /// </summary>
        public RobotService RobotService
        {
            get
            {
                if (_robotService == null)
                {
                    _robotService = new RobotService();
                }
                return _robotService;
            }
        }

        private WorkforceController _workforceController;

        /// <summary>
        /// 控制器操作
        /// </summary>
        public WorkforceController WorkforceController
        {
            get
            {
                if (_workforceController == null)
                {
                    _workforceController = new WorkforceController();
                }
                return _workforceController;
            }
        }

        /// <summary>
        /// 机器唯一码
        /// </summary>
        public string RobotUniqueNo
        {
            get
            {
                return ControllerSettings.Instance.RobotUniqueNo;
            }
            set
            {
                ControllerSettings.Instance.RobotUniqueNo = value;
            }
        }

        public string SessionId
        {
            get
            {
                return ControllerSettings.Instance.SessionId;
            }
            set
            {
                ControllerSettings.Instance.SessionId = value;
            }
        }

        /// <summary>
        /// 机器人Id
        /// </summary>
        public string RobotId { get; set; }

        /// <summary>
        /// 机器人状态
        /// </summary>
        public RobotState RobotState { get; set; }

        /// <summary>
        /// Ip
        /// </summary>
        public string Ip => MyComputerInfo.Instance().IpAddress;

        /// <summary>
        /// 当前的任务Id
        /// </summary>
        public string CurrentTaskId => CurrentTaskContext?.TaskId;

        /// <summary>
        /// 当前任务上下文
        /// </summary>
        public TaskContext CurrentTaskContext { get; set; }

        private ConnectedState _connectedState = ConnectedState.Disconnected;

        public ConnectedState ConnectedState
        {
            get
            {
                return _connectedState;
            }
            private set
            {
                if (_connectedState == value)
                {
                    return;
                }
                _connectedState = value;

                ConnectedStateChanged?.Invoke(null, new ConnectedStateChangedEventArgs(_connectedState));
            }
        }

        public bool ConnectedToController => ConnectedState == ConnectedState.Connected;

        private Timer _heatBeatTimer;

        private Timer _getTaskTimer;

        public Context(bool connectedToController=false)
        {
            if(_isCreated)
            {
                throw new InvalidOperationException("Context实例只能创建一次");
            }
            _heatBeatTimer = new Timer(SendHeartBeat);
            _getTaskTimer = new Timer(GetTask);

            //连接后的事件
            Messenger.Default.Register<ConnectedToControllerMessage>(this, "ConnectedToController", AfterConnectedToController);
            
            //连接后的事件
            Messenger.Default.Register<DisconnectToControllerMessage>(this, "DisconnectToController", AfterDisconnectedToController);

            //登陆后的事件
            Messenger.Default.Register<LoggedInMessage>(this, "LoggedIn", LoggedIn);

            _instance = this;
            _isCreated = true;

            ConnectedStateChanged += Settings.Instance.ConnectedStateChanged;

            if (connectedToController)
            {
                var response = RobotService.CheckRobot();

                if (response.Code == ResultCode.SuccessCode)
                {
                    Messenger.Default.Send(new ConnectedToControllerMessage(response.Result.RobotId), "ConnectedToController");
                }
            }
        }

        #region HeartBeat
        /// <summary>
        /// 计时器回调
        /// </summary>
        /// <param name="state"></param>
        private void SendHeartBeat(object state)
        {
            if (!ConnectedToController)
            {
                StopHeartBeat();
                return;
            }
            RobotService.SendHeartBeat();
        }

        /// <summary>
        /// 开始发送心跳
        /// </summary>
        public void StartHeartBeat()
        {
            _heatBeatTimer.Change(0, 10000);
        }

        /// <summary>
        /// 停止发送心跳
        /// </summary>
        public void StopHeartBeat()
        {
            _heatBeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        #endregion

        #region GetTask
        /// <summary>
        /// 获取任务
        /// </summary>
        /// <param name="state"></param>
        private void GetTask(object state)
        {
            if (!ConnectedToController)
            {
                StopGetTask();
                return;
            }
            RobotService.GetTask().Wait();
        }

        /// <summary>
        /// 开始获取任务
        /// </summary>
        public void StartGetTask()
        {
            _getTaskTimer.Change(5000, 0);
        }

        /// <summary>
        /// 停止获取任务
        /// </summary>
        public void StopGetTask()
        {
            _getTaskTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        #endregion

        /// <summary>
        /// 连接后事件
        /// </summary>
        /// <param name="loggedInMessage"></param>
        private void AfterConnectedToController(ConnectedToControllerMessage connectedToControllerMessage)
        {
            ConnectedState = ConnectedState.Connected;
            RobotId = connectedToControllerMessage.RobotId;
            RobotState = RobotState.Pend;
            StartHeartBeat();
            StartGetTask();
        }

        /// <summary>
        /// 断开连接后事件
        /// </summary>
        /// <param name="disconnectToControllerMessage"></param>
        private void AfterDisconnectedToController(DisconnectToControllerMessage disconnectToControllerMessage)
        {
            ConnectedState = ConnectedState.Disconnected;
            RobotId = null;
            RobotState = RobotState.Disconnected;
            StopGetTask();
            StopHeartBeat();
        }

        /// <summary>
        /// 登陆后事件
        /// </summary>
        /// <param name="loggedInMessage"></param>
        private void LoggedIn(LoggedInMessage loggedInMessage)
        {
            SessionId = loggedInMessage.SessionId;
        }
    }
}
