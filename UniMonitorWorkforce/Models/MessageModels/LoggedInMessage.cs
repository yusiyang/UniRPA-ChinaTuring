using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniWorkforce.Models.MessageModels
{
    public class LoggedInMessage
    {
        public string SessionId { get; set; }

        public LoggedInMessage(string sessionId)
        {
            SessionId = sessionId;
        }
    }
}
