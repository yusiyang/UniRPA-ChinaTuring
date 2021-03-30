using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniExecutor.Core.Models
{
    public class JobModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string JobName { get; set; }

        public Guid RobotId { get; set; }

        public string RobotName { get; set; }

        public ProcessInfo ProcessInfo { get; set; }
    }
}
