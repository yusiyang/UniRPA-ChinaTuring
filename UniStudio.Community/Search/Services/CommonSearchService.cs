using System.Collections.Generic;
using UniStudio.Community.Search.Enums;
using UniStudio.Community.Search.Models;

namespace UniStudio.Community.Search.Services
{
    public class CommonSearchService:SearchService
    {
        public override List<SearchDataUnit> DoSearch(SearchParams searchParams, out Dictionary<CommonSearchType, int> countResult)
        {
            countResult = null;
            return null;
        }
    }
}
