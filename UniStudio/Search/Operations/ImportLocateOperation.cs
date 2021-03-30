using Plugins.Shared.Library.Extensions;
using ReflectionMagic;
using System;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniStudio.Librarys;
using UniStudio.Search.Models;
using UniStudio.Search.Models.SearchLocations;
using UniStudio.WorkflowOperation;

namespace UniStudio.Search.Operations
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
