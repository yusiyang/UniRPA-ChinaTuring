using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LitJson;
using Microsoft.Win32;
using UniStudio.Librarys;
using Plugins.Shared.Library.Extensions;

namespace UniStudio.ViewModel
{
    public class ToolViewModel : ViewModelBase
    {
        public UserControl m_view { get; set; }

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
                        m_view = (UserControl)p.Source;
                    }));
            }
        }


        #region 主题色配置
        public const string ItemTitleForegroundPropertyName = "ItemTitleForeground";
        private SolidColorBrush _itemTitleForeground = new BrushConverter().ConvertFrom("#383838") as SolidColorBrush;
        public SolidColorBrush ItemTitleForeground
        {
            get
            {
                return _itemTitleForeground;
            }
            set
            {
                if (_itemTitleForeground == value)
                {
                    return;
                }

                _itemTitleForeground = value;
                RaisePropertyChanged(ItemTitleForegroundPropertyName);
            }
        }

        public const string ItemDescriptionForegroundPropertyName = "ItemDescriptionForeground";
        private SolidColorBrush _itemDescriptionForeground = new BrushConverter().ConvertFrom("#606060") as SolidColorBrush;
        public SolidColorBrush ItemDescriptionForeground
        {
            get
            {
                return _itemDescriptionForeground;
            }
            set
            {
                if (_itemDescriptionForeground == value)
                {
                    return;
                }

                _itemDescriptionForeground = value;
                RaisePropertyChanged(ItemDescriptionForegroundPropertyName);
            }
        }

        public const string ItemMouseOverBackgroundPropertyName = "ItemMouseOverBackground";
        private SolidColorBrush _itemMouseOverBackground = new BrushConverter().ConvertFrom("#d6d6d6") as SolidColorBrush;
        public SolidColorBrush ItemMouseOverBackground
        {
            get
            {
                return _itemMouseOverBackground;
            }
            set
            {
                if (_itemMouseOverBackground == value)
                {
                    return;
                }

                _itemMouseOverBackground = value;
                RaisePropertyChanged(ItemMouseOverBackgroundPropertyName);
            }
        }
        #endregion


        private RelayCommand _launchUiExplorerCommand;
        public RelayCommand LaunchUiExplorerCommand
        {
            get
            {
                return _launchUiExplorerCommand
                    ?? (_launchUiExplorerCommand = new RelayCommand(
                    () =>
                    {
                        // 运行 UI 探测器.exe
                        string currentWorkDirectory = Directory.GetCurrentDirectory();
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = Path.Combine(currentWorkDirectory, "UniExplorer.exe");
                        startInfo.WorkingDirectory = currentWorkDirectory;
                        //startInfo.Arguments = "<Pane ClassName = '#32769' />";
                        Process.Start(startInfo);
                    }));
            }
        }

        private RelayCommand _initInternetExplorerCommand;

        public RelayCommand InitInternetExplorerCommand
        {
            get
            {
                return _initInternetExplorerCommand
                       ?? (_initInternetExplorerCommand = new RelayCommand(
                           () =>
                           {
                               if (!IsAdministrator())
                               {
                                   UniMessageBox.Show("安装失败，请尝试用管理员权限运行此软件！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                                   return;
                               }
                               var lcMachine = Registry.LocalMachine;
                               var crx = lcMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Internet Explorer\\BrowserEmulation");
                               crx?.SetValue("IntranetCompatibilityMode", 0);
                               lcMachine.Close();
                               UniMessageBox.Show("操作完成，请重启 Internet Explorer 以便操作生效", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                           }));
            }
        }

        private RelayCommand _installChromePluginCommand;
        public RelayCommand InstallChromePluginCommand
        {
            get
            {
                return _installChromePluginCommand
                    ?? (_installChromePluginCommand = new RelayCommand(
                        InstallChromeExtension));
            }
        }

        private RelayCommand _installFirefoxPluginCommand;
        public RelayCommand InstallFirefoxPluginCommand
        {
            get
            {
                return _installFirefoxPluginCommand
                    ?? (_installFirefoxPluginCommand = new RelayCommand(
                        () => { InstallFirefoxPlugin(); }));
            }
        }

        private void InstallFirefoxPlugin()
        {
            if (!IsAdministrator())
            {
                UniMessageBox.Show("插件安装失败，请尝试用管理员权限运行此软件！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var extensionPath = Environment.CurrentDirectory + $"\\BrowserExtensions\\FirefoxExtension\\unirpa_web_automation-1.0.0-fx.xpi";
            var hostPath = Environment.CurrentDirectory + "\\BrowserExtensions\\NativeHost";
            var mainFest = JsonMapper.ToObject(File.ReadAllText(hostPath + "\\mainfest-ff.json"));
            mainFest["path"] = hostPath + "\\UniBrowserHost.exe";
            File.WriteAllText(hostPath + "\\mainfest-ff.json", mainFest.ToJson());
            RegUtil.SetRegistryKey("HKEY_CURRENT_USER", "Software\\Mozilla\\NativeMessagingHosts\\com.uni.native_message_host",
                "",$"{hostPath}\\mainfest-ff.json");


            Process.Start("firefox.exe", $" -install -extension {extensionPath}");
        }

        private RelayCommand _installJavaPluginCommand;
        public RelayCommand InstallJavaPluginCommand
        {
            get
            {
                return _installJavaPluginCommand
                    ?? (_installJavaPluginCommand = new RelayCommand(
                        () =>
                        {
                            // 检查是否存在系统环境变量（JAVA_HOME）
                            string javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
                            if (string.IsNullOrEmpty(javaHome))
                            {
                                UniMessageBox.Show("本机未安装 Java 运行环境或未配置系统环境变量 JAVA_HOME，请安装 JDK 后重试", "警告");
                                return;
                            }

                            var ret = UniMessageBox.Show("是否启用Java Access Bridge？", "询问", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                            if (ret == MessageBoxResult.Yes)
                            {
                                //java8及以后的版本直接调用命令行，之前的版本需要手动安装
                                string jabswitchExe = Path.Combine(javaHome, @"bin\jabswitch.exe");
                                if (File.Exists(jabswitchExe))
                                {
                                    //存在jabswitch.exe，则可直接调用jabswitch.exe -enable来启用
                                    UiCommon.RunProcess(jabswitchExe, "-enable", true);
                                }
                                else
                                {
                                    //需要主动安装accessbridge相关文件(根据32位或64位的JRE进行拷贝)
                                    string javaExe = Path.Combine(javaHome, @"bin\java.exe");
                                    string windowsHome = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

                                    OverlayForm.InstallJavaAccessBridge(Environment.Is64BitOperatingSystem, UiCommon.IsExe64Bit(javaExe), windowsHome, javaHome);
                                }

                                UniMessageBox.Show("操作完成，请重新运行Java程序以便操作生效", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }));
            }
        }

        private void InstallChromeExtension()
        {
            if (!IsAdministrator())
            {
                UniMessageBox.Show("Chrome插件安装失败，请尝试用管理员权限运行此软件！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var crxId = "faccajciemabnibafnckngakdflmjlip";
            var extensionPath = Environment.CurrentDirectory + $"\\BrowserExtensions\\ChromeExtension\\{crxId}.crx";
            var settingPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\User Data\\Default\\Secure Preferences";
            var hostPath = Environment.CurrentDirectory + "\\BrowserExtensions\\NativeHost";
            var cachePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                            "\\ChromeExtensionCache";

            if (Process.GetProcessesByName("chrome").Any())
            {
                UniMessageBox.Show("请关闭所有的Chrome浏览器窗口后重试", "Error");
                return;
            }
            if (!File.Exists(extensionPath) || !Directory.GetFiles(hostPath).Any())
            {
                UniMessageBox.Show("当前目录内没有找到插件文件或相应软件", "Error");
                return;
            }

            if (!File.Exists(settingPath))
            {
                UniMessageBox.Show("未找到Chrome配置文件", "Error");
                return;
            }

            var settings = JsonMapper.ToObject(File.ReadAllText(settingPath));
            if (settings["extensions"]["settings"].ContainsKey(crxId))
            {
                settings["extensions"]["settings"][crxId].Clear();
            }
            File.WriteAllText(settingPath, settings.ToJson());

            var mainFest = JsonMapper.ToObject(File.ReadAllText(hostPath + "\\manifest.json"));
            mainFest["path"] = hostPath + "\\UniBrowserHost.exe";
            mainFest["allowed_origins"].Clear();
            mainFest["allowed_origins"].Add($"chrome-extension://{crxId}/");
            File.WriteAllText(hostPath + "\\manifest.json", mainFest.ToJson());

            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            File.Copy(extensionPath, cachePath + $"\\{crxId}.crx", true);

            try
            {
                RegistryChrome(crxId, cachePath + $"\\{crxId}.crx", hostPath + "\\manifest.json");
            }
            catch
            {
                
            }

            Process.Start("chrome.exe", $" --install-supervised-user-whitelists");
            UniMessageBox.Show("Chrome插件安装成功！请在浏览器中手动启用", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static void RegistryChrome(string crxId, string crxPath, string hostPath)
        {
            RegUtil.SetRegistryKey("HKEY_CURRENT_USER", $"Software\\Google\\Chrome\\Extensions\\{crxId}", "path", crxPath);
            RegUtil.SetRegistryKey("HKEY_CURRENT_USER", $"Software\\Google\\Chrome\\Extensions\\{crxId}", "version", "1.0");
            RegUtil.SetRegistryKey("HKEY_CURRENT_USER", $"Software\\Google\\Chrome\\NativeMessagingHosts\\com.uni.native_message_host", "", hostPath);
            RegUtil.SetRegistryKey("HKEY_LOCAL_MACHINE", "SOFTWARE\\Policies\\Google\\Chrome\\ExtensionInstallWhitelist", "1", crxId);
            //var cUser= Registry.CurrentUser;
            //var crx= cUser.CreateSubKey($"Software\\Google\\Chrome\\Extensions\\{crxId}", RegistryKeyPermissionCheck.ReadWriteSubTree);
            //crx?.SetValue("path",crxPath);
            //crx?.SetValue("version", "1.0");

            //crx = cUser.CreateSubKey($"Software\\Google\\Chrome\\NativeMessagingHosts\\com.uni.native_message_host", RegistryKeyPermissionCheck.ReadWriteSubTree);
            //crx?.SetValue("", hostPath);
            //cUser.Close();

            //var lcMachine = Registry.LocalMachine;
            //crx = lcMachine.CreateSubKey("SOFTWARE\\Policies\\Google\\Chrome\\ExtensionInstallWhitelist", RegistryKeyPermissionCheck.ReadWriteSubTree);
            //crx?.SetValue("1",crxId);
            //lcMachine.Close();
        }
        
        public static bool IsAdministrator()
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
