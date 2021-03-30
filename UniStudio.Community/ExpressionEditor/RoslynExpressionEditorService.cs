using System;
using System.Activities.Presentation.Hosting;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Plugins.Shared.Library.CodeCompletion;

namespace UniStudio.Community.ExpressionEditor
{
    public class RoslynExpressionEditorService : IExpressionEditorService
    {
        public void CloseExpressionEditors()
        {

        }

        public IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, List<ModelItem> variables, string text)
        {
            return CreateExpressionEditor(assemblies, importedNamespaces, variables, text, null, Size.Empty);
        }

        public IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, List<ModelItem> variables, string text, Size initialSize)
        {
            return CreateExpressionEditor(assemblies, importedNamespaces, variables, text, null, initialSize);
        }

        public IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, List<ModelItem> variables, string text, Type expressionType)
        {
            return CreateExpressionEditor(assemblies, importedNamespaces, variables, text, expressionType, Size.Empty);
        }

        public IExpressionEditorInstance CreateExpressionEditor(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces, List<ModelItem> variables, string text, Type expressionType, Size initialSize)
        {
            var editor = new RoslynExpressionEditorInstance();
            editor.UpdateInstance(variables, text, importedNamespaces);
            editor.RegisterKeyCommand(new KeyBinding(new RelayCommand(
                editor.ExcuteKeyCommand),
                Key.K,
                ModifierKeys.Control));
            //生成命名空间树
            editor.m_namespaceNodeRoot = AddNamespacesToAutoCompletionList(EditorUtil.autoCompletionTree, importedNamespaces);

            return editor;
        }

        public void UpdateContext(AssemblyContextControlItem assemblies, ImportedNamespaceContextItem importedNamespaces)
        {

        }

        private ExpressionNode AddNamespacesToAutoCompletionList(ExpressionNode data, ImportedNamespaceContextItem importedNamespaces)
        {
            foreach (var ns in importedNamespaces.ImportedNamespaces)
            {
                var foundNodes = ExpressionNode.SearchForNode(data, ns, true, true);
                foreach (var node in foundNodes.Nodes)
                {
                    data.Add(node);
                }
            }
            return data;
        }



    }

}
