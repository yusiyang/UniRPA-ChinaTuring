using System;
using System.Activities.Expressions;
using System.Diagnostics;
using System.Reflection;

namespace UniCompiler.Utils
{
	public static class SafeAssemblyLoader
    {
        public static Assembly TryLoadAssembly(this AssemblyReference reference)
        {
            try
            {
                reference.LoadAssembly();
                return reference.Assembly;
            }
            catch (Exception arg)
            {
                Trace.WriteLine(string.Format("{0} exception {1}", "TryLoadAssembly", arg));
            }
            return null;
        }

        public static Assembly TryGetAssemblyName(string assemblyFullName)
        {
            if (string.IsNullOrWhiteSpace(assemblyFullName))
            {
                return null;
            }
            try
            {
                return Assembly.Load(assemblyFullName);
            }
            catch (Exception arg)
            {
                Trace.WriteLine(string.Format("{0} exception {1}", "TryGetAssemblyName", arg));
            }
            return null;
        }
    }
}
