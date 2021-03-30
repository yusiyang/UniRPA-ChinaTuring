using System.Activities.Presentation.PropertyEditing;
using System.Windows;

namespace FileActivity
{
    public class ChangeTypeEditor : PropertyValueEditor
    {
        public ChangeTypeEditor()
        {
            this.InlineEditorTemplate = PropertyEditorResources.GetResources()["ChangeTypeEditor"] as DataTemplate;
        }
    }
}
