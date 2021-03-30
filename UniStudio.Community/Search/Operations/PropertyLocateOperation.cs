using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using Plugins.Shared.Library.Extensions;
using ReflectionMagic;
using UniStudio.Community.Librarys;
using UniStudio.Community.Search.Models;
using UniStudio.Community.Search.Models.SearchLocations;

namespace UniStudio.Community.Search.Operations
{
    public class PropertyLocateOperation : ILocateOperation
    {
        public void LocateTo(SearchDataUnit searchDataUnit)
        {
            var location = (PropertySearchLocation)searchDataUnit.SearchLocation;
            Common.OpenWorkFlow(location.FilePath);
            var modelService = DocumentContext.Current.Services.GetService<ModelService>();

            ModelProperty modelProperty = null;
            if (!string.IsNullOrWhiteSpace(location.IdRef))
            {
                modelProperty = modelService.FindPropertyByIdRef(location.IdRef,location.PropertyName);
            }
            if (modelProperty == null && !string.IsNullOrWhiteSpace(location.ActivityId))
            {
                modelProperty = modelService.FindProperty(location.ActivityId, location.PropertyName);
            }

            var modelSearchService = DocumentContext.Current.Services.GetService<ModelSearchService>();
            modelSearchService.AsDynamic().HighlightModelItem(modelProperty.Parent);

            var propertyInspector = DocumentContext.Current.WorkflowDesigner.PropertyInspectorView;
            propertyInspector.AsDynamic().SelectPropertyByPath(modelProperty.Name);
        }
    }
}
