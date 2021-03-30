using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Exceptions
{
    [Serializable]
    public class BusinessException : Exception
    {
        public BusinessException(string message):base(message)
        {
        }

        public BusinessException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public BusinessException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize data for our base classes.  base will verify info != null.
            base.GetObjectData(info, context);

        }
    }
}
