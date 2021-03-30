using System.Windows;
using System.Windows.Controls;
using UniStudio.Community.ViewModel;
using Xceed.Wpf.AvalonDock.Layout;

namespace UniStudio.Community.Selectors
{

  class DocumentTemplateSelector : DataTemplateSelector
    {
        public DocumentTemplateSelector()
        {
        
        }


        public DataTemplate DocumentViewTemplate
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is DocumentViewModel)
                return DocumentViewTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
