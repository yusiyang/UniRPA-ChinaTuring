using System.Text.RegularExpressions;

namespace UniCompiler.PreProcessing
{
    public class WorkflowArgument
    {
        private const string InvokeWorkflowFileInArgumentTagName = "InArgument";

        public const string PropertyKindRegexGroup = "Kind";

        public const string PropertyFullTypeRegexGroup = "FullType";

        public const string PropertyTypeRegexGroup = "Type";

        private static readonly string _propertyXamlRegex;

        private static readonly string _typeXamlRegex;

        internal static Regex RegPropertyXaml
        {
            get;
            private set;
        }

        internal static Regex RegTypeXaml
        {
            get;
            private set;
        }

        internal string Name
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

        internal string XamlFullType
        {
            get;
            private set;
        }

        internal bool Required
        {
            get;
            private set;
        }

        internal bool HasDefaultValue
        {
            get;
            private set;
        }

        internal bool IsInArgument => "InArgument" == Kind;

        static WorkflowArgument()
        {
            _propertyXamlRegex = "^(?<Kind>\\w*)\\((?<FullType>\\w*:(?<Type>.*))\\)$";
            _typeXamlRegex = ".*?:(?<Type>.*)";
            RegPropertyXaml = new Regex(_propertyXamlRegex, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
            RegTypeXaml = new Regex(_typeXamlRegex, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        }

        internal WorkflowArgument(string argumentName, string type, bool isRequired = false, bool hasDefaultValue = false)
        {
            Match match = RegPropertyXaml.Match(type);
            if (match.Success)
            {
                string value = match.Groups["Kind"].Value;
                string value2 = match.Groups["Type"].Value;
                string value3 = match.Groups["FullType"].Value;
                Name = argumentName;
                Kind = value;
                XamlType = value2;
                XamlFullType = value3;
                Required = isRequired;
                HasDefaultValue = hasDefaultValue;
            }
        }
    }
}