using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace UniWorkforce.Config
{
    public class ControllerSettings
    {
        private string _xmlFile;

        private XmlDocument _xmlDoc;

        private DateTime _fileLastUpdateTime;

        private bool _dataChanged = false;

        private Timer _timer;

        private static ControllerSettings _instance;

        private string _loginUrl;

        public string LoginUrl
        {
            get
            {
                if (_loginUrl == null)
                {
                    var xmlNode = _xmlDoc.SelectSingleNode("Settings/LoginUrl");
                    _loginUrl = xmlNode?.InnerText;
                }
                return _loginUrl;
            }
            set
            {
                if (_loginUrl != value)
                {
                    _loginUrl = value;
                    _dataChanged = true;
                }
            }
        }

        private string _controllerUrl;

        public string ControllerUrl
        {
            get
            {
                if(_controllerUrl==null)
                {
                    var xmlNode = _xmlDoc.SelectSingleNode("Settings/ControllerUrl");
                    _controllerUrl = xmlNode?.InnerText;
                }
                return _controllerUrl;
            }
            set
            {
                if (_controllerUrl != value)
                {
                    _controllerUrl = value;
                    _dataChanged = true;
                }
            }
        }

        private string _controllerPort;

        public string ControllerPort
        {
            get
            {
                if (_controllerPort == null)
                {
                    var xmlNode = _xmlDoc.SelectSingleNode("Settings/ControllerPort");
                    _controllerPort = xmlNode?.InnerText;
                }
                return _controllerPort;
            }
            set
            {
                if (_controllerPort != value)
                {
                    _controllerPort = value;
                    _dataChanged = true;
                }
            }
        }

        private string _robotUniqueNo;

        public string RobotUniqueNo
        {
            get
            {
                if (_robotUniqueNo == null)
                {
                    var xmlNode = _xmlDoc.SelectSingleNode("Settings/RobotUniqueNo");
                    _robotUniqueNo = xmlNode?.InnerText;
                }
                return _robotUniqueNo;
            }
            set
            {
                if (_robotUniqueNo != value)
                {
                    _robotUniqueNo = value;
                    _dataChanged = true;
                }
            }
        }

        private string _sessionId;

        public string SessionId
        {
            get
            {
                if (_sessionId == null)
                {
                    var xmlNode = _xmlDoc.SelectSingleNode("Settings/SessionId");
                    _sessionId = xmlNode?.InnerText;
                }
                return _sessionId;
            }
            set
            {
                if (_sessionId != value)
                {
                    _sessionId = value;
                    _dataChanged = true;
                }
            }
        }

        //private string _monitorProcess;

        //public string MonitorProcess
        //{
        //    get
        //    {
        //        if (_monitorProcess == null)
        //        {
        //            var xmlNode = _xmlDoc.SelectSingleNode("Settings/MonitorProcess");
        //            _monitorProcess = xmlNode?.InnerText;
        //        }
        //        return _monitorProcess;
        //    }
        //    set
        //    {
        //        if (_monitorProcess != value)
        //        {
        //            _monitorProcess = value;
        //            _dataChanged = true;
        //        }
        //    }
        //}

        public static ControllerSettings Instance => _instance;

        static ControllerSettings()
        {
            _instance = new ControllerSettings();
        }

        private ControllerSettings()
        {
            _xmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\ControllerSettings.xml");
            _timer = new Timer(Update);

            Init();
            StartTimer();
        }

        private void Init()
        {
            _xmlDoc = new XmlDocument();
            _xmlDoc.Load(_xmlFile);

            _fileLastUpdateTime = File.GetLastWriteTime(_xmlFile);
            _loginUrl = null;
            _controllerUrl = null;
            _controllerPort = null;
            _robotUniqueNo = null;
            _dataChanged = false;
            _sessionId = null;
            //_monitorProcess = null;
        }

        /// <summary>
        /// 计时器回调
        /// </summary>
        /// <param name="state"></param>
        private void Update(object state)
        {
            if(_dataChanged)
            {
                var loginUrlNode = _xmlDoc.SelectSingleNode("Settings/LoginUrl");
                loginUrlNode.InnerText = _loginUrl;

                var controllerUrlNode = _xmlDoc.SelectSingleNode("Settings/ControllerUrl");
                controllerUrlNode.InnerText = _controllerUrl;

                var controllerPortNode = _xmlDoc.SelectSingleNode("Settings/ControllerPort");
                controllerPortNode.InnerText = _controllerPort;

                var robotUniqueNoNode = _xmlDoc.SelectSingleNode("Settings/RobotUniqueNo");
                robotUniqueNoNode.InnerText = _robotUniqueNo;

                var sessionIdNode = _xmlDoc.SelectSingleNode("Settings/SessionId");
                sessionIdNode.InnerText = _sessionId;

                //var monitorProcessNode = _xmlDoc.SelectSingleNode("Settings/MonitorProcess");
                //monitorProcessNode.InnerText = _monitorProcess;

                _xmlDoc.Save(_xmlFile);
                return;
            }

            if(File.GetLastWriteTime(_xmlFile)>_fileLastUpdateTime)
            {
                Init();
            }
        }

        /// <summary>
        /// 开始
        /// </summary>
        public void StartTimer()
        {
            _timer.Change(0, 1000*60);
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void StopTimer()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
