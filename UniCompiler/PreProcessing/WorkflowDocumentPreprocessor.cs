using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using UniCompiler.Logging;

namespace UniCompiler.PreProcessing
{
	public class WorkflowDocumentPreprocessor : IWorkflowDocumentPreprocessor
	{
		internal const string ClrNs = "clr-namespace:";

		private const string XamlNs = "http://schemas.microsoft.com/winfx/2006/xaml";

		private const string UiPathXamlNs = "http://schemas.uipath.com/workflow/activities";

		private const string InvokeWorkflowFileTagName = "InvokeWorkflowFile";

		private const string InvokeWorkflowInteractiveFileTagName = "InvokeWorkflowInteractive";

		private const string InvokeWorkflowFileNameAttribute = "WorkflowFileName";

		private const string InvokeWorkflowFileKeyAttribute = "Key";

		private const string UnSafeAttribute = "UnSafe";

		private const string CommentOutTagName = "CommentOut";

		private const string XamlClassAttributeName = "Class";

		internal const string InvokeWorkflowFileInArgumentTagName = "InArgument";

		private const string InvokeWorkflowFileOutArgumentTagName = "OutArgument";

		private const string InvokeWorkflowFileInOutArgumentTagName = "InOutArgument";

		private const string InvokeWorkflowFileArgumentKeyAttributeName = "Key";

		private const string InvokeWorkflowFileArgumentTypeAttributeName = "TypeArguments";

		private const string PropertyGroupTag = "Members";

		private const string PropertyTag = "Property";

		private const string PropertyNameAttribute = "Name";

		private const string PropertyTypeAttribute = "Type";

		private const string PropertyAttributes = "Property.Attributes";

		private const string RequiredArgumentAttribute = "RequiredArgumentAttribute";

		private const string ActivityNode = "Activity";

		private readonly string _path;

		private readonly HashSet<string> _privateActivities;

		private readonly string _libraryName;

		private readonly string _rootCategory;

		public WorkflowDocumentPreprocessor(string path, string libraryName, string rootCategory, string[] privateActivities)
		{
			_path = path;
			string fullPath = Path.GetFullPath(path);
			_privateActivities = new HashSet<string>(privateActivities.Select((string s) => WfDocUtils.NormalizeDirectorySeparator(Path.Combine(fullPath, s))), StringComparer.InvariantCultureIgnoreCase);
			_libraryName = libraryName;
			_rootCategory = rootCategory;
		}

		public WorkflowDocument Load(string xamlFile)
		{
			bool publishActivity = !_privateActivities.Contains(WfDocUtils.NormalizeDirectorySeparator(xamlFile));
			WorkflowDocument workflowDocument = new WorkflowDocument(xamlFile, _path, _libraryName, _rootCategory, publishActivity);
			RemoveCommentOutActivities(workflowDocument);
			ProcessWorkflowArguments(workflowDocument);
			ProcessInvokeActivity(workflowDocument);
			ProcessInvokeInteractiveActivity(workflowDocument);
			return workflowDocument;
		}

		private void RemoveCommentOutActivities(WorkflowDocument wfDoc)
		{
			XmlNodeList elementsByTagName = wfDoc.Document.GetElementsByTagName("CommentOut", "http://schemas.uipath.com/workflow/activities");
			if (elementsByTagName != null)
			{
				List<XmlElement> list = new List<XmlElement>();
				foreach (XmlElement item in elementsByTagName)
				{
					list.Add(item);
				}
				foreach (XmlElement item2 in list)
				{
					if (item2.ParentNode != null)
					{
						item2.ParentNode.RemoveChild(item2);
					}
				}
			}
		}

		private void ProcessWorkflowArguments(WorkflowDocument wfDoc)
		{
			XmlNodeList elementsByTagName = wfDoc.Document.GetElementsByTagName("Members", "http://schemas.microsoft.com/winfx/2006/xaml");
			HashSet<string> allArgumentsWithDefaultValues = GetAllArgumentsWithDefaultValues(wfDoc);
			foreach (XmlElement item in elementsByTagName)
			{
				foreach (XmlElement item2 in item.GetElementsByTagName("Property", "http://schemas.microsoft.com/winfx/2006/xaml"))
				{
					try
					{
						string value = item2.Attributes["Name"].Value;
						string value2 = item2.Attributes["Type"].Value;
						bool isRequired = IsRequired(item2);
						bool hasDefaultValue = allArgumentsWithDefaultValues.Contains(value);
						WorkflowArgument workflowArgument = new WorkflowArgument(value, value2, isRequired, hasDefaultValue);
						if (workflowArgument.Name != null)
						{
							wfDoc.Arguments.Add(workflowArgument);
						}
					}
					catch
					{
						//throw new WorkflowArgumentsException(string.Format(Resources.WorkflowDocumentPreprocessor_ProcessWorkflowArguments_Unable_to_parse_workflow_arguments, wfDoc.DocumentPath));
						throw new Exception("");
					}
				}
			}
		}

		private static HashSet<string> GetAllArgumentsWithDefaultValues(WorkflowDocument wfDoc)
		{
			HashSet<string> hashSet = new HashSet<string>();
			string xamlClassName = GetXamlClassName(wfDoc);
			if (string.IsNullOrWhiteSpace(xamlClassName))
			{
				return hashSet;
			}
			int length = (xamlClassName + ".").Length;
			foreach (XmlElement item in wfDoc.Document.GetElementsByTagName("Activity"))
			{
				foreach (XmlAttribute attribute in item.Attributes)
				{
					if ("clr-namespace:" == attribute.NamespaceURI && attribute.LocalName.Length >= length)
					{
						hashSet.Add(attribute.LocalName.Substring(length));
					}
				}
				foreach (XmlElement childNode in item.ChildNodes)
				{
					if ("clr-namespace:" == childNode.NamespaceURI && childNode.LocalName.Length >= length)
					{
						hashSet.Add(childNode.LocalName.Substring(length));
					}
				}
			}
			return hashSet;
		}

		private static string GetXamlClassName(WorkflowDocument wfDoc)
		{
			string text = null;
			foreach (XmlElement item in wfDoc.Document.GetElementsByTagName("Activity"))
			{
				text = item.GetAttribute("Class", "http://schemas.microsoft.com/winfx/2006/xaml");
				if (!string.IsNullOrWhiteSpace(text))
				{
					return text;
				}
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				//Logger.WriteLine(string.Format(Resources.WorkflowDocumentPreprocessor_GetXamlClassName_Unable_to_read_xaml_class_name_in_document_, wfDoc.DocumentAbsolutePath), Categories.Preprocessing, TraceEventType.Warning);
			}
			return null;
		}

		private static bool IsRequired(XmlElement property)
		{
			bool result = false;
			foreach (XmlElement item in property.GetElementsByTagName("Property.Attributes", "http://schemas.microsoft.com/winfx/2006/xaml"))
			{
				if (item.GetElementsByTagName("RequiredArgumentAttribute", "http://schemas.microsoft.com/winfx/2006/xaml") != null)
				{
					return true;
				}
			}
			return result;
		}

		private void ProcessInvokeActivity(WorkflowDocument wfDoc)
		{
			List<Tuple<XmlElement, XmlElement>> list = new List<Tuple<XmlElement, XmlElement>>();
			foreach (XmlElement item in wfDoc.Document.GetElementsByTagName("InvokeWorkflowFile", "http://schemas.uipath.com/workflow/activities"))
			{
				ValidateInvokeElement(wfDoc, item);
				(string, string, string) invokedElementProperties = GetInvokedElementProperties(item);
				XamlWorkflowArgument[] invokedArguments = GetInvokedArguments(item);
				string invokedElementKeyAttribute = GetInvokedElementKeyAttribute(item);
				XmlElement xmlElement2 = CreateActivityInvokeElement(wfDoc.Document, invokedElementProperties.Item1, invokedElementProperties.Item2, invokedArguments);
				AddKeyAttributeToActivityElement(wfDoc.Document, xmlElement2, invokedElementKeyAttribute);
				wfDoc.Dependencies.Add(new WorkFlowInvokeDependency
				{
					Name = invokedElementProperties.Item1,
					Arguments = invokedArguments,
					DocumentAbsolutePath = invokedElementProperties.Item3
				});
				list.Add(Tuple.Create(item, xmlElement2));
			}
			foreach (Tuple<XmlElement, XmlElement> item2 in list)
			{
				item2.Item1.ParentNode.ReplaceChild(item2.Item2, item2.Item1);
			}
		}

		private string GetInvokedElementKeyAttribute(XmlElement invokeElement)
		{
			return invokeElement.Attributes["Key", "http://schemas.microsoft.com/winfx/2006/xaml"]?.Value;
		}

		private void AddKeyAttributeToActivityElement(XmlDocument xmlDocument, XmlElement xmlElement, string value)
		{
			if (value != null)
			{
				XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("x", "Key", "http://schemas.microsoft.com/winfx/2006/xaml");
				xmlAttribute.InnerText = value;
				xmlElement.SetAttributeNode(xmlAttribute);
			}
		}

		private void ProcessInvokeInteractiveActivity(WorkflowDocument wfDoc)
		{
			foreach (XmlElement item in wfDoc.Document.GetElementsByTagName("InvokeWorkflowInteractive", "http://schemas.uipath.com/workflow/activities"))
			{
				string value = item.Attributes["WorkflowFileName"].Value;
				if (!string.IsNullOrWhiteSpace(value))
				{
					wfDoc.RunInteractiveWorkflows.Add(value);
				}
			}
		}

		private XmlElement CreateActivityInvokeElement(XmlDocument xmlDocument, string className, string nameSpace, IEnumerable<XamlWorkflowArgument> arguments)
		{
			string namespaceURI = "clr-namespace:" + nameSpace + ";assembly=" + _libraryName;
			XmlElement xmlElement = xmlDocument.CreateElement(className, namespaceURI);
			foreach (XamlWorkflowArgument argument in arguments)
			{
				XmlNode xmlNode = xmlDocument.CreateNode(XmlNodeType.Element, className + "." + argument.Name, namespaceURI);
				xmlNode.AppendChild(argument.XamlForm);
				xmlElement.AppendChild(xmlNode);
			}
			return xmlElement;
		}

		private void ValidateInvokeElement(WorkflowDocument wfDoc, XmlElement invokeElement)
		{
			string documentPath = wfDoc.DocumentPath;
			string value = invokeElement.Attributes["WorkflowFileName"].Value;
			XmlAttribute xmlAttribute = invokeElement.Attributes["UnSafe"];
			if (xmlAttribute != null && !string.IsNullOrEmpty(xmlAttribute.Value) && true.Equals(Convert.ToBoolean(xmlAttribute.Value)))
			{
				wfDoc.RunInIsolationWorkflows.Add(value);
			}
			if (Regex.Match(value, "^\\[.*\\]$").Success)
			{
				//throw new WorkflowInvokeExpressionException(Resources.Unsupported_Invoke_Workflow_Expression, documentPath, value);
				throw new Exception("");
			}
		}

		private (string computedClassName, string computedNamespace, string invokedWfAbsolutePath) GetInvokedElementProperties(XmlElement invokeElement)
		{
			string value = invokeElement.Attributes["WorkflowFileName"].Value;
			string fullPath = Path.GetFullPath(_path);
			string correctCaseFileRelativePath = WfDocUtils.GetCorrectCaseFileRelativePath(fullPath, value, Categories.Preprocessing);
			string item = Path.Combine(fullPath, correctCaseFileRelativePath);
			string item2 = WfDocUtils.ComputeClassName(correctCaseFileRelativePath);
			string item3 = WfDocUtils.ComputeQualifiedName(correctCaseFileRelativePath, _path, _libraryName);
			return (item2, item3, item);
		}

		private XamlWorkflowArgument[] GetInvokedArguments(XmlElement invokeElement)
		{
			XmlNodeList elementsByTagName = invokeElement.GetElementsByTagName("InArgument");
			XmlNodeList elementsByTagName2 = invokeElement.GetElementsByTagName("OutArgument");
			XmlNodeList elementsByTagName3 = invokeElement.GetElementsByTagName("InOutArgument");
			return (from arg in new XmlNodeList[3]
				{
				elementsByTagName,
				elementsByTagName2,
				elementsByTagName3
				}.SelectMany((XmlNodeList s) => s.OfType<XmlNode>()).Select(GetArgument)
					where arg != null
					select arg).ToArray();
		}

		private XamlWorkflowArgument GetArgument(XmlNode xmlNode)
		{
			string innerText = xmlNode.InnerText;
			string name = xmlNode.Name;
			if (name == "OutArgument" && string.IsNullOrEmpty(innerText))
			{
				return null;
			}
			string name2 = xmlNode.Attributes?["Key", "http://schemas.microsoft.com/winfx/2006/xaml"].Value;
			string xamlType = xmlNode.Attributes?["TypeArguments", "http://schemas.microsoft.com/winfx/2006/xaml"].Value;
			return new XamlWorkflowArgument(name2, name, xamlType, xmlNode);
		}
	}
}
