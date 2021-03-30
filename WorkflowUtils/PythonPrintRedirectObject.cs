using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowUtils
{
    /// <summary>
    ///  重定向Python脚本中的print到控制台输出
    /// </summary>
    public class PythonPrintRedirectObject
    {
        public static PythonPrintRedirectObject instance = null;

        static PythonPrintRedirectObject()
        {
            instance = new PythonPrintRedirectObject();
        }

        public void write(string str)
        {
            if (str != "\n")
            {
                Console.WriteLine(str);
            }
        }
    }
}
