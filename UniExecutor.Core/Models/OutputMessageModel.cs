using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniExecutor.Core.Models
{
    public class OutputMessageModel
    {
        public int OutputType { get; set; }

        public string Message { get; set; }

        public string MessageDetail { get; set; }
    }
}
