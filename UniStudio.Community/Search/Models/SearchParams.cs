using UniStudio.Community.Search.Enums;
using UniStudio.Community.Search.Utils;

namespace UniStudio.Community.Search.Models
{
    public class SearchParams
    {
        public SearchType SearchType { get; set; }

        public CommonSearchType CommonSearchType { get; set; }

        public CommonDataType CommonDataType { get; set; }

        public string SearchText { get; set; }
        
        public SearchParams()
        { }

        public SearchParams(SearchType searchType,CommonSearchType commonSearchType,string searchText)
        {
            SearchType = searchType;
            CommonSearchType = commonSearchType;
            SearchText = searchText;
            GenerateDataType();
        }

        public void GenerateDataType()
        {
            switch(SearchType)
            {
                case SearchType.AddActivity:
                    CommonDataType = CommonDataType.Activity;
                    break;
                case SearchType.Common:
                    CommonDataType = CommonSearchType.GetCommonDataType();
                    break;
                case SearchType.GoToFile:
                    CommonDataType = CommonDataType.ProjectFile;
                    break;
                case SearchType.LocateActivity:
                    CommonDataType = CommonDataType.DesignerActivity;
                    break;
            }
        }
    }
}
