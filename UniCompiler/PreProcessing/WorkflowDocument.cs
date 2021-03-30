using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace UniCompiler.PreProcessing
{
	public class WorkflowDocument
	{
		public string DocumentPath
		{
			get;
		}

		public string DocumentAbsolutePath
		{
			get;
		}

		public string RootPath
		{
			get;
		}

		public XmlDocument Document
		{
			get;
		}

		internal List<WorkflowArgument> Arguments
		{
			get;
		}

		public List<WorkFlowInvokeDependency> Dependencies
		{
			get;
		}

		public string ClassName
		{
			get;
		}

		public string ClassNamespace
		{
			get;
		}

		public string Category
		{
			get;
		}

		public bool PublishActivity
		{
			get;
		}

		public List<string> RunInIsolationWorkflows
		{
			get;
			private set;
		}

		public List<string> RunInteractiveWorkflows
		{
			get;
			private set;
		}

		public IDictionary<string, string> DocumentNamespaces
		{
			get;
			private set;
		}

		public WorkflowDocument(string filePath, string rootPath, string libraryName, string rootCategory, bool publishActivity)
		{
			XmlReader reader = XmlReader.Create(filePath, new XmlReaderSettings
			{
				DtdProcessing = DtdProcessing.Ignore
			});
			XmlDocument xmlDocument = new XmlDocument
			{
				XmlResolver = null
			};
			xmlDocument.Load(reader);
			Document = xmlDocument;
			DocumentAbsolutePath = Path.GetFullPath(filePath);
			DocumentPath = filePath;
			RootPath = rootPath;
			PublishActivity = publishActivity;
			Arguments = new List<WorkflowArgument>();
			Dependencies = new List<WorkFlowInvokeDependency>();
			ClassName = WfDocUtils.ComputeClassName(DocumentPath);
			Category = WfDocUtils.ComputeQualifiedName(DocumentPath, RootPath, rootCategory, escapeRootNamespace: false);
			ClassNamespace = WfDocUtils.ComputeQualifiedName(DocumentPath, RootPath, libraryName);
			InternalInitialize();
		}

		public WorkflowDocument(WorkflowDocument doc)
		{
			Document = doc.Document;
			DocumentPath = doc.DocumentPath;
			DocumentAbsolutePath = doc.DocumentAbsolutePath;
			RootPath = doc.RootPath;
			ClassName = doc.ClassName;
			ClassNamespace = doc.ClassNamespace;
			Category = doc.Category;
			PublishActivity = doc.PublishActivity;
			Arguments = new List<WorkflowArgument>(doc.Arguments);
			Dependencies = new List<WorkFlowInvokeDependency>(doc.Dependencies);
			InternalInitialize();
		}

		private void InternalInitialize()
		{
			RunInIsolationWorkflows = new List<string>();
			RunInteractiveWorkflows = new List<string>();
			XPathNavigator xPathNavigator = new XPathDocument(new StringReader(Document.OuterXml)).CreateNavigator();
			xPathNavigator.MoveToFollowing(XPathNodeType.Element);
			DocumentNamespaces = xPathNavigator.GetNamespacesInScope(XmlNamespaceScope.All);
		}

		public override string ToString()
		{
			return DocumentPath;
		}
	}

}