using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace UniCompiler.CSharpCompiler
{
    class AssemblyInfoFactory
    {
        private const string CompilerVersionMacro = "{uiPathCompilerVersion}";

        private const string AssemblyVersionMacro = "{assemblyVersion}";

        public static SyntaxTree GenerateAssemblyInfo(string assemblyVersion)
        {
            var assemblyInfoTemplate = @"using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyVersion(""{assemblyVersion}"")]
[assembly: AssemblyFileVersion(""{assemblyVersion}"")]
[assembly: AssemblyMetadata(""UniCompilerVersion"",""{uniCompilerVersion}"")]";
            return SyntaxFactory.ParseSyntaxTree(assemblyInfoTemplate.Replace("{uniCompilerVersion}", Assembly.GetExecutingAssembly().GetName().Version.ToString()).Replace("{assemblyVersion}", assemblyVersion));
        }
	}
}
