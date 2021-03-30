using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Enums;

namespace UniExecutor.Output
{
    public delegate void OutputDelegate(OutputTypeEnum outputType, string message, string messageDetail);
}
