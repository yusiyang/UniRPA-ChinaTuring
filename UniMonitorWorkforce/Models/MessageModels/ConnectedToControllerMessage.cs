using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniWorkforce.Models.MessageModels
{
    public class ConnectedToControllerMessage
    {
        public string RobotId { get; set; }

        public ConnectedToControllerMessage(string robotId)
        {
            RobotId = robotId;
        }
    }
}
