using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniWorkforce.Process.Models
{
    public class PublishProcessRequest
    {
        public string ProcessName{ get; set; }

        public string ProcessDescription { get; set; }

        public string FilePath { get; set; }

        public string Version { get; set; }

        public DateTime PublishTime { get; set; }
    }
}
