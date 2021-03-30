using UniStudio.Community.Search.Enums;

namespace UniStudio.Community.Search.Models
{
    public class CommonSearchInfo
    {
        public CommonSearchType CommonSearchType { get; set; }

        public string Display { get; set; }

        public int Count { get; set; }

        public int Index { get; set; }
    }
}
