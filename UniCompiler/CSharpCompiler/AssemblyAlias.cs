using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace UniCompiler.CSharpCompiler
{
	public static class AssemblyAlias
    {
        private static readonly ConcurrentDictionary<Assembly, string> AliasCache = new ConcurrentDictionary<Assembly, string>();

        public static string GetAliasFor(Assembly assembly)
        {
            return AliasCache.GetOrAdd(assembly, (Assembly newAssembly) => $"A{newAssembly.GetHashCode()}");
        }

        public static IEnumerable<string> GetAliases(Assembly assembly)
        {
            yield return "global";
            yield return GetAliasFor(assembly);
        }
    }
}
