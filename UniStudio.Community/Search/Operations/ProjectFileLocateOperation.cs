using System.Diagnostics;
using System.IO;
using Plugins.Shared.Library;
using UniStudio.Community.Librarys;
using UniStudio.Community.Search.Models;
using UniStudio.Community.Search.Models.SearchLocations;

namespace UniStudio.Community.Search.Operations
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
