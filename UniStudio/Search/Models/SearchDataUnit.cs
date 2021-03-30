using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniStudio.Search.Enums;
using UniStudio.Search.Models.SearchLocations;

namespace UniStudio.Search.Models
{
    public class SearchDataUnit
    {
        public CommonDataType CommonDataType { get; set; }

        public string SearchText { get; set; }

        public string DisplayText { get; set; }

        public string Icon { get; set; }

        public string Path { get; set; }

        public string RelativeFilePath { get; set; }

        public bool IsCurrentFile { get; set; }

        public SearchLocation SearchLocation { get; set; }
    }
}
