using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniWorkforce
{
    public class Settings
    {
        private static Settings _instance;

        public static Settings Instance => _instance;

        private bool _isRemoting = false;

        private string _localRPAStudioDir = null;

        public string LocalRPAStudioDir
        {
            get 
            {
                if(_localRPAStudioDir==null)
                {
                    var localAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                    _localRPAStudioDir = localAppData + @"\UniStudio";

                    if (!Directory.Exists(_localRPAStudioDir))
                    {
                        Directory.CreateDirectory(_localRPAStudioDir);
                    }
                }
                return _localRPAStudioDir;
            }
        }

        private string _logsDir = null;

        public string LogsDir
        {
            get
            {
                if(_logsDir==null)
                {
                    _logsDir = LocalRPAStudioDir + @"\Logs";

                    if (!Directory.Exists(_logsDir))
                    {
                        Directory.CreateDirectory(_logsDir);
                    }
                }
                return _logsDir;
            }
        }

        private string _packagesDir = null;

        public string PackagesDir
        {
            get
            {
                if (_packagesDir == null)
                {
                    var commonApplicationData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
                    if (_isRemoting)
                    {
                        _packagesDir = commonApplicationData + @"\UniStudio\Remoting\Packages";
                    }
                    else
                    {
                        _packagesDir = commonApplicationData + @"\UniStudio\Packages"; ;
                    }

                    if (!Directory.Exists(_packagesDir))
                    {
                        Directory.CreateDirectory(_packagesDir);
                    }
                }
                return _packagesDir;
            }
        }

        private string _installedPackagesDir = null;

        public string InstalledPackagesDir
        {
            get
            {
                if (_installedPackagesDir == null)
                {
                    var commonApplicationData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
                    if (_isRemoting)
                    {
                        _installedPackagesDir = commonApplicationData + @"\UniStudio\Remoting\InstalledPackages";
                    }
                    else
                    {
                        _installedPackagesDir = commonApplicationData + @"\UniStudio\InstalledPackages";
                    }

                    if (!Directory.Exists(_installedPackagesDir))
                    {
                        Directory.CreateDirectory(_installedPackagesDir);
                    }
                }
                return _installedPackagesDir;
            }
        }

        static Settings()
        {
            _instance = new Settings();
        }

        public void ConnectedStateChanged(object sender, Events.ConnectedStateChangedEventArgs e)
        {
            _isRemoting = e.ConnectedState == ConnectedState.Connected;
            _packagesDir = null;
            _installedPackagesDir = null;
        }
    }
}
