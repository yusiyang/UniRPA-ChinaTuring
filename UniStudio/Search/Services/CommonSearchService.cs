using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniStudio.Search.Enums;
using UniStudio.Search.Models;

namespace UniStudio.Search.Services
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
