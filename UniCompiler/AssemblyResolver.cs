using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniCompiler.Common;

namespace UniCompiler
{
	public static class AssemblyResolver
	{
		public static IEnumerable<Assembly> LoadDependencies(string studioFolder, IEnumerable<string> dependencies)
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
			IReadOnlyCollection<AssemblyLoadInfo> readOnlyCollection = AssemblyLoaderUtils.LoadAssemblies(dependencies.ToArray(), null);
			foreach (AssemblyLoadInfo item in readOnlyCollection)
			{
				LogAssemblyLoadedResult(item);
			}
			return (from info in readOnlyCollection
					where info.LoadResultType == AssemblyLoadResultType.Ok
					select info.Assembly).ToList();
		}

		public static void End()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
		}

		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			try
			{
				AssemblyName assemblyName = new AssemblyName(args.Name);
				Assembly assembly = AssemblyUtils.LatestOrDefault(args.Name) ?? LoadRetargetableAssembly(assemblyName);
				if (assembly != null)
				{
					//Logger.WriteLine(string.Format(Resources.Loading_dependency_assembly, assembly.Location), Categories.Compiler, TraceEventType.Verbose);
				}
				return assembly;
			}
			catch (Exception ex)
			{
				//Logger.WriteError(ex.Message);
				return null;
			}
		}

		private static Assembly LoadRetargetableAssembly(AssemblyName assemblyName)
		{
			if (!assemblyName.Flags.HasFlag(AssemblyNameFlags.Retargetable))
			{
				return null;
			}
			try
			{
				AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
				return Assembly.Load(assemblyName);
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
			}
		}

		private static void LogAssemblyLoadedResult(AssemblyLoadInfo assemblyLoadInfo)
		{
			if (assemblyLoadInfo.LoadResultType == AssemblyLoadResultType.Ok)
			{
				//Logger.WriteLine(string.Format(Resources.AssemblyResolver_TryLoadAssembly_Loaded_dependency_, assemblyLoadInfo.Path), Categories.Initialization, TraceEventType.Verbose);
			}
			else
			{
				//Logger.WriteLine(string.Format(Resources.XamlTypeResolver_Initialize_Unable_To_Load_Specified_Dependency, assemblyLoadInfo.Path, assemblyLoadInfo.LoadResultType.ToString()), Categories.Initialization, TraceEventType.Verbose);
			}
		}
	}
}
