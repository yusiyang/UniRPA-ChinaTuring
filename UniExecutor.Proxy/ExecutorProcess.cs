using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniExecutor.Core.Models;
using UniNamedPipe;

namespace UniExecutor.Proxy
{
    public class ExecutorProcess
    {
        private static bool IsExecutorRunning()
        {
            var process = Process.GetProcessesByName("UniExecutor");
            return process?.Length>0;
        }

        /// <summary>
        /// 打开Executor
        /// </summary>
        public static void StartExecutor(JobModel jobModel)
        {
            if (IsExecutorRunning())
            {
                throw new InvalidOperationException("只能同时执行一个流程");
            }

            var mutex = new Mutex(false, "ExecutorProcess");
            try
            {
                mutex.WaitOne();

                if (IsExecutorRunning())
                {
                    throw new InvalidOperationException("只能同时执行一个流程");
                }

                string currentWorkDirectory = Directory.GetCurrentDirectory();
                var process = new Process();
                process.StartInfo.FileName = Path.Combine(currentWorkDirectory, "UniExecutor.exe");
                process.StartInfo.Arguments = "\"" + JsonConvert.SerializeObject(jobModel).Replace("\"", "\\\"") + "\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.EnableRaisingEvents = true;

                Configure.ConfigureServer();
                process.Start();
                //Thread.Sleep(15000);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}
