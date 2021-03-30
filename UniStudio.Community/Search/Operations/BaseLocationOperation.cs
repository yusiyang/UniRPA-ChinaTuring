using System;
using UniStudio.Community.Search.Models;

namespace UniStudio.Community.Search.Operations
{
    public class BaseLocationOperation : ILocateOperation
    {
        public BaseLocationOperation()
        {
        }

        public void LocateTo(SearchDataUnit searchDataUnit)
        {
            throw new NotImplementedException();
        }
    }
}
