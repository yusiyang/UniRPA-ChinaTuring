using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniStudio.Search.Models;

namespace UniStudio.Search.Operations
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
