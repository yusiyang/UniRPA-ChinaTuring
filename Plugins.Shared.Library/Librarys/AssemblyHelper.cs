using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Librarys
{
    public class AssemblyHelper
    {        
        public static IEnumerable<Assembly> GetAllDependencies(Assembly assembly)
        {
            var dict = new Dictionary<string, AssemblyName>();
            dict.Add(assembly.GetName().FullName, assembly.GetName());
            dict = GetAllDependenciesRecursive(assembly.GetName(), dict);
            return dict.Select(d => Assembly.Load(d.Value)).ToArray();
        }

        private static Dictionary<string, AssemblyName> GetAllDependenciesRecursive(AssemblyName assemblyName, Dictionary<string, AssemblyName> existingRefList)
        {
            var assembly = Assembly.Load(assemblyName);
            if (!assemblyName.FullName.ToLower().Contains("sqlsugar"))
            {
                var assemblies = assembly.GetReferencedAssemblies();
                foreach (var refAssemblyName in assemblies)
                {
                    if (!existingRefList.ContainsKey(refAssemblyName.FullName))
                    {
                        existingRefList.Add(refAssemblyName.FullName, refAssemblyName);
                        existingRefList = GetAllDependenciesRecursive(refAssemblyName, existingRefList);
                    }
                }
            }
            return existingRefList;
        }
    }
}
