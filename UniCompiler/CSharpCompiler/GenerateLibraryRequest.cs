using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UniCompiler.CSharpCompiler
{
    public class GenerateLibraryRequest
    {
        public string LibraryName
        {
            get;
            set;
        }

        public string RootPath
        {
            get;
            set;
        }

        public string AssemblyVersion
        {
            get;
            set;
        }

        public string OutputFolder
        {
            get;
            set;
        }

        public ConcurrentBag<ResourceDescription> AssemblyResources
        {
            get;
            set;
        }

        public Dictionary<string, (string classNamespace, string className)> PathsToClassNames
        {
            get;
            set;
        }

        public ConcurrentQueue<Lazy<GenerateActivityRequest>> ActivitiesToCompile
        {
            get;
            set;
        }
	}
}
