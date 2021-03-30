using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniWorkforce.Process.Models
{
    public class CheckProcessInfo
    {
        public int Code { get; set; }

        public string Version { get; set; }

        public string Message { get; set; }

        public string RobotTypeID { get; set; }

        public string RobotType { get; set; }

        public CheckProcessInfo()
        { }

        public CheckProcessInfo(int code,string version,string robotTypeID,string robotType)
        {
            Code = code;
            Version = version;
            RobotTypeID = robotTypeID;
            RobotType = robotType;
        }
    }
}
