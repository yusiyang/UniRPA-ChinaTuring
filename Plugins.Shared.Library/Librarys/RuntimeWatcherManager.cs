using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Librarys
{
    public class RuntimeWatcherManager
    {
        public static RuntimeWatcherManager Instance { get; }

        private Dictionary<string, RuntimeWatcher> _nameWatcherDic = new Dictionary<string, RuntimeWatcher>();

        static RuntimeWatcherManager()
        {
            Instance = new RuntimeWatcherManager();
        }

        public RuntimeWatcher StartRuntimeWatcher(string name)
        {
            return _nameWatcherDic.Locking(a =>
            {
                var watcher = new RuntimeWatcher(name);
                _nameWatcherDic[name] = watcher;
                return watcher;
            });
        }

        public void Trace(string name,string traceName)
        {
            if (_nameWatcherDic.TryGetValue(name, out var watcher))
            {
                watcher.Add(traceName);
            }
        }

        public void EndRuntimeWatcher(string name)
        {
            _nameWatcherDic.Locking(a =>
            {
                if (a.TryGetValue(name, out var watcher))
                {
                    watcher.Dispose();
                    a.Remove(name);
                }
            });
        }
    }
}
