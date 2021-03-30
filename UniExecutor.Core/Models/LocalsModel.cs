using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniExecutor.Core.Models
{
    public class LocalsModel
    {
        public IDictionary<string, string> Variables { get; set; }

        public IDictionary<string, string> Arguments { get; set; }
    }
}
