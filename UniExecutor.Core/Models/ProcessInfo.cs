using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniExecutor.Core.Models
{
    public class ProcessInfo
    {
        public string ProcessName { get; set; }

        public string InstalledPackagesDir { get; set; }

        public string Version { get; set; }

        public string Main { get; set; }

        private string _processDir;
        public string ProcessDir
        {
            get
            {
                if (_processDir == null)
                {
                    if (!string.IsNullOrWhiteSpace(InstalledPackagesDir) &&
                        !string.IsNullOrWhiteSpace(ProcessName) &&
                        !string.IsNullOrWhiteSpace(Version))
                    {
                        _processDir = Path.Combine(InstalledPackagesDir, $"{ProcessName}.{Version}", "lib\net452");
                    }
                }
                return _processDir;
            }
            set
            {
                _processDir = value;
            }
        }

        public string MainPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_processDir))
                {
                    throw new Exception("过程路径没有设置");
                }
                return Path.Combine(ProcessDir, Main);
            }
        }

        public Dictionary<string, string> Dependencies { get; set; } = new Dictionary<string, string>();
    }
}
