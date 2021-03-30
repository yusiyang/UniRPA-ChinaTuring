using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UniCompiler.Logging;

namespace UniCompiler.PreProcessing
{
	public class WorkflowInvokeListBuilder
	{
		private readonly string _path;

		private readonly IWorkflowDocumentPreprocessor _workflowDocumentPreprocessor;

		private readonly XamlTypeResolver _xamlTypeResolver;

		public WorkflowInvokeListBuilder(string path, string libraryName, string rootCategory, XamlTypeResolver xamlTypeResolver, string[] privateActivities)
		{
			_path = path;
			_xamlTypeResolver = xamlTypeResolver;
			_workflowDocumentPreprocessor = new WorkflowDocumentPreprocessor(_path, libraryName, rootCategory, privateActivities);
		}

		public List<WorkflowDocument> BuildList()
		{
			string[] filesToScan = Directory.EnumerateFiles(_path, "*.xaml", SearchOption.AllDirectories).Where(IsXamlCompilable).ToArray();
			if (filesToScan.Length == 0)
			{
				//throw new UiPathLocalizedException(string.Format(Resources.WorkflowInvokeListBuilder_BuildList_The_specified_folder_has_no_files_, _path), Categories.Preprocessing);
				throw new Exception("");
			}
			List<WorkflowDocument> list = filesToScan.Select(delegate (string file, int idx)
			{
				WorkflowDocument doc = null;
				LoggingManager.ExecuteXamlDocumentLoader(delegate
				{
					doc = _workflowDocumentPreprocessor.Load(file);
				}, file, idx, filesToScan.Length);
				return doc;
			}).ToList();
			//Logger.WriteLine(string.Format(Resources.WorkflowInvokeListBuilder_BuildList_Found_documents_, list.Count), Categories.Preprocessing);
			//Logger.WriteLine(Resources.WorkflowInvokeListBuilder_BuildList_Verifying_dependencies_, Categories.Preprocessing);
			CheckMissingDependencies(list);
			CheckInvokedWorkflowsInIsolation(list);
			CheckInvokeInteractiveWorkflows(list);
			CheckArgumentsMismatch(list);
			//Logger.WriteLine(Resources.WorkflowInvokeListBuilder_BuildList_Verifying_namespaces_and_class_names_, Categories.Preprocessing);
			CheckDuplicateNamings(list);
			//Logger.WriteLine(Resources.WorkflowInvokeListBuilder_BuildList_Ordering_the_documents_list_based_on_dependencies, Categories.Preprocessing);
			return GetOrderedInvokeWorkflows(list).ToList();
		}

		private bool IsXamlCompilable(string filePath)
		{
			FileInfo fileInfo = new FileInfo(filePath);
			if (!fileInfo.Exists)
			{
				return false;
			}
			if (Path.GetFileName(filePath).StartsWith("~"))
			{
				return false;
			}
			if (fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
			{
				return false;
			}
			DirectoryInfo directoryInfo = fileInfo.Directory;
			string text = WfDocUtils.GetNormalizedDirectoryFullPath(_path).ToLower();
			string text2 = WfDocUtils.GetNormalizedDirectoryFullPath(directoryInfo.FullName).ToLower();
			while (text2 != text && text2.StartsWith(text))
			{
				if (directoryInfo.Attributes.HasFlag(FileAttributes.Hidden))
				{
					return false;
				}
				directoryInfo = directoryInfo.Parent;
				text2 = WfDocUtils.GetNormalizedDirectoryFullPath(directoryInfo.FullName).ToLower();
			}
			return true;
		}

		private void CheckArgumentsMismatch(List<WorkflowDocument> wfDocuments)
		{
			foreach (WorkflowDocument wfDocument in wfDocuments)
			{
				foreach (WorkFlowInvokeDependency invokedDependency in wfDocument.Dependencies)
				{
					WorkflowDocument workflowDocument = wfDocuments.FirstOrDefault((WorkflowDocument dep) => dep.DocumentAbsolutePath == invokedDependency.DocumentAbsolutePath);
					if (workflowDocument != null)
					{
						CheckMandatoryParameters(wfDocument, invokedDependency, workflowDocument);
						CheckInvokedParameters(wfDocument, invokedDependency, workflowDocument);
					}
				}
			}
		}

		private void CheckMandatoryParameters(WorkflowDocument doc, WorkFlowInvokeDependency invokedDependency, WorkflowDocument dependencyDocument)
		{
			foreach (WorkflowArgument argument in dependencyDocument.Arguments.Where((WorkflowArgument a) => a.Required && !a.HasDefaultValue && a.IsInArgument))
			{
				if (invokedDependency.Arguments.FirstOrDefault((XamlWorkflowArgument arg) => arg.Name == argument.Name) == null)
				{
					//throw new WorkflowInvocationArgumentMismatch(string.Format(Resources.WorkflowInvokeListBuilder_CheckArgumentsMismatch_Workflow_is_invoking_workflow_with_missing_argument_, doc.DocumentPath, invokedDependency.Name, argument.Name));
					throw new Exception("");
				}
			}
		}

		private void CheckInvokedParameters(WorkflowDocument doc, WorkFlowInvokeDependency invokedDependency, WorkflowDocument dependencyDocument)
		{
			XamlWorkflowArgument[] arguments = invokedDependency.Arguments;
			int num = 0;
			XamlWorkflowArgument argument;
			Type type;
			Type type2;
			while (true)
			{
				if (num >= arguments.Length)
				{
					return;
				}
				argument = arguments[num];
				WorkflowArgument workflowArgument = dependencyDocument.Arguments.FirstOrDefault((WorkflowArgument arg) => arg.Name == argument.Name);
				if (workflowArgument == null)
				{
					//throw new WorkflowInvocationArgumentMismatch(string.Format(Resources.WorkflowInvokeListBuilder_CheckArgumentsMismatch_Workflow_is_invoking_workflow_with_extra_argument_, doc.DocumentPath, invokedDependency.Name, argument.Name));
					throw new Exception("");
				}
				if (workflowArgument.Kind != argument.Kind)
				{
					//throw new WorkflowInvocationArgumentMismatch(string.Format(Resources.WorkflowInvokeListBuilder_CheckArgumentsMismatch_Workflow_invoking_workflow_with_argument_of_different_kind_, doc.DocumentPath, invokedDependency.Name, argument.Name, workflowArgument.Kind, argument.Kind));
                    throw new Exception("");
				}
				if (workflowArgument.XamlFullType != argument.XamlType)
				{
					type = WorkflowTypeParser.ResolveType(argument.XamlType, doc.DocumentNamespaces, _xamlTypeResolver);
					if (type == null)
					{
						//throw new WorkflowInvocationUnresolvedArgumentType(doc.ClassName, argument.XamlType, argument.Name);
						throw new Exception("");
					}
					type2 = WorkflowTypeParser.ResolveType(workflowArgument.XamlFullType, dependencyDocument.DocumentNamespaces, _xamlTypeResolver);
					if (type2 == null)
					{
						//throw new WorkflowInvocationUnresolvedArgumentType(invokedDependency.Name, workflowArgument.XamlType, workflowArgument.Name);
                        throw new Exception("");
					}
					if (!type2.IsAssignableFrom(type))
					{
						break;
					}
				}
				num++;
			}
			//throw new WorkflowInvocationArgumentMismatch(string.Format(Resources.WorkflowInvokeListBuilder_CheckArgumentsMismatch_IncompatibleInvocationType, doc.DocumentPath, invokedDependency.Name, argument.Name, type2.Name, type?.Name));
            throw new Exception("");
		}

		private void CheckDuplicateNamings(List<WorkflowDocument> wfDocuments)
		{
			IGrouping<string, WorkflowDocument>[] array = (from doc in wfDocuments
														   group doc by doc.Category + "." + doc.ClassName into @group
														   where @group.Count() > 1
														   select @group).ToArray();
			if (array.Any())
			{
				//throw new WorkflowInvocationDuplicateNameException(array);
                throw new Exception("");
			}
		}

		private static void CheckMissingDependencies(List<WorkflowDocument> wfDocuments)
		{
			string[] second = wfDocuments.Select((WorkflowDocument doc) => doc.DocumentAbsolutePath).ToArray();
			string[] array = (from dep in wfDocuments.SelectMany((WorkflowDocument s) => s.Dependencies)
							  select dep.DocumentAbsolutePath).Distinct().Except(second).ToArray();
			Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
			string[] array2 = array;
			foreach (string missingDependency in array2)
			{
				string[] array3 = (from doc in wfDocuments
								   where doc.Dependencies.Any((WorkFlowInvokeDependency dep) => dep.DocumentAbsolutePath == missingDependency)
								   select doc.DocumentAbsolutePath).ToArray();
				if (!dictionary.ContainsKey(missingDependency))
				{
					dictionary.Add(missingDependency, array3);
				}
				else
				{
					dictionary[missingDependency] = dictionary[missingDependency].Concat(array3).ToArray();
				}
			}
			if (array.Length != 0)
			{
				//throw new WorkflowInvocationMissingDependeciesException(dictionary);
                throw new Exception("");
			}
		}

		private static void CheckInvokedWorkflowsInIsolation(List<WorkflowDocument> wfDocuments)
		{
			CheckWorkflowsWithUnsupportedItems(wfDocuments, (WorkflowDocument wd) => wd.RunInIsolationWorkflows, "WorkflowInvokeListBuilder_CheckRunInIsolationWorkflows", "WorkflowInvokeListBuilder_RunInIsolationWorkflowList");
		}

		private static void CheckInvokeInteractiveWorkflows(List<WorkflowDocument> wfDocuments)
		{
			CheckWorkflowsWithUnsupportedItems(wfDocuments, (WorkflowDocument wd) => wd.RunInteractiveWorkflows, "WorkflowDocumentPreprocessor_ProcessInvokeInteractiveActivity_NotAllowedTitle", "WorkflowDocumentPreprocessor_ProcessInvokeInteractiveActivity_NotAllowedItem");
		}

		private static void CheckWorkflowsWithUnsupportedItems(IList<WorkflowDocument> wfDocuments, Func<WorkflowDocument, IList<string>> targetList, string headerMessage, string itemMessage)
		{
			if (wfDocuments != null && targetList != null)
			{
				List<WorkflowDocument> list = wfDocuments.Where((WorkflowDocument doc) => targetList(doc).Count > 0).ToList();
				if (list.Count != 0)
				{
					StringBuilder stringBuilder = new StringBuilder(headerMessage + Environment.NewLine);
					foreach (WorkflowDocument item in list)
					{
						string arg = string.Join("WorkflowInvokeListBuilder_ErrorMessageListSeparator", targetList(item).ToArray());
						stringBuilder.Append(Environment.NewLine);
						stringBuilder.Append(string.Format(itemMessage, item.DocumentAbsolutePath, arg));
					}
					//throw new UiPathLocalizedException(stringBuilder.ToString(), Categories.Preprocessing);
                    throw new Exception("");
				}
			}
		}

		private static IEnumerable<WorkflowDocument> GetOrderedInvokeWorkflows(List<WorkflowDocument> documents)
		{
			List<WorkflowDocument> list = new List<WorkflowDocument>();
			List<WorkflowDocument> list2 = documents.Select((WorkflowDocument doc) => new WorkflowDocument(doc)).ToList();
			Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
			foreach (WorkflowDocument document in documents)
			{
				dictionary[document.DocumentAbsolutePath.ToLower()] = document.Dependencies.Select((WorkFlowInvokeDependency wi) => wi.DocumentAbsolutePath.ToLower()).Distinct().ToList();
			}
			while (list.Count < documents.Count)
			{
				int num = 0;
				for (int i = 0; i < list2.Count; i++)
				{
					WorkflowDocument workflowDocument = list2[i];
					string text = workflowDocument.DocumentAbsolutePath.ToLower();
					if (dictionary[text].Count <= 0)
					{
						list.Add(workflowDocument);
						list2.RemoveAt(i);
						UpdateDependencies(list2, dictionary, text);
						num++;
					}
				}
				if (num == 0)
				{
					//throw new WorkflowInvocationCycleException(Logger.FormatErrorList(Resources.WorkflowInvokeListBuilder__Unsupported_Workflow_Cycle, list2.Select((WorkflowDocument wd) => wd.DocumentPath).ToList(), addQuotesToErrors: true));
					throw new Exception("");
				}
			}
			return list;
		}

		private static void UpdateDependencies(List<WorkflowDocument> documents, Dictionary<string, List<string>> dependenciesList, string documentAbsolutePathToRemove)
		{
			foreach (WorkflowDocument document in documents)
			{
				List<string> list = dependenciesList[document.DocumentAbsolutePath.ToLower()];
				int num = list.FindIndex((string path) => path.Equals(documentAbsolutePathToRemove, StringComparison.CurrentCultureIgnoreCase));
				if (num >= 0)
				{
					list.RemoveAt(num);
				}
			}
		}
	}
}
