using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using Plugins.Shared.Library.Extensions;
using ReflectionMagic;
using UniStudio.Community.Librarys;
using UniStudio.Community.Search.Models;
using UniStudio.Community.Search.Models.SearchLocations;

namespace UniStudio.Community.Search.Operations
{
    public class ImportLocateOperation : ILocateOperation
    {
        public void LocateTo(SearchDataUnit searchDataUnit)
        {
            var location = (ImportSearchLocation)searchDataUnit.SearchLocation;
            Common.OpenWorkFlow(location.FilePath);
            var modelService = DocumentContext.Current.Services.GetService<ModelService>();
            var modelItem = modelService.FindImport(location.ImportName);

            var designerView = DocumentContext.Current.Services.GetService<DesignerView>();
            designerView.AsDynamic().buttonImports1.IsChecked = true;
            var importDesigner = designerView.AsDynamic().imports1;
            var dataGrid = importDesigner.importedNamespacesDataGrid;
            dataGrid.SelectedItem = modelItem;
            dataGrid.ScrollIntoView(modelItem, null);
        }
    }
}
