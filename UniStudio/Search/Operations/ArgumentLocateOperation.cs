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
    public class ArgumentLocateOperation : ILocateOperation
    {
        public void LocateTo(SearchDataUnit searchDataUnit)
        {
            var location = (ArgumentSearchLocation)searchDataUnit.SearchLocation;
            Common.OpenWorkFlow(location.FilePath);
            var modelService = DocumentContext.Current.Services.GetService<ModelService>();
            var modelItem = modelService.FindArgument(location.Name);

            var  modelSearchService = DocumentContext.Current.Services.GetService<ModelSearchService>();
            modelSearchService.AsDynamic().HighlightModelItem(modelService.Root);

            var  designerView = DocumentContext.Current.Services.GetService<DesignerView>();
            designerView.AsDynamic().CheckButtonArguments();
            designerView.AsDynamic().arguments1.SelectArgument(modelItem);
        }
    }
}
