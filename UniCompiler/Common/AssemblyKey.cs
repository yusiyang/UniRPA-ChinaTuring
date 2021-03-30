using System;
using System.Diagnostics;
using System.Reflection;

namespace UniCompiler.Common
{
	public readonly struct AssemblyKey : IEquatable<AssemblyKey>
    {
        public static readonly AssemblyKey Empty = new AssemblyKey(null, null);

        public string Name
        {
            get;
        }

        public string Version
        {
            get;
        }

        public AssemblyKey(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public bool Equals(AssemblyKey other)
        {
            if (Name == other.Name)
            {
                return Version == other.Version;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AssemblyKey))
            {
                return false;
            }
            return Equals((AssemblyKey)obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Version.GetHashCode();
        }

        public override string ToString()
        {
            return "Name: " + Name + ", Version: " + Version;
        }

        public static AssemblyKey Create(Assembly a)
        {
            AssemblyName assemblyName = new AssemblyName(a.FullName);
            return new AssemblyKey(assemblyName.Name, assemblyName.Version.ToString());
        }

        public static bool TryCreate(string assemblyPath, out AssemblyName assemblyName, out AssemblyKey key)
        {
            try
            {
                assemblyName = AssemblyUtils.GetAssemblyName(assemblyPath);
                if (assemblyName == null)
                {
                    key = default(AssemblyKey);
                    return false;
                }
                key = new AssemblyKey(assemblyName.Name, assemblyName.Version.ToString());
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
            assemblyName = null;
            key = Empty;
            return false;
        }
    }
}
