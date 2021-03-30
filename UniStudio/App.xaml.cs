using GalaSoft.MvvmLight.Threading;
using log4net;
using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.UiAutomation;
using UniStudio.Librarys;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using UniExecutor.View;
using ActiproSoftware.Text.Parsing;
using ActiproSoftware.Text.Parsing.Implementation;
using ActiproSoftware.Text.Languages.DotNet.Reflection;
using ActiproSoftware.Text.Languages.DotNet.Reflection.Implementation;
using ActiproSoftware.Text.Languages.VB.Implementation;

namespace UniStudio
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static App instance = null;

        private Mutex instanceMutex = null;

        public static string LocalRPAStudioDir { get; set; }

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            instance = this;

            new Context();

            DispatcherHelper.Initialize();

            Plugins.Shared.Library.Librarys.LanguageLocalization.ToChinese();

#if DEBUG
            AllocConsole();
#else
            Console.SetOut(new LogToOutputWindowTextWriter());
#endif

            Current.DispatcherUnhandledException += App_OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            bool createdNew = false;
            instanceMutex = new Mutex(true, "{211491920214915-2085-251920-18161-1915620231185-914-2085-231518124}", out createdNew);
            if (createdNew)
            {

                AmbientParseRequestDispatcherProvider.Dispatcher = new ThreadedParseRequestDispatcher();
                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"UniStudio\Assembly Repository");
                AmbientAssemblyRepositoryProvider.Repository = new FileBasedAssemblyRepository(appDataPath);


                Logger.Debug("Uni Studio启动……", logger);
                UiElement.Init();
            }
            else
            {
                UniMessageBox.Show("该程序已经运行，不能重复运行！");
                Environment.Exit(0);
            }

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
                        //UniMessageBox.Show("错误类型： " + exception.Message + "\n错误源： " + exception.Source + "\n错误详细信息： " + exception.StackTrace, "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        //UniMessageBox.Show(exception.ToString(), "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        var window = new RuntimeErrorDialogs(exception.Source, exception.Message, exception.GetType().FullName, exception.ToString());
                        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        window.Topmost = true;
                        window.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal("不可恢复的非UI线程全局异常", logger);
                    Logger.Fatal(ex, logger);
                    //UniMessageBox.Show("错误类型： " + ex.Message + "\n错误源： " + ex.Source + "\n错误详细信息： " + ex.StackTrace, "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    //UniMessageBox.Show(ex.ToString(), "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    var window = new RuntimeErrorDialogs(ex.Source, ex.Message, ex.GetType().FullName, ex.ToString());
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    window.Topmost = true;
                    window.ShowDialog();
                }
            });
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Common.RunInUI(() =>
            {
                try
                {
                    var exception = e.Exception;
                    Logger.Error("UI线程全局异常", logger);
                    Logger.Error(exception, logger);
                    e.Handled = true;
                    //UniMessageBox.Show("错误类型： " + exception.Message + "\n错误源： " + exception.Source + "\n错误详细信息： " + exception.StackTrace, "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    //UniMessageBox.Show(exception.ToString(), "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    var window = new RuntimeErrorDialogs(exception.Source, exception.Message, exception.GetType().FullName, exception.ToString());
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    window.Topmost = true;
                    window.ShowDialog();
                }
                catch (Exception ex)
                {
                    Logger.Fatal("不可恢复的UI线程全局异常", logger);
                    Logger.Fatal(ex, logger);
                    //UniMessageBox.Show("错误类型： " + ex.Message + "\n错误源： " + ex.Source + "\n错误详细信息： " + ex.StackTrace, "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    //UniMessageBox.Show(ex.ToString(), "运行时执行错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    var window = new RuntimeErrorDialogs(ex.Source, ex.Message, ex.GetType().FullName, ex.ToString());
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    window.Topmost = true;
                    window.ShowDialog();
                }
            });
        }

        private static void KillProcess(string processName)
        {
            Process[] myproc = Process.GetProcesses();
            foreach (Process item in myproc)
            {
                if (item.ProcessName == processName)
                {
                    item.Kill();
                }
             }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
#if DEBUG
            FreeConsole();
#endif
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var dispatcher = AmbientParseRequestDispatcherProvider.Dispatcher;
            if (dispatcher != null)
            {
                AmbientParseRequestDispatcherProvider.Dispatcher = null;
                dispatcher.Dispose();
            }

            var repository = AmbientAssemblyRepositoryProvider.Repository;
            if (repository != null)
                repository.PruneCache();
            base.OnExit(e);
        }





    }
}
