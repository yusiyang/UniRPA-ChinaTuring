using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using Plugins.Shared.Library.Extensions;
using ReflectionMagic;
using UniStudio.Community.Librarys;
using UniStudio.Community.Search.Models;
using UniStudio.Community.Search.Models.SearchLocations;

namespace UniStudio.Community.Search.Operations
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
