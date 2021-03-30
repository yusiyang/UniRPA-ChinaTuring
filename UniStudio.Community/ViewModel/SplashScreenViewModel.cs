using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using UniStudio.Community.Librarys;
using UniStudio.Community.Upgrader;
using UniStudio.Community.Windows;

namespace UniStudio.Community.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SplashScreenViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Window m_view;

        /// <summary>
        /// Initializes a new instance of the SplashScreenViewModel class.
        /// </summary>
        public SplashScreenViewModel()
        {
        }

        private RelayCommand<RoutedEventArgs> _loadedCommand;

        /// <summary>
        /// Gets the LoadedCommand.
        /// </summary>
        public RelayCommand<RoutedEventArgs> LoadedCommand
        {
            get
            {
                return _loadedCommand
                    ?? (_loadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        Logger.Debug("SplashScreen窗口启动", logger);
                        m_view = (Window)p.Source;

                        Init();
                    }));
            }
        }


        private void Init()
        {
            Task.Run(() =>
            {
                initLocalRPAStudioDir();
                initLogsDir();
                initConfigDir();
                initUpdateDir();

                //授权检测

                if (!DoAuthorizationCheck())
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RegisterWindow registerWindow = new RegisterWindow();
                        App.Current.MainWindow = registerWindow;
                        registerWindow.Show();
                        m_view.Close();
                        registerWindow.Activate();
                    });
                }

                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var mainWindow = new Windows.MainWindow();
                        App.Current.MainWindow = mainWindow;
                        mainWindow.Show();
                        m_view.Close();
                    });
                }
            });
        }


        public const string StudioVersionPropertyName = "StudioVersion";
        private string _studioVersion;
        public string StudioVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_studioVersion))
                {
                    _studioVersion = "版本：Pegasus_v" + Common.GetProgramVersion();
                }

                return _studioVersion;
            }

            set
            {
                if (_studioVersion == value)
                {
                    return;
                }

                _studioVersion = value;
                RaisePropertyChanged(StudioVersionPropertyName);
            }
        }


        private void initUpdateDir()
        {
            var udpateDir = App.LocalRPAStudioDir + @"\Update";

            if (!System.IO.Directory.Exists(udpateDir))
            {
                System.IO.Directory.CreateDirectory(udpateDir);
            }
        }

        private void initLocalRPAStudioDir()
        {
            var localAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            App.LocalRPAStudioDir = localAppData + @"\UniStudio";
        }

        private void initLogsDir()
        {
            var logsDir = App.LocalRPAStudioDir + @"\Logs";

            if (!System.IO.Directory.Exists(logsDir))
            {
                System.IO.Directory.CreateDirectory(logsDir);
            }
        }

        private void initConfigDir()
        {
            var configDir = App.LocalRPAStudioDir + @"\Config";

            if (!System.IO.Directory.Exists(configDir))
            {
                System.IO.Directory.CreateDirectory(configDir);
            }


            //以下的XML是用户可能会修改的配置，升级时一般要保留旧数据
            //TODO WJF 后期需要根据XML里的版本号对配置文件数据进行迁移
            if (!System.IO.File.Exists(configDir + @"\CodeSnippets.xml"))
            {
                byte[] data = UniStudio.Community.Properties.Resources.CodeSnippets;
                System.IO.File.WriteAllBytes(configDir + @"\CodeSnippets.xml", data);
            }

            if (!System.IO.File.Exists(configDir + @"\FavoriteActivities.xml"))
            {
                byte[] data = UniStudio.Community.Properties.Resources.FavoriteActivities;
                System.IO.File.WriteAllBytes(configDir + @"\FavoriteActivities.xml", data);
            }

            if (!System.IO.File.Exists(configDir + @"\ProjectUserConfig.xml"))
            {
                byte[] data = UniStudio.Community.Properties.Resources.ProjectConfig;
                System.IO.File.WriteAllBytes(configDir + @"\ProjectUserConfig.xml", data);
            }

            if (!System.IO.File.Exists(configDir + @"\RecentActivities.xml"))
            {
                byte[] data = UniStudio.Community.Properties.Resources.RecentActivities;
                System.IO.File.WriteAllBytes(configDir + @"\RecentActivities.xml", data);
            }

            if (!System.IO.File.Exists(configDir + @"\RecentProjects.xml"))
            {
                byte[] data = UniStudio.Community.Properties.Resources.RecentProjects;
                System.IO.File.WriteAllBytes(configDir + @"\RecentProjects.xml", data);
            }

            if (!System.IO.File.Exists(configDir + @"\UniStudio.settings"))
            {
                byte[] data = UniStudio.Community.Properties.Resources.UniStudio_settings;
                System.IO.File.WriteAllBytes(configDir + @"\UniStudio.settings", data);
            }
            else
            {
                if (UpgradeSettings())
                {
                    Logger.Debug(string.Format("升级xml配置文件 {0} ……", App.LocalRPAStudioDir + @"\Config\UniStudio.settings"), logger);
                }
            }
        }

        private bool UpgradeSettings()
        {
            XmlDocument doc = new XmlDocument();
            var path = App.LocalRPAStudioDir + @"\Config\UniStudio.settings";
            doc.Load(path);
            var rootNode = doc.DocumentElement;

            var schemaVersion = rootNode.GetAttribute("schemaVersion");

            XmlDocument docNew = new XmlDocument();

            using (var ms = new MemoryStream(UniStudio.Community.Properties.Resources.UniStudio_settings))
            {
                ms.Flush();
                ms.Position = 0;
                docNew.Load(ms);
                ms.Close();
            }

            var rootNodeNew = docNew.DocumentElement;
            var schemaVersionNew = rootNodeNew.GetAttribute("schemaVersion");

            var schemaVersionTmp = schemaVersion;

            UpgradeSettings(ref schemaVersionTmp, schemaVersionNew);

            if (schemaVersion == schemaVersionTmp)
            {
                return false;
            }
            else
            {
                schemaVersion = schemaVersionTmp;
                return true;
            }
        }

        private void UpgradeSettings(ref string schemaVersion, string schemaVersionNew)
        {
            if (schemaVersion == schemaVersionNew)
            {
                return;
            }

            if (string.IsNullOrEmpty(schemaVersion))
            {
                //从空schemaVersion=>1.0.0
                XmlDocument doc = new XmlDocument();
                var path = App.LocalRPAStudioDir + @"\Config\UniStudio.settings";
                doc.Load(path);
                var rootNode = doc.DocumentElement;
                rootNode.SetAttribute("schemaVersion", "1.0.0");

                var publishHistoryElement = rootNode.SelectSingleNode("PublishHistory") as XmlElement;

                var lastPublishBrowseInitialFolderElement = publishHistoryElement.SelectSingleNode("LastPublishBrowseInitialFolder") as XmlElement;
                if (lastPublishBrowseInitialFolderElement != null)
                {
                    publishHistoryElement.RemoveChild(lastPublishBrowseInitialFolderElement);
                }

                var lastPublishUriElement = publishHistoryElement.SelectSingleNode("LastPublishUri") as XmlElement;

                if (lastPublishUriElement == null)
                {
                    lastPublishUriElement = doc.CreateElement("LastPublishUri");
                    publishHistoryElement.AppendChild(lastPublishUriElement);
                }

                doc.Save(path);

                schemaVersion = "1.0.0";
            }
            else if (schemaVersion == "1.0.0")
            {
                //TODO WJF 1.0.0=>1.0.1升级
                schemaVersion = "1.0.1";
            }
            else if (schemaVersion == "1.0.1")
            {
                //TODO WJF 1.0.1=>1.0.2升级
                schemaVersion = "1.0.2";
            }

            UpgradeSettings(ref schemaVersion, schemaVersionNew);
        }



        public bool DoAuthorizationCheck()
        {
#if !DEBUG
            //授权检测
            if (!IsNotExpired())
            {
                var tip = "软件未通过授权检测，请注册产品！";
                Logger.Debug(tip, logger);
                //UniMessageBox.Show(tip, "提示", MessageBoxButton.OK, MessageBoxImage.Error);

                //RegisterWindow registerWindow = new RegisterWindow();
                //registerWindow.ShowDialog();

                //弹出注册窗口
                //if (!ViewModelLocator.instance.Startup.RegisterWindow.IsVisible)
                //{
                //    var vm = ViewModelLocator.instance.Startup.RegisterWindow.DataContext as RegisterViewModel;
                //    vm.LoadRegisterInfo();

                //    ViewModelLocator.instance.Startup.RegisterWindow.Show();
                //}

                //ViewModelLocator.instance.Startup.RegisterWindow.Activate();
                //Environment.Exit(0);
                return false;
            }
            return true;
#else
            return true;
#endif
        }



        public bool IsNotExpired()
        {
            string expiresDate = "";
            return IsNotExpired(ref expiresDate);
        }

        public bool IsNotExpired(ref string expiresDate)
        {
            var fileFullPath = App.LocalRPAStudioDir + @"\Authorization\license.authorization";

            return IsNotExpired(fileFullPath, ref expiresDate);
        }


        public bool IsNotExpired(string fileFullPath, ref string expiresDate)
        {
            bool isNotExpired = false;

            if (!System.IO.File.Exists(fileFullPath))
            {
                return isNotExpired;
            }

            if (Plugins.Shared.Library.Librarys.Common.CheckAuthorization(fileFullPath, UniStudio.Community.Properties.Resources.verify_public_rsa, ref expiresDate))
            {
                //授权合法，检查下有效期
                if (expiresDate == "forever")
                {
                    isNotExpired = true;
                }
                else
                {
                    DateTime current = DateTime.Now;
                    DateTime deadline = Convert.ToDateTime(expiresDate).AddDays(1);//截止日期得再加上一天，因为从当天00:00:00截止
                    if (current.CompareTo(deadline) < 0)
                    {
                        isNotExpired = true;
                    }
                }
            }

            return isNotExpired;
        }







    }
}