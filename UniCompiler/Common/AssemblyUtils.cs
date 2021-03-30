using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace UniCompiler.Common
{
	public static class AssemblyUtils
	{
		//private static readonly string[] ExcludedAssemblies = (((NameValueCollection)ConfigurationManager.GetSection("excludedAssemblies")) ?? new NameValueCollection()).AllKeys;
        private static readonly string[] ExcludedAssemblies = new string[]{};

		public static string DeploymentVersion
		{
			get;
		} = typeof(AssemblyUtils).Assembly.GetName().Version.ToString();


		public static AssemblyName GetAssemblyName(string filePath)
		{
			using (FileStream peStream = File.OpenRead(filePath))
			{
				using (PEReader pEReader = new PEReader(peStream))
				{
					if (pEReader.HasMetadata)
					{
						MetadataReader metadataReader = pEReader.GetMetadataReader();
						if (metadataReader.IsAssembly)
						{
							AssemblyName assemblyName = metadataReader.GetAssemblyDefinition().GetAssemblyName();
							if (assemblyName.CultureInfo == null)
							{
								assemblyName.CultureInfo = CultureInfo.InvariantCulture;
							}
							if (assemblyName.GetPublicKey() == null)
							{
								assemblyName.SetPublicKey(Array.Empty<byte>());
							}
							return assemblyName;
						}
					}
				}
			}
			return null;
		}


		public static Assembly LatestOrDefault(string name, IEnumerable<Assembly> assemblies = null)
		{
			if (assemblies == null)
			{
				assemblies = AppDomain.CurrentDomain.GetAssemblies();
			}
			string shortName = new AssemblyName(name).Name;
			Assembly assembly = (from a in assemblies
								 let assemblyIdentity = a.GetName()
								 where assemblyIdentity.Name.Equals(shortName, StringComparison.OrdinalIgnoreCase)
								 orderby assemblyIdentity.Version descending
								 select a).FirstOrDefault();
			if (assembly != null)
			{
				Trace.TraceInformation($"Redirected {name} to {assembly}.");
			}
			return assembly;
		}

		public static List<(string assemblyName, string assemblyPath)> GetBinAssemblies()
		{
			List<(string, string)> list = new List<(string, string)>();
			try
			{
				foreach (string item in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll"))
				{
					try
					{
						list.Add((Path.GetFileNameWithoutExtension(item), item));
					}
					catch (Exception ex)
					{
						Trace.TraceError(ex.ToString());
					}
				}
				return list;
			}
			catch (Exception ex2)
			{
				Trace.TraceError(ex2.ToString());
				return list;
			}
		}

		public static IReadOnlyCollection<string> ExcludeLegacyActivities(this IReadOnlyCollection<string> assemblies)
		{
			return (IReadOnlyCollection<string>)(object)assemblies.Where((string a) => ExcludedAssemblies.All((string legacy) => a.IndexOf(legacy, StringComparison.OrdinalIgnoreCase) < 0)).ToArray();
		}
	}
}
