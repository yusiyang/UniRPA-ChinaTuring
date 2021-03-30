using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UniNamedPipe.Attributes
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface)]
    public class PipeServerAttribute:Attribute
    {
        public PipeServerAttribute(string pipeName,string name=null)
        {
            PipeName = pipeName;
            Name = name??MethodBase.GetCurrentMethod().DeclaringType.Name;
        }

        public string PipeName { get; set; }

        public string Name { get; set; }
    }
}
