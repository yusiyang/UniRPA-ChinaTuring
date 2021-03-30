using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xaml;
using System.Xaml.Schema;
using UniCompiler.Common;

namespace UniCompiler.PreProcessing
{
	public static class WorkflowTypeParser
	{
		internal class XamlTypeResolutionContext
		{
			public string SourceXamlTypeDefinition
			{
				get;
				private set;
			}

			public XamlNamespacesResolver XamlNamespacesResolver
			{
				get;
				private set;
			}

			public XamlSchemaContext XamlSchemaContext
			{
				get;
				private set;
			}

			public IUnknownXamlTypeResolver UnknownTypeResolver
			{
				get;
				private set;
			}

			public XamlTypeResolutionContext(string sourceXamlTypeDefinition, IDictionary<string, string> documentNamespaces, XamlSchemaContext xamlTypeResolver, IUnknownXamlTypeResolver unknownTypeResolver = null)
			{
				SourceXamlTypeDefinition = sourceXamlTypeDefinition;
				XamlNamespacesResolver = new XamlNamespacesResolver(documentNamespaces);
				XamlSchemaContext = xamlTypeResolver;
				UnknownTypeResolver = unknownTypeResolver;
			}
		}

		private const string Namespace = "Namespace";

		private const string Type = "Type";

		private const string Arguments = "Arguments";

		private const string identifier = "[^\\(\\)\\,\\s\\.\\,][^\\(\\)\\,]*";

		private static readonly string xamlTypeRegex;

		private static readonly string xamlTypeArgumentsRegex;

		private static readonly HashSet<char> _dictionaryAllowedChars;

		internal static Regex RegXamlType
		{
			get;
			private set;
		}

		internal static Regex RegXamlTypeArguments
		{
			get;
			private set;
		}

		static WorkflowTypeParser()
		{
			xamlTypeRegex = "^(?<Namespace>[^\\(\\)\\,\\s\\.\\,][^\\(\\)\\,]*):(?<Type>[^\\(\\)\\,\\s\\.\\,][^\\(\\)\\,]*)(\\((?<Arguments>.*)\\))*$";
			xamlTypeArgumentsRegex = "([^\\(\\)\\,\\s\\.\\,][^\\(\\)\\,]*)\r\n                        (\r\n                            \\(\r\n                            (?:                 \r\n                            [^\\(\\)]               \r\n                            |\r\n                            (?<open> \\( )       \r\n                            |\r\n                            (?<close-open> \\) )     \r\n                            )*\r\n                            \\)\r\n                        )?\r\n                        (?(open)(?!))";
			_dictionaryAllowedChars = new HashSet<char>
		{
			'[',
			']',
			','
		};
			RegXamlType = new Regex(xamlTypeRegex, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
			RegXamlTypeArguments = new Regex(xamlTypeArgumentsRegex, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
		}

		private static bool IsArrayType(XamlTypeName xamlTypeName, XamlNamespacesResolver xamlNamespacesResolver, out string arrayItemType, out string arraySpecification)
		{
			arrayItemType = null;
			arraySpecification = null;
			if (string.IsNullOrWhiteSpace(xamlNamespacesResolver.DocumentNamespaces.FirstOrDefault((KeyValuePair<string, string> kvp) => kvp.Value == xamlTypeName.Namespace).Key))
			{
				return false;
			}
			string text = xamlTypeName.ToString(xamlNamespacesResolver);
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			int num = text.Length;
			Dictionary<char, int> dictionary = new Dictionary<char, int>();
			while (num > 0 && (_dictionaryAllowedChars.Contains(text[num - 1]) || char.IsWhiteSpace(text[num - 1])))
			{
				char key = text[num - 1];
				if (!dictionary.TryGetValue(key, out int value))
				{
					value = 0;
				}
				value = (dictionary[key] = value + 1);
				num--;
			}
			if (!dictionary.ContainsKey('[') || !dictionary.ContainsKey(']') || !dictionary.Keys.All((char ch) => ch == '[' || ch == ']' || ch == ',' || char.IsWhiteSpace(ch)))
			{
				return false;
			}
			arrayItemType = text.Substring(0, num);
			arraySpecification = text.Substring(num);
			return true;
		}

		private static XamlType ResolveUnknownType(string xamlTypeDefinition, XamlTypeResolutionContext context, XamlTypeResolution result)
		{
			result.UnknownTypeSpecifications.Add(xamlTypeDefinition);
			Type type = context.UnknownTypeResolver?.ResolveUnknownType(xamlTypeDefinition, context.XamlSchemaContext);
			if (type == null)
			{
				return null;
			}
			return new XamlType(type, context.XamlSchemaContext);
		}

		private static XamlType ResolveUnknownType(XamlTypeName xamlTypeName, XamlTypeResolutionContext context, XamlTypeResolution result)
		{
			if (IsArrayType(xamlTypeName, context.XamlNamespacesResolver, out string arrayItemType, out string arraySpecification))
			{
				Type type = InternalParseXamlType(arrayItemType, context, result)?.UnderlyingType;
				if (type != null)
				{
					Type type2 = System.Type.GetType(type.FullName + arraySpecification + ", " + type.Assembly.GetCachedShortName(), throwOnError: false);
					if (type2 != null)
					{
						return new XamlType(type2, context.XamlSchemaContext);
					}
				}
			}
			return ResolveUnknownType(xamlTypeName.ToString(context.XamlNamespacesResolver), context, result);
		}

		private static void CheckTypeArgumentsForUnknownType(XamlTypeName xamlTypeName, XamlTypeResolutionContext context, XamlTypeResolution result)
		{
			if (xamlTypeName == null || xamlTypeName.TypeArguments == null || xamlTypeName.TypeArguments.Count == 0)
			{
				return;
			}
			XamlSchemaContext xamlSchemaContext = context.XamlSchemaContext;
			for (int i = 0; i < xamlTypeName.TypeArguments.Count; i++)
			{
				XamlTypeName xamlTypeName2 = xamlTypeName.TypeArguments[i];
				IList<XamlTypeName> typeArguments = xamlTypeName2.TypeArguments;
				if (typeArguments != null && typeArguments.Count > 0)
				{
					CheckTypeArgumentsForUnknownType(xamlTypeName2, context, result);
				}
				XamlType xamlType = xamlSchemaContext.GetXamlType(xamlTypeName2);
				if (!(xamlType != null))
				{
					xamlType = ResolveUnknownType(xamlTypeName2, context, result);
					if (xamlType != null)
					{
						xamlTypeName.TypeArguments[i] = new XamlTypeName(xamlType);
					}
				}
			}
		}

		private static XamlType InternalParseXamlType(string xamlTypeDefinition, XamlTypeResolutionContext context, XamlTypeResolution result)
		{
			if (!XamlTypeName.TryParse(xamlTypeDefinition, context.XamlNamespacesResolver, out XamlTypeName result2) || result2 == null)
			{
				return null;
			}
			IList<XamlTypeName> typeArguments = result2.TypeArguments;
			if (typeArguments != null && typeArguments.Count > 0)
			{
				CheckTypeArgumentsForUnknownType(result2, context, result);
			}
			XamlType xamlType = context.XamlSchemaContext.GetXamlType(result2);
			if (xamlType != null)
			{
				return xamlType;
			}
			return ResolveUnknownType(result2, context, result);
		}

		public static XamlTypeResolution ResolveXamlType(string xamlTypeDefinition, IDictionary<string, string> documentNamespaces, XamlSchemaContext xamlTypeResolver, IUnknownXamlTypeResolver unknownTypeResolver = null)
		{
			if (xamlTypeResolver == null || documentNamespaces == null)
			{
				return null;
			}
			XamlTypeResolutionContext context = new XamlTypeResolutionContext(xamlTypeDefinition, documentNamespaces, xamlTypeResolver, unknownTypeResolver);
			XamlTypeResolution xamlTypeResolution = new XamlTypeResolution();
			xamlTypeResolution.ResolvedXamlType = InternalParseXamlType(xamlTypeDefinition, context, xamlTypeResolution);
			return xamlTypeResolution;
		}

		public static Type ResolveType(string xamlTypeDefinition, IDictionary<string, string> documentNamespaces, XamlSchemaContext xamlTypeResolver)
		{
			if (xamlTypeResolver == null || documentNamespaces == null)
			{
				return null;
			}
			return ResolveXamlType(xamlTypeDefinition, documentNamespaces, xamlTypeResolver).ResolvedXamlType?.UnderlyingType;
		}
	}

}
