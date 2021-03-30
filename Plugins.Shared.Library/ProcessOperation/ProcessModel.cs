using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.ProcessOperation
{
    public class ProcessModel
    {
        public string ProcessName { get; set; }

        public string FilePath { get; set; }

        public string Arguments { get; set; }

        public ProcessModel(string processName,object arguments=null,string filePath=null)
        {
            ProcessName = processName;
            if(string.IsNullOrWhiteSpace(filePath))
            {
                string currentWorkDirectory = Directory.GetCurrentDirectory();
                filePath = Path.Combine(currentWorkDirectory, $"{processName}.exe");
            }
            FilePath = filePath;
            if (arguments != null)
            {
                if (arguments.GetType() == typeof(string))
                {
                    Arguments = (string)arguments;
                }
                else
                {
                    Arguments = "\"" + JsonConvert.SerializeObject(arguments).Replace("\"", "\\\"") + "\"";
                }
            }
        }
    }
}
