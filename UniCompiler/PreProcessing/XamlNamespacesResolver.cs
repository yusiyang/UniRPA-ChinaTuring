using System.Collections.Generic;
using System.Linq;
using System.Xaml;

namespace UniCompiler.PreProcessing
{
	public class XamlNamespacesResolver : IXamlNamespaceResolver, INamespacePrefixLookup
    {
        public IDictionary<string, string> DocumentNamespaces
        {
            get;
            private set;
        }

        public XamlNamespacesResolver(IDictionary<string, string> documentNamespaces)
        {
            DocumentNamespaces = (documentNamespaces ?? new Dictionary<string, string>());
        }

        string IXamlNamespaceResolver.GetNamespace(string prefix)
        {
            if (DocumentNamespaces.TryGetValue(prefix, out string value))
            {
                return value;
            }
            return null;
        }

        IEnumerable<NamespaceDeclaration> IXamlNamespaceResolver.GetNamespacePrefixes()
        {
            return DocumentNamespaces.Select((KeyValuePair<string, string> kvp) => new NamespaceDeclaration(kvp.Value, kvp.Key));
        }

        string INamespacePrefixLookup.LookupPrefix(string ns)
        {
            return DocumentNamespaces.FirstOrDefault((KeyValuePair<string, string> kvp) => kvp.Value == ns).Key;
        }
    }
}
