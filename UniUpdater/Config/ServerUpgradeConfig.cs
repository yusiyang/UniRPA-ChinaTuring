using System;
using System.Collections.Generic;
using System.Xml;
using UniUpdater.Libraries;

namespace UniUpdater.Config
{
    public class ServerUpgradeConfig
    {
        private XmlDocument _xmlDoc;

        private static ServerUpgradeConfig _instance;

        private string _upgradeUrl;

        public string UpgradeUrl
        {
            get
            {
                if (_upgradeUrl == null)
                {
                    var xmlNode = _xmlDoc.SelectSingleNode("RPAUpgradeServerConfig/AutoUpgradePackpage") as XmlElement;
                    _upgradeUrl = xmlNode?.GetAttribute("Url");
                }
                return _upgradeUrl;
            }
        }

        private Version _version;

        public Version Version
        {
            get
            {
                if (_version == null)
                {
                    var xmlNode = _xmlDoc.SelectSingleNode("RPAUpgradeServerConfig/AutoUpgradePackpage") as XmlElement;
                    var versionStr = xmlNode?.GetAttribute("Version");
                    if(!string.IsNullOrWhiteSpace(versionStr))
                    {
                        _version = new Version(versionStr);
                    }
                }
                return _version;
            }
        }

        private string _md5;

        public string Md5
        {
            get
            {
                if (_md5 == null)
                {
                    var xmlNode = _xmlDoc.SelectSingleNode("RPAUpgradeServerConfig/AutoUpgradePackpage") as XmlElement;
                    _md5 = xmlNode?.GetAttribute("Md5");
                }
                return _md5;
            }
        }

        private List<string> _updateLogs;

        public List<string> UpdateLogs
        {
            get
            {
                if (_updateLogs == null)
                {
                    _updateLogs = new List<string>();
                    var updateLogElement = _xmlDoc.SelectSingleNode("RPAUpgradeServerConfig/UpdateLog");
                    var items = updateLogElement.SelectNodes("Item");
                    foreach (var item in items)
                    {
                        var text = (item as XmlElement).InnerText;
                        _updateLogs.Add(text);
                    }
                }
                return _updateLogs;
            }
        }

        public static ServerUpgradeConfig Instance => _instance;

        static ServerUpgradeConfig()
        {
            _instance = new ServerUpgradeConfig();
        }

        private ServerUpgradeConfig()
        {
            var rpaUpgradeServerConfig = HttpRequest.Get(UpgradeConfig.Instance.UpgradeServerConfigUrl);

            if (!string.IsNullOrEmpty(rpaUpgradeServerConfig))
            {
                _xmlDoc = new XmlDocument();
                _xmlDoc.LoadXml(rpaUpgradeServerConfig);
            }
        }
    }
}
