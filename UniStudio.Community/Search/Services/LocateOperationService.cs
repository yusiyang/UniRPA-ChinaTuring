using UniStudio.Community.Search.Models;
using UniStudio.Community.Search.Operations;

namespace UniStudio.Community.Search.Services
{
    public class LocateOperationService
    {
        public void LocateTo(SearchDataUnit searchDataUnit)
        {
            var locateOperation = LocateOperationFactory.Create(searchDataUnit.CommonDataType);
            locateOperation.LocateTo(searchDataUnit);
            Context.Current.SearchViewManager.Current.IsOpen = false;
        }
    }
}
