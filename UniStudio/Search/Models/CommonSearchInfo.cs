using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniStudio.Search.Enums;

namespace UniStudio.Search.Models
{
    public class CommonSearchInfo
    {
        public CommonSearchType CommonSearchType { get; set; }

        public string Display { get; set; }

        public int Count { get; set; }

        public int Index { get; set; }
    }
}
