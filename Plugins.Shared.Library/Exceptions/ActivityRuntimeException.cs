using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Exceptions
{
    public class ActivityRuntimeException : Exception
    {
        public ActivityRuntimeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
