using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniExecutor.Core.Models
{
    public class DebugOptions
    {
        public Dictionary<string, object> InputArguments { get; set; }

        public List<ActivityIdLocationBreakpoint> ActivityIdLocationBreakpointList { get; set; }

        public List<string> VariableNames { get; set; }        

        public int SpeedType { get; set; }

        public int DebugOperate { get; set; }

        public bool NeedStepInto { get; set; } = true;
    }
}
