using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Enums;

namespace UniExecutor.Output
{
    public class OutputHelper
    {
        private static OutputDelegate _output;

        public static void AddOutput(OutputDelegate outputDelegate)
        {
            if(_output==null)
            {
                _output = outputDelegate;
            }
            else
            {
                _output += outputDelegate;
            }
        }

        public static void Output(OutputTypeEnum outputType, string message, string messageDetail=null)
        {
            _output?.Invoke(outputType, message, messageDetail);
        }
    }
}
