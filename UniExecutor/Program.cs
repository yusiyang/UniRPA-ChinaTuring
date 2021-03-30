using Newtonsoft.Json;
using Plugins.Shared.Library.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UniExecutor.Core.Models;

namespace UniExecutor
{
    class Program
    {
        [System.STAThread()]
        [System.Diagnostics.DebuggerNonUserCode()]
        public static void Main(string[] args)
        {
            //Thread.Sleep(15000);
            //UniMessageBox.Show("Start Executor");
            Task.Run(WaitForExit);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Mutex mutex = new Mutex(true, "ExecutorProcess", out var canCreate);
            if (!canCreate)
            {
                UniMessageBox.Show("只能创建一个UniExecutor进程");
                Environment.Exit(-1);
            }

            if (args == null || args.Length == 0)
            {
                throw new Exception("需要传入参数");
            }

            var jobModel = JsonConvert.DeserializeObject<JobModel>(args[0]);
            App app = new App();
            app.Init(jobModel);
            app.Run();
        }

        private static void WaitForExit()
        {
            var parentProcess = GetParentProcess(Process.GetCurrentProcess().Handle);
            parentProcess.EnableRaisingEvents = true;
            var autoRest=new AutoResetEvent(false);
            parentProcess.Exited += (sender, args) => { autoRest.Set(); };
            autoRest.WaitOne();
            try
            {
                Application.Current.Shutdown();
            }
            finally
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            UniMessageBox.Show(e.ExceptionObject.ToString());
            Environment.Exit(-1);
        }

        /// <summary>
        /// Gets the parent process of a specified process.
        /// </summary>
        /// <param name="handle">The process handle.</param>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess(IntPtr handle)
        {
            PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
            int returnLength;
            int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
            if (status != 0)
                throw new Win32Exception(status);

            try
            {
                return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
            }
            catch (ArgumentException)
            {
                // not found
                return null;
            }
        }
        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref PROCESS_BASIC_INFORMATION processInformation, int processInformationLength, out int returnLength);
        public struct PROCESS_BASIC_INFORMATION
        {
            // These members must match PROCESS_BASIC_INFORMATION
            internal IntPtr Reserved1;
            internal IntPtr PebBaseAddress;
            internal IntPtr Reserved2_0;
            internal IntPtr Reserved2_1;
            internal IntPtr UniqueProcessId;
            internal IntPtr InheritedFromUniqueProcessId;
        }
    }
}
