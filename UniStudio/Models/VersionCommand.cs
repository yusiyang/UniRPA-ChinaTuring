using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniStudio.Models
{

    public class VersionCommand
    {
        public VersionCommand() { }

        public VersionCommand(int id,Version ver) {
            AppID = id;
            TargetVersion = ver;
        }

        public int AppID { get; set; }
        public Version TargetVersion { get; set; }
    }
}
