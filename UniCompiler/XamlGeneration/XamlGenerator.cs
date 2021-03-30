using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Validation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xaml;

namespace UniCompiler.XamlGeneration
{
	public class XamlGenerator
	{
		private sealed class Utf8StringWriter : StringWriter
		{
			public override Encoding Encoding => Encoding.UTF8;

			public Utf8StringWriter(StringBuilder sb)
				: base(sb)
			{
			}
		}

		public static string GenerateXaml(ActivityBuilder builder, Type fakeType, bool needCurrentDirectoryResolver)
		{
			object obj = Activator.CreateInstance(fakeType);
			foreach (DynamicActivityProperty property in builder.Properties)
			{
				obj.GetType().GetProperty(property.Name).SetValue(obj, property.Value, null);
			}
			string typeName = fakeType.FullName + ", " + fakeType.Assembly.GetName().Name;
			Activity value = needCurrentDirectoryResolver ? builder.Implementation.DecorateWithCurrentDirectoryResolver(typeName) : builder.Implementation;
			fakeType.GetProperty("Implementation").SetValue(obj, value, null);
			Collection<Constraint> collection = (Collection<Constraint>)(fakeType.GetProperty("Constraints")?.GetValue(obj, null));
			foreach (Constraint constraint in builder.Constraints)
			{
				collection?.Add(constraint);
			}
			IList<string> namespacesForImplementation = TextExpression.GetNamespacesForImplementation(builder);
			IList<AssemblyReference> referencesForImplementation = TextExpression.GetReferencesForImplementation(builder);
			if (needCurrentDirectoryResolver)
			{
				AddMandatoryNamespaceForImplementation(namespacesForImplementation);
			}
			TextExpression.SetNamespacesForImplementation(obj, namespacesForImplementation);
			TextExpression.SetReferencesForImplementation(obj, referencesForImplementation);
			using (XamlObjectReader xamlReader = new XamlObjectReader(obj))
			{
				StringBuilder stringBuilder = new StringBuilder();
				using (Utf8StringWriter utf8StringWriter = new Utf8StringWriter(stringBuilder))
				{
					using (XamlXmlWriter xamlXmlWriter = new XamlXmlWriter(utf8StringWriter, new XamlSchemaContext()))
					{
						XamlServices.Transform(xamlReader, xamlXmlWriter);
						xamlXmlWriter.Flush();
						utf8StringWriter.Flush();
					}
				}
				return stringBuilder.ToString();
			}
		}

		private static void AddMandatoryNamespaceForImplementation(IList<string> namespaces)
		{
			if (namespaces.All((string ns) => ns != "System.IO"))
			{
				namespaces.Add("System.IO");
			}
		}
	}
}
