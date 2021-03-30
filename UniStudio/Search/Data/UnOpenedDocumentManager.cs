using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniStudio.Search.Models;
using UniStudio.ViewModel;
using UniStudio.WorkflowOperation;

namespace UniStudio.Search.Data
{
    public class UnOpenedDocumentManager
    {
        public List<SimpleWorkflow> UnOpenedWorkflows { get;}

        public UnOpenedDocumentManager()
        {
            UnOpenedWorkflows = new List<SimpleWorkflow>();
        }

        private void GetXamlItems(List<ProjectTreeItem> projectTreeItems, List<ProjectTreeItem> xmalItems)
        {
            foreach (var projectTreeItem in projectTreeItems)
            {
                if (projectTreeItem.IsXaml)
                {
                    xmalItems.Add(projectTreeItem);
                }
                if (projectTreeItem.Children?.Count > 0)
                {
                    GetXamlItems(projectTreeItem.Children.ToList(), xmalItems);
                }
            }
        }

        private bool IsDocumentOpened(string xmalPath)
        {
            return ViewModelLocator.instance.Dock.Documents.Any(d => d.XamlPath == xmalPath);
        }

        private List<string> GetUnOpendDocuments()
        {
            var unOpenedDocuments = new List<string>();
            var xmalItems = new List<ProjectTreeItem>();
            GetXamlItems(ViewModelLocator.instance.Project.ProjectItems.ToList(), xmalItems);

            foreach (var xmalItem in xmalItems)
            {
                if (!IsDocumentOpened(xmalItem.Path))
                {
                    unOpenedDocuments.Add(xmalItem.Path);
                }
            }
            return unOpenedDocuments;
        }

        public List<SimpleWorkflow> CreateWorkflows()
        {
            var unOpenedDocuments = GetUnOpendDocuments();
            foreach (var filePath in unOpenedDocuments)
            {
                var workflow = UnOpenedWorkflows.FirstOrDefault(d => d.XmalPath == filePath);
                if (workflow != null)
                {
                    workflow.Update();
                }
                else
                {
                    workflow = new SimpleWorkflow();
                    workflow.Load(filePath);
                    UnOpenedWorkflows.Add(workflow);
                }
            }

            for(var i= UnOpenedWorkflows.Count-1;i>=0;i--)
            {
                if(!unOpenedDocuments.Contains(UnOpenedWorkflows[i].XmalPath))
                {
                    UnOpenedWorkflows.RemoveAt(i);
                }
            }

            return UnOpenedWorkflows;
        }
    }
}

