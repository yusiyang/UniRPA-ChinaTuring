using Plugins.Shared.Library.Extensions;
using System;
using System.Linq;
using System.Windows;
using UniStudio.Community.Search.Models;
using UniStudio.Community.Search.Models.SearchLocations;
using UniStudio.Community.ViewModel;
using UniStudio.Community.WorkflowOperation;

namespace UniStudio.Community.Search.Operations
{
    public class ActivityLocateOperation : ILocateOperation
    {
        public void LocateTo(SearchDataUnit searchDataUnit)
        {
            var location = (ActivitySearchLocation)searchDataUnit.SearchLocation;
            var availableActivityItems = ViewModelLocator.instance.Activities.ActivityTreeItemTypeOfDic.Values;
            var activityTreeItem = availableActivityItems.FirstOrDefault(i => i.IsActivity && i.Name == location.ActivityName);
            if(activityTreeItem==null)
            {
                return;
            }

            var type = Type.GetType(activityTreeItem.AssemblyQualifiedName);
            if (type == null)
            {
                // TODO:【🐵🐵 LPY 🐵🐵】暂时妥协的方式，不够优雅，待有空时优化
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = a.GetType(activityTreeItem.TypeOf.Split(',')[0]);
                    if (type != null)
                        break;
                }
            }

            try
            {
                var item = Activator.CreateInstance(type);

                var designerViewWrapper = DocumentContext.Current.Services.GetService<DesignerViewWrapper>();
                designerViewWrapper.AddItem(item);
            }
            catch (Exception ex)
            {
                UniMessageBox.Show(App.Current.MainWindow, "添加活动出错", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
