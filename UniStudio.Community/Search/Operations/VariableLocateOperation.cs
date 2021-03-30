using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using Plugins.Shared.Library.Extensions;
using ReflectionMagic;
using UniStudio.Community.Librarys;
using UniStudio.Community.Search.Models;
using UniStudio.Community.Search.Models.SearchLocations;

namespace UniStudio.Community.Search.Operations
{
    public class VariableLocateOperation : ILocateOperation
    {
        public void LocateTo(SearchDataUnit searchDataUnit)
        {
            var location = (VariableSearchLocation)searchDataUnit.SearchLocation;
            Common.OpenWorkFlow(location.FilePath);
            var modelService = DocumentContext.Current.Services.GetService<ModelService>();

            ModelItem modelItem = null;
            if (!string.IsNullOrWhiteSpace(location.IdRef))
            {
                modelItem = modelService.FindVariableByIdRef(location.IdRef, location.VariableName);
            }
            if (modelItem == null && !string.IsNullOrWhiteSpace(location.ActivityId))
            {
                modelItem = modelService.FindVariable(location.ActivityId, location.VariableName);
            }
            
            var modelSearchService = DocumentContext.Current.Services.GetService<ModelSearchService>();
            modelSearchService.AsDynamic().HighlightModelItem(modelItem.Parent.Parent);

            var designerView = DocumentContext.Current.Services.GetService<DesignerView>();
            designerView.AsDynamic().CheckButtonVariables();
            designerView.AsDynamic().variables1.SelectVariable(modelItem);
        }
    }
}
