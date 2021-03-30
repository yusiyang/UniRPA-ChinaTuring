using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniStudio.Search.Models.SearchLocations
{
    public class DesignerActivitySearchLocation:FilePathSearchLocation
    {
        public string ActivityId { get; set; }

        public string IdRef { get; set; }
    }
}
