using Plugins.Shared.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniStudio.Librarys;
using UniStudio.Search.Models;
using UniStudio.Search.Models.SearchLocations;

namespace UniStudio.Search.Operations
{
    public class ProjectFileLocateOperation : ILocateOperation
    {
        public void LocateTo(SearchDataUnit searchDataUnit)
        {
            var location = (FilePathSearchLocation)searchDataUnit.SearchLocation;
            
            var fileExtention = Path.GetExtension(location.FilePath);
            if(fileExtention.ToLower()=="xaml")
            {
                Common.OpenWorkFlow(location.FilePath);
                return;
            }

            var filePath = location.FilePath;
            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(SharedObject.Instance.ProjectPath, filePath);
            }
            Process.Start(filePath);
        }
    }
}
