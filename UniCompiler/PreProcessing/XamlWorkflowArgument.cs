using System.Xml;

namespace UniCompiler.PreProcessing
{
    internal class XamlWorkflowArgument
    {
        internal string Name
        {
            get;
            private set;
        }

        internal XmlNode XamlForm
        {
            get;
            private set;
        }

        internal string Kind
        {
            get;
            private set;
        }

        internal string XamlType
        {
            get;
            private set;
        }

        internal XamlWorkflowArgument(string name, string kind, string xamlType, XmlNode xamlNode)
        {
            Name = name;
            Kind = kind;
            XamlForm = xamlNode;
            XamlType = xamlType;
        }
    }
}