using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Plugins.Shared.Library.ProcessOperation
{
    public class SingleProcess
    {
        private static bool IsRunning(string processName)
        {
            var process = Process.GetProcessesByName(processName);
            return process?.Length>0;
        }

        /// <summary>
        /// 打开进程
        /// </summary>
        public static void Start(ProcessModel processInfo,bool throwExceptionWhenExist=true)
        {
            if (IsRunning(processInfo.ProcessName))
            {
                if(!throwExceptionWhenExist)
                {
                    return;
                }
                throw new InvalidOperationException($"只能同时执行一个{processInfo.ProcessName}");
            }

            var mutex = new Mutex(false, processInfo.ProcessName);
            try
            {
                mutex.WaitOne();

                if (IsRunning(processInfo.ProcessName))
                {
                    if (!throwExceptionWhenExist)
                    {
                        return;
                    }
                    throw new InvalidOperationException($"只能同时执行一个{processInfo.ProcessName}");
                }

                var process = new Process();
                process.StartInfo.FileName = processInfo.FilePath;
                process.StartInfo.Arguments = processInfo.Arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.UseShellExecute = true;
                process.EnableRaisingEvents = false;
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
