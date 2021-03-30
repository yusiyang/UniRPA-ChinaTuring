using System.Activities.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UniCompiler.Utils
{
	internal class ReferencedAssembliesService : IReferencedAssembliesService
    {
        private Dictionary<string, Assembly> _referencedAssemblies = new Dictionary<string, Assembly>();

        public ReferencedAssembliesService(IEnumerable<string> dependencies)
        {
            foreach (string item in dependencies.Distinct())
            {
                Assembly assembly = SafeAssemblyLoader.TryGetAssemblyName(item);
                if (assembly != null)
                {
                    _referencedAssemblies[item] = assembly;
                }
            }
        }

        public void AddAssemblies(IEnumerable<AssemblyReference> assemblyReferences)
        {
            foreach (AssemblyReference assemblyReference in assemblyReferences)
            {
                if (!_referencedAssemblies.ContainsKey(assemblyReference.AssemblyName?.Name))
                {
                    Assembly assembly = assemblyReference.TryLoadAssembly();
                    if (assembly != null)
                    {
                        _referencedAssemblies[assemblyReference.AssemblyName?.Name] = assembly;
                    }
                }
            }
        }

        public Assembly[] GetAllReferences()
        {
            return _referencedAssemblies.Values.ToArray();
        }
    }
}
