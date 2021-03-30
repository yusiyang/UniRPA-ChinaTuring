using GalaSoft.MvvmLight.Threading;
using log4net;
using Plugins.Shared.Library.UiAutomation;
using UniWorkforce.Librarys;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;

namespace UniWorkforce
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Mutex instanceMutex = null;

        private static bool _shouldConnectedToController = false;

        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();

        [STAThread]
        public static void Main(string[] args)
        {
            //UniMessageBox.Show("UniWorkforce Start");
            if(args?.Length>0)
            {
                var arg1 = args[0];
                if(arg1=="ConnectedToController")
                {
                    _shouldConnectedToController = true;
                }
            }
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            DispatcherHelper.Initialize();

#if DEBUG
            AllocConsole();
#endif

            Current.DispatcherUnhandledException += App_OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            bool createdNew = false;
            instanceMutex = new Mutex(true, "{21149231518116151835-2085-195315144-18161-1915620231185-914-2085-231518124}", out createdNew);
            if (createdNew)
            {
                Logger.Debug("UniWorkforce启动……", logger);

                UiElement.Init();

                initConfigDir();

                new Context(_shouldConnectedToController);
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private void initConfigDir()
        {
            var configDir = Settings.Instance.LocalRPAStudioDir + @"\Config";

            if (!System.IO.Directory.Exists(configDir))
            {
                System.IO.Directory.CreateDirectory(configDir);
            }


            //以下的XML是用户可能会修改的配置，升级时一般要保留旧数据
            //TODO WJF 后期需要根据XML里的版本号对配置文件数据进行迁移

            if (!System.IO.File.Exists(configDir + @"\UniWorkforce.settings"))
            {
                byte[] data = UniWorkforce.Properties.Resources.UniWorkforce_settings;
                System.IO.File.WriteAllBytes(configDir + @"\UniWorkforce.settings", data);
            }
            else
            {
                if (UpgradeSettings())
                {
                    Logger.Debug(string.Format("升级xml配置文件 {0} ……", Settings.Instance.LocalRPAStudioDir + @"\Config\UniWorkforce.settings"), logger);
                }
            }
        }

        private bool UpgradeSettings()
        {
            //TODO WJF 后期可能的配置文件升级，参考UniStudio的升级方式即可
            return false;
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Common.RunInUI(() =>
            {
                try
                {
                    Logger.Error("UI线程全局异常", logger);
                    Logger.Error(e.Exception, logger);
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    Logger.Fatal("不可恢复的UI线程全局异常", logger);
                    Logger.Fatal(ex, logger);
                }
            });
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Common.RunInUI(() =>
            {
                try
                {
                    var exception = e.ExceptionObject as Exception;
                    if (exception != null)
                    {
                        Logger.Error("非UI线程全局异常", logger);
                        Logger.Error(exception, logger);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal("不可恢复的非UI线程全局异常", logger);
                    Logger.Fatal(ex, logger);
                }
            });
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
#if DEBUG
            FreeConsole();
#endif
        }




    }
}
