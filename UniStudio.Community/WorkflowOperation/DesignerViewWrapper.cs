using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Plugins.Shared.Library.Extensions;
using ReflectionMagic;
using UniStudio.Community.Executor;
using UniStudio.Community.ViewModel;
using WorkflowUtils;

namespace UniStudio.Community.WorkflowOperation
{
    public class DesignerViewWrapper
    {
        public DocumentContext DocumentContext { get; }

        public DesignerView DesignerView { get; }

        public ModelItem SelectedModelItem => DesignerView.Context.Items.GetValue<Selection>()?.SelectedObjects?.FirstOrDefault();

        public DesignerViewWrapper(DocumentContext context)
        {
            DocumentContext = context;
            DesignerView = DocumentContext.Services.GetService<DesignerView>();

            AddCommandBindings();
            SetContextMenu();

            DesignerView.Context.Services.Publish(this);
        }

        private void AddCommandBindings()
        {
            DesignerView.CommandBindings.RemoveAt(6);

            var commentOutBinding = new CommandBinding(DesignerViewCommands.CommentOutCommand, CommentOutExecuted, CommentOutCanExecute);
            var commentOutDelBinding=new CommandBinding(DesignerViewCommands.CommentOutDelCommand, CommentOutDelExecuted, CommentOutDelCanExecute);
            var openWorkflowBinding = new CommandBinding(DesignerViewCommands.OpenWorkflowCommand, OpenWorkflowExecuted, OpenWorkflowCanExecute);
            var surroundedWithTryCatchBinding = new CommandBinding(DesignerViewCommands.SurroundedWithTryCatchCommand, SurroundedWithTryCatchExecuted, SurroundedWithTryCatchCanExecute);

            DesignerView.CommandBindings.Add(commentOutBinding);
            DesignerView.CommandBindings.Add(commentOutDelBinding);
            DesignerView.CommandBindings.Add(openWorkflowBinding);
            DesignerView.CommandBindings.Add(surroundedWithTryCatchBinding);
        }

        private void SetContextMenu()
        {
            DesignerView.ContextMenu.Items.Add(System.Windows.Forms.VisualStyles.VisualStyleElement.Menu.Separator.Normal);
            var openWorkflowMenuItem = new MenuItem
            {
                Header = "打开工作流(Ctrl+O)",
                Command= DesignerViewCommands.OpenWorkflowCommand
            };
            DesignerView.ContextMenu.Items.Add(openWorkflowMenuItem);

            var surroundedWithTryCatchwMenuItem = new MenuItem
            {
                Header = "用 \"Try Catch\" (Ctrl+T)包围",
                Command = DesignerViewCommands.SurroundedWithTryCatchCommand
            };
            DesignerView.ContextMenu.Items.Add(surroundedWithTryCatchwMenuItem);
        }

        #region CommentOut

        private void CommentOutDelCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (SelectedModelItem == null || SelectedModelItem == SelectedModelItem.Root)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = true;
        }

        private void CommentOutDelExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedModelItem = SelectedModelItem;
            var commentOutActivity = selectedModelItem.GetCurrentValue() as CommentOutActivity;
            if (commentOutActivity == null)
            {
                return;
            }
            var sequence = commentOutActivity.Body as Sequence;
            if (sequence == null)
            {
                using (ModelEditingScope modelEditingScope = selectedModelItem.BeginEdit())
                {
                    MorphHelper.MorphObject(selectedModelItem, ModelFactory.CreateItem(selectedModelItem.GetEditingContext(), commentOutActivity.Body));
                    modelEditingScope.Complete();
                }
            }
            else if (sequence.Activities.Count == 1)
            {
                using (ModelEditingScope modelEditingScope = selectedModelItem.BeginEdit())
                {
                    MorphHelper.MorphObject(selectedModelItem, ModelFactory.CreateItem(selectedModelItem.GetEditingContext(), sequence.Activities[0]));
                    modelEditingScope.Complete();
                }
            }
            else
            {
                try
                {
                    using (ModelEditingScope modelEditingScope = selectedModelItem.BeginEdit())
                    {
                        ModelItemCollection modelItemCollection = null;
                        modelItemCollection = selectedModelItem.Parent.Parent.Properties["Activities"].Collection;
                        int index = modelItemCollection.IndexOf(selectedModelItem);
                        modelItemCollection.RemoveAt(index);
                        foreach (Activity item in sequence.Activities.Reverse())
                        {
                            modelItemCollection.Insert(index, item);
                        }
                        modelEditingScope.Complete();
                    }
                }
                catch
                {
                    MorphHelper.MorphObject(selectedModelItem, ModelFactory.CreateItem(selectedModelItem.GetEditingContext(), sequence));
                }
            }
        }

        private void CommentOutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (SelectedModelItem == null||SelectedModelItem==SelectedModelItem.Root)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = true;
        }

        public void CommentOutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedModelItem = SelectedModelItem;
            //先移除断点
            BreakpointsManager.RemoveBreakpoint();

            using (ModelEditingScope modelEditingScope = selectedModelItem.BeginEdit())
            {
                var commentOutActivity = new CommentOutActivity();
                (commentOutActivity.Body as Sequence).Activities.Add(selectedModelItem.GetCurrentValue() as Activity);
                MorphHelper.MorphObject(selectedModelItem, ModelFactory.CreateItem(selectedModelItem.GetEditingContext(), commentOutActivity));
                modelEditingScope.Complete();
            }
            var clearIdInfoMethod = typeof(Activity).GetMethod("ClearIdInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            clearIdInfoMethod?.Invoke(selectedModelItem.GetCurrentValue(), null);
        }

        #endregion

        #region TryCatch
        private void SurroundedWithTryCatchCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (SelectedModelItem == null || SelectedModelItem == SelectedModelItem.Root)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = true;
        }


        public void SurroundedWithTryCatchExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedModelItem = SelectedModelItem;
            using (ModelEditingScope modelEditingScope = selectedModelItem.BeginEdit())
            {
                var tryCatchActivity = new TryCatch();
                var modelItem = ModelFactory.CreateItem(selectedModelItem.GetEditingContext(), tryCatchActivity);
                MorphHelper.MorphObject(selectedModelItem, modelItem);
                modelItem.Properties["Try"].SetValue(selectedModelItem.GetCurrentValue());
                modelEditingScope.Complete();
            }
        }

        #endregion

        #region OpenWorkflow
        private void OpenWorkflowCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (SelectedModelItem == null)
            {
                e.CanExecute = false;
                return;
            }

            var activity = SelectedModelItem.GetCurrentValue() as InvokeWorkflowFileActivity;
            e.CanExecute = activity != null;
        }

        private void OpenWorkflowExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var property = SelectedModelItem.Properties["WorkflowFilePath"];
            if (property.Value == null || property.Value.GetCurrentValue() == null)
            {
                UniMessageBox.Show(App.Current.MainWindow, "请先选择文件", "打开工作流", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var filePath = ((InArgument<string>)property.Value.GetCurrentValue()).Expression?.ToString();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                UniMessageBox.Show(App.Current.MainWindow, "请先选择文件", "打开工作流", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Librarys.Common.OpenWorkFlow(filePath);
        }
        #endregion

        /// <summary>
        /// 添加活动
        /// </summary>
        /// <param name="activity"></param>
        public void AddItem(object item)
        {
            var workflowDesigner = DocumentContext.WorkflowDesigner;
            var modelService = DocumentContext.Services.GetService<ModelService>();
            var modelTreeManager = DocumentContext.Services.GetService<ModelTreeManager>();

            if (Librarys.Common.IsWorkflowDesignerEmpty(workflowDesigner))
            {
                var activity = item.GetActivity();
                if(activity==null)
                {
                    return;
                }
                var modelItem = modelTreeManager.CreateModelItem(null, activity);
                SetDisplay(modelItem);

                activity = Librarys.Common.ProcessAutoSurroundWithSequence(activity);

                modelService.Root.Content.SetValue(activity);

                return;
            }
            var selectedModelItem = SelectedModelItem;
            ModelItem parentModelItem = null;
            if (selectedModelItem == null|| selectedModelItem == modelService.Root)
            {
                parentModelItem=Librarys.Common.FindFirstActivity(workflowDesigner);
                if (parentModelItem == null|| !parentModelItem.IsContainer())
                {
                    return;
                }
            }
            else if (!selectedModelItem.IsContainer()||!selectedModelItem.CanAddItem(item))
            { 
                parentModelItem = selectedModelItem.GetParent(m => m.IsContainer());
                if(parentModelItem == null)
                {
                    throw new Exception("找不到包含所选活动的序列或流程图或状态机");
                }
            }
            else
            {
                parentModelItem = selectedModelItem;
            }
            var index = parentModelItem.IndexOf(selectedModelItem);
            parentModelItem.AddItem(item, index==-1?-1:index+1);

            var designer = parentModelItem.View as ActivityDesigner;
            //如果父节点没有展开的话，就进入父节点里面
            if(!designer.ShowExpanded)
            {
                if (!designer.Equals(DesignerView.RootDesigner))
                {
                    DesignerView.MakeRootDesigner(parentModelItem);
                }
            }
        }

        /// <summary>
        /// 选中
        /// </summary>
        /// <param name="modelItem"></param>
        public void Select(ModelItem modelItem)
        {
            var modelSearchService = DocumentContext.Services.GetService<ModelSearchService>();
            modelSearchService.AsDynamic().NavigateTo(modelItem);
        }

        /// <summary>
        /// 设置活动显示名(暂时处理方案)
        /// </summary>
        /// <param name="modelItem"></param>
        public void SetDisplay(ModelItem modelItem)
        {
            var assemblyQualifiedName = modelItem.ItemType.Namespace + "." + modelItem.ItemType.Name + "," + modelItem.ItemType.Assembly;
            assemblyQualifiedName = assemblyQualifiedName.Replace(" ", "");

            if (!ViewModelLocator.instance.Activities.ActivityTreeItemAssemblyQualifiedNameDic.ContainsKey(assemblyQualifiedName))
            {
                assemblyQualifiedName = modelItem.ItemType.AssemblyQualifiedName;
                assemblyQualifiedName = assemblyQualifiedName.Replace(" ", "");
            }

            if (ViewModelLocator.instance.Activities.ActivityTreeItemAssemblyQualifiedNameDic.ContainsKey(assemblyQualifiedName) && !assemblyQualifiedName.Contains("System.Activities.Statements.State"))
            {
                var item = ViewModelLocator.instance.Activities.ActivityTreeItemAssemblyQualifiedNameDic[assemblyQualifiedName];
                if (modelItem.Properties["DisplayName"].Value.ToString().Equals(modelItem.ItemType.GetDisplayName()))
                {
                    modelItem.Properties["DisplayName"].SetValue(item.Name);
                }
            }
            //特殊处理foreach、parallelforeach、/*finalstate*/
            else
            {
                if (assemblyQualifiedName.Contains("ForEach"))
                {
                    if (assemblyQualifiedName.Contains("ParallelForEach"))
                    {
                        if (modelItem.Properties["DisplayName"].Value.ToString().Equals("ParallelForEach<Object>"))
                        {
                            modelItem.Properties["DisplayName"].SetValue("并行的遍历循环");
                        }
                    }
                    else
                    {
                        if (modelItem.Properties["DisplayName"].Value.ToString().Equals("ForEach<Object>"))
                        {
                            modelItem.Properties["DisplayName"].SetValue("遍历循环");
                        }
                    }
                }
                else if (assemblyQualifiedName.Contains("State"))
                {
                    if (modelItem.Properties["DisplayName"].Value.ToString().Equals("State1"))
                    {
                        modelItem.Properties["DisplayName"].SetValue("状态");
                    }
                    else if (modelItem.Properties["DisplayName"].Value.ToString().Equals("FinalState"))
                    {
                        modelItem.Properties["DisplayName"].SetValue("最终状态");
                    }
                }
            }
        }
    }
}
