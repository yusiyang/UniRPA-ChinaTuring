using Plugins.Shared.Library.Librarys;
using System.IO;
using UniWorkforce.Librarys;
using UniWorkforce.ViewModel;

namespace UniWorkforce.Services
{
    public class WorkforceController
    {
        /// <summary>
        /// 发布流程
        /// </summary>
        /// <param name="packageModel"></param>
        public void PublishProcess(PackageModel packageModel)
        {
            var filePath = Path.Combine(Settings.Instance.PackagesDir, $"{packageModel.PackageName}.nupkg");
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                fileStream.Write(packageModel.File.ToByteArray(), 0, packageModel.File.Length);
            }
            DirectoryOperation.DeleteDir(Settings.Instance.InstalledPackagesDir);

            ViewModelLocator.instance.Main.RefreshCommand.Execute(null);
            var packageItems = ViewModelLocator.instance.Main.PackageItems;
            if (packageItems?.Count > 0)
            {
                var packageCount = packageItems.Count;
                for (var i = 0; i < packageCount; i++)
                {
                    packageItems[i].UpdateCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// 根据名称获取PackageItem
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static PackageItem GetPackageItemByName(string name)
        {
            var packageItems = ViewModelLocator.instance.Main.PackageItems;
            foreach (var item in packageItems)
            {
                if (item.Name == name)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 运行流程
        /// </summary>
        /// <param name="packageName"></param>
        public void RunProcess(string packageName)
        {
            var packageItem = GetPackageItemByName(packageName);
            if (packageItem != null)
            {

                packageItem.StartCommand.Execute(null);
            }
        }

        /// <summary>
        /// 停止运行
        /// </summary>
        public void StopProcess()
        {
            ViewModelLocator.instance.Main.StopCommand.Execute(null);
        }
    }
}
