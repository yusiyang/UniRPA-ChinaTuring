using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniStudio.Search.Models;
using UniStudio.Search.Operations;

namespace UniStudio.Search.Services
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
