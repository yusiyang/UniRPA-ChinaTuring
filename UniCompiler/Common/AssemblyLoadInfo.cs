using System.Reflection;

namespace UniCompiler.Common
{
	public class AssemblyLoadInfo
    {
        public Assembly Assembly
        {
            get;
        }

        public AssemblyLoadResult LoadResult
        {
            get;
            set;
        }

        public AssemblyLoadResultType LoadResultType => LoadResult.ResultType;

        public string Path
        {
            get;
        }

        public AssemblyLoadInfo(Assembly assembly, AssemblyLoadResult loadResult, string path)
        {
            Assembly = assembly;
            LoadResult = loadResult;
            Path = path;
        }
    }
    public enum AssemblyLoadResultType
    {
        Ok,
        LoadFromFailure,
        GetExportedTypesFailure,
        Ignored
    }

}
