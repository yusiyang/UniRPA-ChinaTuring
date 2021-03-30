using System.Collections.Concurrent;
using System.Reflection;

namespace UniCompiler.Common
{

	public static class AssemblyNameCache
    {
        private static readonly ConcurrentDictionary<string, string> _assemblyShortNames = new ConcurrentDictionary<string, string>();

        public static string GetCachedShortName(this Assembly assembly)
        {
            if (!(assembly == null))
            {
                return _assemblyShortNames.GetOrAdd(assembly.FullName, (string _) => assembly.GetName().Name);
            }
            return null;
        }

        public static void Invalidate()
        {
            _assemblyShortNames.Clear();
        }
    }
}
