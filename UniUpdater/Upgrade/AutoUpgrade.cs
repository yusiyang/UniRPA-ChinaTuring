using log4net;
using Plugins.Shared.Library.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UniUpdater.Config;
using UniUpdater.Libraries;

namespace UniUpdater.Upgrade
{
    public class AutoUpgrade
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private UpgradeInfo _upgradeInfo;

        public bool IsEnd { get; private set; }

        public bool IsNeedUpgrade()
        {
            try
            {
                //File.Move(@"C:\Users\Administrator\AppData\Local\UniStudio\Update\e0590d4719b2d7080cc3d2b2652ee225.zip", @"C:\Users\Administrator\AppData\Local\UniStudio\Update\aaa.zip");

                Version currentVersion = App.Instance.StudioVersion;
                Version latestVersion = ServerUpgradeConfig.Instance.Version;

                return latestVersion > currentVersion;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public void DoUpgrade()
        {
            //检查是否要更新
            Task.Run(() =>
            {
                try
                {
                    if (IsNeedUpgrade())
                    {
                        //bool hasDownloaded = false;
                        var originFileName = Common.GetFileNameFromUrl(ServerUpgradeConfig.Instance.UpgradeUrl);
                        var path = Path.Combine(App.Instance.UpdateDir, originFileName);
                        //if (File.Exists(path) && Common.GetMD5HashFromFile(path).ToLower() == ServerUpgradeConfig.Instance.Md5.ToLower())
                        //{
                        //    hasDownloaded = true;
                        //}
                        Download();
                        //if (!hasDownloaded)
                        //{
                        //    Download();
                        //}
                        return;
                    }
                }
                catch(Exception ex)
                {

                }
            });
        }

        private void Download()
        {
            //执行升级，后台下载(断点下载)，下载完成后判断MD5是否合法，合法的话弹窗提示用户安装
            var path = Path.Combine(App.Instance.UpdateDir, $"{ServerUpgradeConfig.Instance.Md5}.zip");

            var downloader = new HttpDownloadFile();
            downloader.OnRunningChanged = OnRunningChanged;
            downloader.OnDownloadFinished = OnDownloadFinished;
            downloader.OnDownloading = OnDownloading;
            downloader.Download(ServerUpgradeConfig.Instance.UpgradeUrl, path, ServerUpgradeConfig.Instance.Md5);
        }

        private void OnDownloadFinished(HttpDownloadFile obj)
        {
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    _upgradeInfo?.Close();
            //});

            if (obj.IsDownloadSuccess)
            {
                //var fileMd5 = Common.GetMD5HashFromFile(obj.SaveFilePath);
                //if (ServerUpgradeConfig.Instance.Md5.ToLower() == fileMd5.ToLower())
                //{
                var saveDir = Path.GetDirectoryName(obj.SaveFilePath);

                var originFileName = Common.GetFileNameFromUrl(obj.Url);

                var finishedFilePath = saveDir + @"\" + originFileName;
                if (File.Exists(obj.SaveFilePath))
                {
                    //文件MD5校验一致，说明包下载成功且未被破坏，提示用户安装


                    if (File.Exists(finishedFilePath))
                    {
                        File.Delete(finishedFilePath);
                    }

                    File.Move(obj.SaveFilePath, finishedFilePath);

                    if (!File.Exists(finishedFilePath))
                    {
                        //重命名失败
                        Logger.Error(string.Format("重命名{0}到{1}失败", obj.SaveFilePath, finishedFilePath), logger);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            UniMessageBox.Show("升级包重命名操作出现异常，请检查！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                        });
                    }
                    else
                    {
                        InstallUpgradePackage(finishedFilePath);
                    }
                }
                else
                {
                    InstallUpgradePackage(finishedFilePath);
                }
                    
                //}
                //else
                //{
                //    //安装包下载出现问题（可能安装包之前残留的断点数据不对，删除这个文件），提示用户重试
                //    if (File.Exists(obj.SaveFilePath))
                //    {
                //        File.Delete(obj.SaveFilePath);
                //    }
                //    Application.Current.Dispatcher.Invoke(() =>
                //    {
                //        UniMessageBox.Show("升级包MD5校验不一致，请重试！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    });
                //}
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UniMessageBox.Show("下载过程中出现异常，请检查并重试！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            }
        }

        private void InstallUpgradePackage(string upgradePackageFilePath)
        {
            //MessageBox.Show("werewr");
            SetProgress("正在解压缩", 70);
            ZipHelper.UnZip(upgradePackageFilePath, null,(file, progress) =>
              {
                  var value = (int)(progress * 20);
                  SetProgress($"正在解压缩:{file}", 70 + (value>20?20:value));
              });

            #region 更新文件
            var updateDir = Path.Combine(Path.GetDirectoryName(upgradePackageFilePath), Path.GetFileNameWithoutExtension(upgradePackageFilePath));

            SetProgress("正在拷贝", 90);
            Common.Copy(updateDir, AppDomain.CurrentDomain.BaseDirectory);
            SetProgress("完成", 100);
            #endregion

            Application.Current.Dispatcher.Invoke(() =>
            {
                //UpgradeConfig.Instance.UpdateLogs = ServerUpgradeConfig.Instance.UpdateLogs;
                App.Current.Exit += (sender, e) =>
                {
                    Common.ShellExecute(App.Instance.StudioPath);
                };
                App.Current.Shutdown();
            });
        }

        private void OnDownloading(HttpDownloadFile obj)
        {
            SetProgress("正在下载", (int)(70.0 * obj.FileDownloadedBytes / obj.FileTotalBytes));
        }

        private void OnRunningChanged(HttpDownloadFile obj)
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                if (obj.IsRunning)
                {
                    _upgradeInfo = new UpgradeInfo();
                    _upgradeInfo.Show();
                }
            });
        }

        private void SetProgress(string text,int value)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_upgradeInfo != null)
                {
                    _upgradeInfo.Description = text;
                    _upgradeInfo.ProgressValue = value;
                }
            });
        }
    }
}
