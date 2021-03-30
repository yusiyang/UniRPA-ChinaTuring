using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniExecutor.Core.Events
{
    public class InternalEndRunEventArgs : EventArgs
    {
        public IDictionary<string, object> Outputs { get; set; }

        public InternalEndRunEventArgs(IDictionary<string, object> outputs)
        {
            Outputs = outputs;
        }
    }
}
