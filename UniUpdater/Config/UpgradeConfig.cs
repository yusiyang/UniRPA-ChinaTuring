using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace UniUpdater.Config
{
    public class UpgradeConfig
    {
        private string _xmlFile;

        private XmlDocument _xmlDoc;

        private static UpgradeConfig _instance;

        private string _upgradeServerConfigUrl;

        public string UpgradeServerConfigUrl
        {
            get
            {
                if (_upgradeServerConfigUrl == null)
                {
                    var xmlNode = _xmlDoc.SelectSingleNode("RPAUpgradeClientConfig/RPAUpgradeServerConfig") as XmlElement;
                    _upgradeServerConfigUrl = xmlNode?.GetAttribute("Url");
                }
                return _upgradeServerConfigUrl;
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
                    var updateLogElement = _xmlDoc.SelectSingleNode("RPAUpgradeClientConfig/UpdateLog");
                    var items = updateLogElement.SelectNodes("Item");
                    foreach (var item in items)
                    {
                        var text = (item as XmlElement).InnerText;
                        _updateLogs.Add(text);
                    }
                }
                return _updateLogs;
            }
            set
            {
                _updateLogs = value;
                var updateLogElement = _xmlDoc.SelectSingleNode("RPAUpgradeClientConfig/UpdateLog");
                updateLogElement.RemoveAll();

                foreach (var item in _updateLogs)
                {
                    var xmlItem= _xmlDoc.CreateElement("Item");
                    xmlItem.InnerText = item;
                    updateLogElement.AppendChild(xmlItem);
                }

                _xmlDoc.Save(_xmlFile);
            }
        }

        public static UpgradeConfig Instance => _instance;

        static UpgradeConfig()
        {
            _instance = new UpgradeConfig();
        }

        private UpgradeConfig()
        {
            _xmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\UpgradeConfig.xml");

            _xmlDoc = new XmlDocument();
            _xmlDoc.Load(_xmlFile);
        }
    }
}
