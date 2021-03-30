using System.Collections.Concurrent;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UniCompiler.CSharpCompiler
{
	public static class ActivityFactoryCache
    {
        public static ConcurrentDictionary<string, ObjectCreationExpressionSyntax> AssemblyNamesForImplementationCache
        {
            get;
        } = new ConcurrentDictionary<string, ObjectCreationExpressionSyntax>();


        public static ConcurrentDictionary<string, LiteralExpressionSyntax> NamespacesForImplementationCache
        {
            get;
        } = new ConcurrentDictionary<string, LiteralExpressionSyntax>();


        public static void ClearCache()
        {
            AssemblyNamesForImplementationCache.Clear();
            NamespacesForImplementationCache.Clear();
        }
    }
}
