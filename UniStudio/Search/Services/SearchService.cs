using Plugins.Shared.Library;
using Plugins.Shared.Library.Librarys;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniStudio.Search.Enums;
using UniStudio.Search.Models;
using UniStudio.Search.Utils;
using UniStudio.ViewModel;

namespace UniStudio.Search.Services
{
    public class SearchService
    {
        public SearchService()
        {
        }

        public virtual List<SearchDataUnit> DoSearch(SearchParams searchParams, out Dictionary<CommonSearchType, int> countResult)
        {
            IEnumerable<SearchDataUnit> result= Context.Current.SearchDataManager.SearchData??new List<SearchDataUnit>();

            if (!string.IsNullOrWhiteSpace(searchParams.SearchText))
            {
                var searchText = searchParams.SearchText.ToLower().Trim();
                result = result.Where(d => d.SearchText.ToLower().Contains(searchText));
            }

            countResult = null;
            if (searchParams.SearchType == SearchType.Common)
            {
                var enumInfos = CommonSearchType.CurrentFile.GetEnumInfos();
                countResult = enumInfos.ToDictionary(d => d.Key, d => 0);
                foreach (var item in result)
                {
                    var commonSearchTypeList = item.GetCommonSearchTypes();
                    foreach (var commonSearchType in commonSearchTypeList)
                    {
                        countResult[commonSearchType] += 1;
                    }
                }

                if(countResult[searchParams.CommonSearchType]==0)
                {
                    if(countResult.All(d => d.Value == 0))
                    {
                        return new List<SearchDataUnit>();
                    }
                    searchParams.CommonSearchType = countResult.First(d => d.Value > 0).Key;
                    searchParams.GenerateDataType();
                }
            }

            result = result.Where(d => searchParams.CommonDataType.HasFlag(d.CommonDataType));
            if (searchParams.SearchType == SearchType.Common&&searchParams.CommonSearchType == CommonSearchType.CurrentFile)
            {
                result = result.Where(d => d.RelativeFilePath==DocumentContext.Current.RelativeFilePath);
            }
            var searchResultData = result.ToList();
            return searchResultData;
        }
    }
}
