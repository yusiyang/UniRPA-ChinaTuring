using System.Collections.Generic;
using System.Xaml;

namespace UniCompiler.PreProcessing
{
	public class XamlTypeResolution
    {
        public XamlType ResolvedXamlType
        {
            get;
            internal set;
        }

        public HashSet<string> UnknownTypeSpecifications
        {
            get;
            private set;
        }

        public XamlTypeResolution()
        {
            ResolvedXamlType = null;
            UnknownTypeSpecifications = new HashSet<string>();
        }
    }

}
