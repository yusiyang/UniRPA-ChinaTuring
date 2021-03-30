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
    public class DesignerActivityLocateOperation : ILocateOperation
    {
        public void LocateTo(SearchDataUnit searchDataUnit)
        {
            var location = (DesignerActivitySearchLocation)searchDataUnit.SearchLocation;
            Common.OpenWorkFlow(location.FilePath);
            var modelService = DocumentContext.Current.Services.GetService<ModelService>();

            ModelItem modelItem=null;
            if (!string.IsNullOrWhiteSpace(location.IdRef))
            {
                modelItem = modelService.FindActivityByIdRef(location.IdRef);
            }
            if (modelItem == null&& !string.IsNullOrWhiteSpace(location.ActivityId))
            {
                modelItem = modelService.FindActivityById(location.ActivityId);
            }

            //var designerViewWrapper = DocumentContext.Current.Services.GetService<DesignerViewWrapper>();
            //designerViewWrapper.Select(modelItem);

            var modelSearchService = DocumentContext.Current.Services.GetService<ModelSearchService>();
            modelSearchService.AsDynamic().HighlightModelItem(modelItem);
            Selection.SelectOnly(DocumentContext.Current.WorkflowContext, modelItem);
        }
    }
}
