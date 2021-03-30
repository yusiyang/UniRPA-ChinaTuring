using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniNamedPipe.Models
{
    public class ApiInfo
    {
        public string PipeServerName { get; set; }

        public string ApiName { get; set; }

        public Type Type { get; set; }
    }
}
