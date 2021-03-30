using Microsoft.CodeAnalysis.VisualBasic;
using Plugins.Shared.Library.Librarys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.CodeCompletion
{
    public class VBCompilationManager
    {
        private static Dictionary<string, VBCompilation> _assemblyCompilationDic;

        static VBCompilationManager()
        {
            _assemblyCompilationDic = new Dictionary<string, VBCompilation>();
        }

        public static VBCompilation Create(string assemblyName)
        {
            return _assemblyCompilationDic.Locking(a =>
            {
                if (!a.TryGetValue(assemblyName, out var vbCompilation))
                {
                    vbCompilation = new VBCompilation(assemblyName);
                    a.Add(assemblyName, vbCompilation);
                }
                return vbCompilation;
            });
        }

        public static void Remove(string assemblyName)
        {
            _assemblyCompilationDic.Locking(a =>
            {
                if(a.TryGetValue(assemblyName,out var vbCompilation))
                {
                    vbCompilation.Dispose();
                    a.Remove(assemblyName);
                }
            });
        }
    }
}
