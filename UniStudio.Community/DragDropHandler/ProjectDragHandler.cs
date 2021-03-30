using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using UniStudio.Community.Librarys;
using UniStudio.Community.ViewModel;
using WorkflowUtils;

namespace UniStudio.Community.DragDropHandler
{
    public class ProjectDragHandler: DefaultDragHandler
    {
        public override void StartDrag(IDragInfo dragInfo)
        {
            var item = dragInfo.SourceItem as ProjectTreeItem;
            if(item.IsXaml && ViewModelLocator.instance.Dock.ActiveDocument != null)
            {
                var designer = ViewModelLocator.instance.Dock.ActiveDocument.WorkflowDesignerInstance;

                var dragActivity = new InvokeWorkflowFileActivity();
                dragActivity.SetWorkflowFilePath(item.Path);

                Activity resultActivity = dragActivity;
                if(Common.IsWorkflowDesignerEmpty(designer))
                {
                    resultActivity = Common.ProcessAutoSurroundWithSequence(dragActivity);
                }
               
                if (resultActivity != null)
                {
                    ModelItem mi = designer.Context.Services.GetService<ModelTreeManager>().CreateModelItem(null, resultActivity);
                    DataObject data = new DataObject();
                    data.SetData(DragDropHelper.ModelItemDataFormat, mi);
                    dragInfo.DataObject = data;
                }
            }

            base.StartDrag(dragInfo);
        }


  


    }

   


}