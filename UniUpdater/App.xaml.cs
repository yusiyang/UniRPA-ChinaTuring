using log4net;
using Plugins.Shared.Library.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using UniNamedPipe;
using UniUpdater.Libraries;

namespace UniUpdater
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _localRPAStudioDir;

        public string LocalRPAStudioDir
        {
            get
            {
                if (_localRPAStudioDir == null)
                {
                    var localAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                    _localRPAStudioDir = localAppData + @"\UniStudio";
                }

                return _localRPAStudioDir;
            }
        }

        private string _updateDir;

        public string UpdateDir
        {
            get
            {
                if(_updateDir==null)
                {
                    _updateDir = Path.Combine(LocalRPAStudioDir, "Update");
                    if(!Directory.Exists(_updateDir))
                    {
                        Directory.CreateDirectory(_updateDir);
                    }
                }
                return _updateDir;
            }
        }

        private string _studioPath;

        public string StudioPath
        {
            get
            {
                if(_studioPath==null)
                {
                    _studioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UniStudio.Community.exe");
                }
                return _studioPath;
            }
        }

        private Version _studioVersion;

        public Version StudioVersion
        {
            get
            {
                if(_studioVersion==null)
                {
                    var versionStr= Common.GetProgramVersion();
                    _studioVersion = new Version(versionStr);
                }
                return _studioVersion;
            }
        }

        public static App Instance { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //MessageBox.Show("Updater start");
            Instance = this;

            bool createdNew = false;
            var instanceMutex = new Mutex(true, "UniUpdaterApp", out createdNew);
            if (!createdNew)
            {
                UniMessageBox.Show("该程序已经运行，不能重复运行！");
                Environment.Exit(0);
            }

            Current.DispatcherUnhandledException += App_OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Configure.ConfigureServer();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var exception = e.ExceptionObject as Exception;
                    if (exception != null)
                    {
                        Logger.Error("非UI线程全局异常", logger);
                        Logger.Error(exception, logger);
                        //UniMessageBox.Show("错误类型： " + exception.Message + "\n错误源： " + exception.Source + "\n错误详细信息： " + exception.StackTrace, "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        UniMessageBox.Show(exception.ToString(), "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal("不可恢复的非UI线程全局异常", logger);
                    Logger.Fatal(ex, logger);
                    //UniMessageBox.Show("错误类型： " + ex.Message + "\n错误源： " + ex.Source + "\n错误详细信息： " + ex.StackTrace, "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    UniMessageBox.Show(ex.ToString(), "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var exception = e.Exception;
                    Logger.Error("UI线程全局异常", logger);
                    Logger.Error(exception, logger);
                    e.Handled = true;
                    //UniMessageBox.Show("错误类型： " + exception.Message + "\n错误源： " + exception.Source + "\n错误详细信息： " + exception.StackTrace, "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    UniMessageBox.Show(exception.ToString(), "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    Logger.Fatal("不可恢复的UI线程全局异常", logger);
                    Logger.Fatal(ex, logger);
                    //UniMessageBox.Show("错误类型： " + ex.Message + "\n错误源： " + ex.Source + "\n错误详细信息： " + ex.StackTrace, "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    UniMessageBox.Show(ex.ToString(), "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }
    }
}
