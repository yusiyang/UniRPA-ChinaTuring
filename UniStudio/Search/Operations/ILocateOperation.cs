using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniStudio.Search.Models;

namespace UniStudio.Search.Operations
{
    public interface ILocateOperation
    {
        void LocateTo(SearchDataUnit searchDataUnit);
    }
}
