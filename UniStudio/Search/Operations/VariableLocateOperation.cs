using Plugins.Shared.Library.Extensions;
using ReflectionMagic;
using System;
using System.Activities.Presentation.Model;
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
