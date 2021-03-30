using System;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UniCompiler.CSharpCompiler
{
	public static class LibraryFactoryCache
    {
        public static ConcurrentDictionary<ObjectFactoryContext, ConcurrentDictionary<(string, Type), ObjectCreationExpressionSyntax>> InArgumentsCache
        {
            get;
        } = new ConcurrentDictionary<ObjectFactoryContext, ConcurrentDictionary<(string, Type), ObjectCreationExpressionSyntax>>();


        public static void ClearCache()
        {
            InArgumentsCache.Clear();
        }
    }
}
