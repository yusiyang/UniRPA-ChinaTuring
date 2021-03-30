using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Librarys
{
    public class RuntimeWatcher:IDisposable
    {
        private Dictionary<string,double> watcherDic=new Dictionary<string, double>();
        private Stopwatch stopwatch=new Stopwatch();
        private string watchName = null;
        private Action<string> log = null;

        public RuntimeWatcher(string name=null,Action<string> watchLog=null)
        {
            if (name.IsNullOrWhiteSpace())
            {
                var trace=new StackTrace();
                var callMethodInfo = trace.GetFrame(1).GetMethod();
                name = $"{callMethodInfo.DeclaringType.Name}_{callMethodInfo.Name}";
            }

            watchName = name;
            watcherDic.Add(watchName, 0);
            log = watchLog ?? WriteLog;
            stopwatch.Start();
        }

        private void WriteLog(string info)
        {
            SharedObject.Instance.Output(SharedObject.OutputType.Trace, info);
        }

        public void Add(string name)
        {
            stopwatch.Stop();
            watcherDic.Add(name, stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
        }

        public void End()
        {
            stopwatch.Stop();
            watcherDic[watchName] = watcherDic.Sum(w => w.Value) + stopwatch.ElapsedMilliseconds;

            if (log != null)
            {
                var logInfo = JsonConvert.SerializeObject(watcherDic);
                log.BeginInvoke(logInfo,null,null);
            }
        }

        public void Dispose()
        {
            End();
        }
    }
}
