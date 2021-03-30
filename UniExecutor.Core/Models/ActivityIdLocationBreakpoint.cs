using System;
using System.Activities.Presentation.Debug;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniExecutor.Core.Models
{
    public class ActivityIdLocationBreakpoint
    {
        public string ActivityId { get; set; }

        public ActivityLocation Location { get; set; }

        public BreakpointTypes BreakpointTypes { get; set; }
    }
}
