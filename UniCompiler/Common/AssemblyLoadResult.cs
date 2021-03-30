using System;

namespace UniCompiler.Common
{
	[Serializable]
    public readonly struct AssemblyLoadResult
    {
        public string Exception
        {
            get;
        }

        public string Path
        {
            get;
        }

        public AssemblyLoadResultType ResultType
        {
            get;
        }

        public AssemblyLoadResult(string path, AssemblyLoadResultType resultType, string exception)
        {
            Path = path;
            ResultType = resultType;
            Exception = exception;
        }

        public override string ToString()
        {
            return $"Result: {ResultType}, Path: {Path}";
        }
    }
}
