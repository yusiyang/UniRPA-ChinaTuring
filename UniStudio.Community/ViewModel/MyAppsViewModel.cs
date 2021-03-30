using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NPOI.OpenXmlFormats.Dml;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UniStudio.Community.Models;

namespace UniStudio.Community.ViewModel
{
    public class MyAppsViewModel : ViewModelBase
    {
        public ObservableCollection<AppInfo> AppInfos { get; set; }

        private Visibility _visibility = Visibility.Collapsed;
        public Visibility Visibility { get { return _visibility; } set { Set(ref _visibility, value); } }

        /// <summary>
        /// 安装最新版本
        /// </summary>
        public ICommand InstallCommand { get; private set; }


        /// <summary>
        /// 安装历史版本
        /// </summary>
        public ICommand InstallHisCommand { get; private set; }


        private string _searchText;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                Set(ref _searchText, value);
                HightLightText = value?.Trim();
                DoSearch();
            }
        }

        private string _hightLightText;
        public string HightLightText { get { return _hightLightText; } set { Set(ref _hightLightText, value); } }

        public MyAppsViewModel()
        {
            InstallCommand = new RelayCommand<int>((id) =>
            {
                ChangeVersion(id);
            });

            InstallHisCommand = new RelayCommand<VersionCommand>((obj) =>
            {
                ChangeVersion(obj);
            });

            AppInfos = new ObservableCollection<AppInfo>() {
                new AppInfo(){
                    ID =1 ,
                    Name ="微信批量发送",
                    Des ="向客户发送短信/微信消息通知",
                    Pic="https://qlogo4.store.qq.com/qzone/390811879/390811879/100?0",
                    Versions = new ObservableCollection<Version>(){
                        new Version("0.1"),
                        new Version("0.5"),
                        new Version("1.0"),
                    },
                },
                new AppInfo(){
                    ID =2 ,
                    Name ="电子发票",
                    Des ="买家自助开票，方便高效",
                    Pic="https://qlogo4.store.qq.com/qzone/390811879/390811879/100?0",
                    Versions = new ObservableCollection<Version>(){
                        new Version("0.1"),
                        new Version("0.5"),
                        new Version("1.0"),
                    },
                    CurrentVersion = new Version("0.5"),
                },
                new AppInfo(){
                    ID =3 ,
                    Name ="验证工具",
                    Des ="核销优惠券/码、电子卡券等",
                    Pic="https://qlogo4.store.qq.com/qzone/390811879/390811879/100?0",
                    Versions = new ObservableCollection<Version>(){
                        new Version("0.1"),
                        new Version("0.5"),
                        new Version("1.0"),
                    },
                    CurrentVersion = new Version("1.0"),
                },
                new AppInfo(){
                    ID =4 ,
                    Name ="验证工具",
                    Des ="核销优惠券/码、电子卡券等",
                    Pic="https://qlogo4.store.qq.com/qzone/390811879/390811879/100?0",
                    Versions = new ObservableCollection<Version>(){
                        new Version("1.0"),
                    },
                    CurrentVersion = new Version("1.0"),
                },
            };
        }

        void ChangeVersion(int id)
        {
            ChangeVersion(new VersionCommand()
            {
                AppID = id,
            });
        }

        void ChangeVersion(VersionCommand command)
        {
            var info = AppInfos.FirstOrDefault(x => x.ID == command.AppID);
            if (info != null)
            {
                #region 安装流程
                #endregion
                info.CurrentVersion = command.TargetVersion ?? info.Versions.Max();
            }
        }

        void DoSearch()
        {
            AppInfos.ForEach(i =>
            {
                i.Visibility = i.Name.Contains(HightLightText) ? Visibility.Visible : Visibility.Collapsed;
            });
            Visibility = AppInfos.Any(x => x.Name.Contains(HightLightText)) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class AppInfo : ViewModelBase
    {
        void RefreshVersionStatus()
        {
            if (CurrentVersion == null || Versions == null || Versions.Count == 0)
            {
                AppVersionStatus = AppVersionEnum.NotInstall;
            }
            else if (CurrentVersion < Versions.Max())
            {
                AppVersionStatus = AppVersionEnum.OldVersion;
            }
            else
            {
                AppVersionStatus = AppVersionEnum.NewestVersion;
            }
        }

        public int ID { get; set; }

        public string Pic { get; set; }

        public string Name { get; set; }

        public string Des { get; set; }

        public ObservableCollection<Version> Versions { get; set; }

        private Version _currentVersion;
        public Version CurrentVersion { get { return _currentVersion; } set { Set(ref _currentVersion, value); RefreshVersionStatus(); } }

        private AppVersionEnum _appVersionStatus;
        public AppVersionEnum AppVersionStatus { get { return _appVersionStatus; } private set { Set(ref _appVersionStatus, value); } }

        private Visibility _visibility = Visibility.Visible;
        public Visibility Visibility { get { return _visibility; } set { Set(ref _visibility, value); } }
    }

    public enum AppVersionEnum
    {
        NotInstall,
        OldVersion,
        NewestVersion,
    }
}
