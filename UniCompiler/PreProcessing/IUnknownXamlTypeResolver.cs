using System;
using System.Xaml;

namespace UniCompiler.PreProcessing
{
    public interface IUnknownXamlTypeResolver
    {
        Type ResolveUnknownType(string xamlTypeDefinition, XamlSchemaContext xamlSchemaContext);
    }
}
