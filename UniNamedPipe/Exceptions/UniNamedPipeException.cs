using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UniNamedPipe.Exceptions
{
    public class UniNamedPipeException:Exception
    {
        public UniNamedPipeException(string message,int code=1):base(message)
        {
            Code = code;
        }

        public UniNamedPipeException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public UniNamedPipeException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize data for our base classes.  base will verify info != null.
            base.GetObjectData(info, context);

        }

        public int Code { get; } = 1;
    }
}
