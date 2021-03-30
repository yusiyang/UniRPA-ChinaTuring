using System.Activities.Presentation;
using System.Collections.Generic;
using Plugins.Shared.Library.Librarys;
using UniStudio.Community.Executor;
using UniStudio.Community.ViewModel;

namespace UniStudio.Community
{
    public class DocumentContext
    {
        private static Dictionary<DocumentViewModel, DocumentContext> _docContextDic;

        static DocumentContext()
        {
            _docContextDic = new Dictionary<DocumentViewModel, DocumentContext>();
        }

        public DocumentViewModel DocumentViewModel { get; }

        public WorkflowDesigner WorkflowDesigner { get; }

        public string RelativeFilePath => DocumentViewModel.RelativeXamlPath;

        private DocumentContext(DocumentViewModel documentViewModel)
        {
            DocumentViewModel = documentViewModel;
            WorkflowDesigner = documentViewModel.WorkflowDesignerInstance;
        }

        public static DocumentContext Current
        {
            get
            {
                return _docContextDic.Locking(d =>
                {
                    if (!_docContextDic.TryGetValue(ViewModelLocator.instance.Dock.ActiveDocument, out var context))
                    {
                        context = new DocumentContext(ViewModelLocator.instance.Dock.ActiveDocument);
                        _docContextDic.Add(ViewModelLocator.instance.Dock.ActiveDocument, context);
                    }
                    return context;
                });
            }
        }

        public static DocumentContext Create(DocumentViewModel documentViewModel)
        {
            return _docContextDic.Locking(d =>
            {
                if (!_docContextDic.TryGetValue(documentViewModel, out var context))
                {
                    context = new DocumentContext(documentViewModel);
                    _docContextDic.Add(documentViewModel, context);
                }
                return context;
            });
        }

        public static DocumentContext GetContext(DocumentViewModel documentViewModel)
        {
            if (!_docContextDic.TryGetValue(documentViewModel, out var context))
            {
                return null;
            }
            return context;
        }

        public static void Remove(DocumentViewModel documentViewModel)
        {
            _docContextDic.Locking(d =>
            {
                if(_docContextDic.ContainsKey(documentViewModel))
                {
                    _docContextDic.Remove(documentViewModel);
                }
            });
        }

        private DebuggerManager _debuggerManager;

        public DebuggerManager DebuggerManager
        {
            get
            {
                if(_debuggerManager==null)
                {
                    _debuggerManager = new DebuggerManager(this);
                }
                return _debuggerManager;
            }
        }

        public EditingContext WorkflowContext => WorkflowDesigner.Context;

        public ServiceManager Services => WorkflowContext.Services;
    }
}
