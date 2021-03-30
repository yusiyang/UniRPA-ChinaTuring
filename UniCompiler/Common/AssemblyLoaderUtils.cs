using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UniCompiler.Common
{
	public static class AssemblyLoaderUtils
	{
		public static IReadOnlyCollection<AssemblyLoadInfo> LoadAssemblies(string[] paths, Action verifyCancellation)
		{
			if (paths == null || paths.Length == 0)
			{
				return new List<AssemblyLoadInfo>();
			}
			Dictionary<string, string> asmNames = (from g in ((IEnumerable<string>)paths).Select((Func<string, (string, string)>)((string path) => (Path.GetFileNameWithoutExtension(path), path))).GroupBy((Func<(string, string), string>)(((string name, string path) t) => t.name))
												   select g.First()).ToDictionary(((string name, string path) t) => t.name, ((string name, string path) t) => t.path);
			Dictionary<AssemblyKey, Assembly> collection = AppDomain.CurrentDomain.GetAssemblies().GroupBy(AssemblyKey.Create).ToDictionary((IGrouping<AssemblyKey, Assembly> group) => group.Key, (IGrouping<AssemblyKey, Assembly> group) => group.First());
			ConcurrentDictionary<AssemblyKey, Assembly> cache = new ConcurrentDictionary<AssemblyKey, Assembly>(collection);
			Dictionary<string, string> binAssemblies = AssemblyUtils.GetBinAssemblies().ToDictionary(((string assemblyName, string assemblyPath) x) => x.assemblyName, ((string assemblyName, string assemblyPath) x) => x.assemblyPath);
			AssemblyLoadInfo[] results = new AssemblyLoadInfo[paths.Length];
			try
			{
				AppDomain.CurrentDomain.AssemblyResolve += LookForAssembly;
				Parallel.ForEach(Partitioner.Create(0, paths.Length, 5), delegate (Tuple<int, int> range)
				{
					for (int i = range.Item1; i < range.Item2; i++)
					{
						verifyCancellation?.Invoke();
						Assembly assembly;
						AssemblyLoadResultType resultType = TryLoadAssembly(cache, binAssemblies, paths[i], out assembly);
						results[i] = new AssemblyLoadInfo(assembly, new AssemblyLoadResult(paths[i], resultType, null), paths[i]);
					}
				});
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= LookForAssembly;
			}
			return results.Where((AssemblyLoadInfo o) => o != null).ToList();
			Assembly LookForAssembly(object sender, ResolveEventArgs args)
			{
				int num = args.Name.IndexOf(',');
				string key = (num > 0) ? args.Name.Substring(0, num) : args.Name;
				if (asmNames.TryGetValue(key, out string value))
				{
					try
					{
						return Assembly.LoadFrom(value);
					}
					catch (Exception exception)
					{
						exception.Trace();
					}
				}
				return null;
			}
		}

		private static AssemblyLoadResultType TryLoadAssembly(IDictionary<AssemblyKey, Assembly> cache, IReadOnlyDictionary<string, string> binAssemblies, string path, out Assembly assembly)
		{
			//Trace.TraceInformation("TryLoadAssembly: Loading " + path);
			assembly = null;
			if (AssemblyKey.TryCreate(path, out AssemblyName assemblyName, out AssemblyKey key))
			{
				if (cache.TryGetValue(key, out assembly))
				{
					return AssemblyLoadResultType.Ok;
				}
				if (TryLoadBinAssembly(binAssemblies, assemblyName, out assembly))
				{
					cache[key] = assembly;
					return AssemblyLoadResultType.Ok;
				}
			}
			try
			{
				assembly = Assembly.LoadFrom(path);
				assembly.GetCustomAttributes();
				cache[AssemblyKey.Create(assembly)] = assembly;
			}
			catch (BadImageFormatException ex)
			{
				//Trace.TraceError(ex.ToString());
				return AssemblyLoadResultType.Ignored;
			}
			catch (Exception ex2)
			{
				//Trace.TraceError(ex2.ToString());
				return AssemblyLoadResultType.LoadFromFailure;
			}
			return AssemblyLoadResultType.Ok;
		}

		private static bool TryLoadBinAssembly(IReadOnlyDictionary<string, string> binAssemblies, AssemblyName assemblyName, out Assembly assembly)
		{
			if (binAssemblies.TryGetValue(assemblyName.Name, out string value))
			{
				try
				{
					AssemblyName assemblyName2 = AssemblyName.GetAssemblyName(value);
					assembly = Assembly.Load(assemblyName2);
					return true;
				}
				catch (Exception ex)
				{
					//Trace.TraceError(ex.ToString());
				}
			}
			assembly = null;
			return false;
		}

		public static string Trace(this Exception exception, string label = null)
        {
            string arg = string.IsNullOrWhiteSpace(label) ? string.Empty : (label + ": ");
            string text = $"{arg}{exception}, HResult {exception.HResult}";
            System.Diagnostics.Trace.TraceError(text);
            return text;
        }
	}
}
