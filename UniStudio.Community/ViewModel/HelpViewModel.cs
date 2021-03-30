using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using UniStudio.Community.Librarys;

namespace UniStudio.Community.ViewModel
{
    public class HelpViewModel : ViewModelBase
    {
        public UserControl m_view { get; set; }

        private RelayCommand<RoutedEventArgs> _loadedCommand;

        /// <summary>
        /// Gets the LoadedCommand.
        /// </summary>
        public RelayCommand<RoutedEventArgs> LoadedCommand
        {
            get
            {
                return _loadedCommand
                    ?? (_loadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        m_view = (UserControl)p.Source;
                    }));
            }
        }


        #region 主题色配置
        public const string ItemTitleForegroundPropertyName = "ItemTitleForeground";
        private SolidColorBrush _itemTitleForeground = new BrushConverter().ConvertFrom("#383838") as SolidColorBrush;
        public SolidColorBrush ItemTitleForeground
        {
            get
            {
                return _itemTitleForeground;
            }
            set
            {
                if (_itemTitleForeground == value)
                {
                    return;
                }

                _itemTitleForeground = value;
                RaisePropertyChanged(ItemTitleForegroundPropertyName);
            }
        }

        public const string ItemDescriptionForegroundPropertyName = "ItemDescriptionForeground";
        private SolidColorBrush _itemDescriptionForeground = new BrushConverter().ConvertFrom("#606060") as SolidColorBrush;
        public SolidColorBrush ItemDescriptionForeground
        {
            get
            {
                return _itemDescriptionForeground;
            }
            set
            {
                if (_itemDescriptionForeground == value)
                {
                    return;
                }

                _itemDescriptionForeground = value;
                RaisePropertyChanged(ItemDescriptionForegroundPropertyName);
            }
        }

        public const string ItemMouseOverBackgroundPropertyName = "ItemMouseOverBackground";
        private SolidColorBrush _itemMouseOverBackground = new BrushConverter().ConvertFrom("#d6d6d6") as SolidColorBrush;
        public SolidColorBrush ItemMouseOverBackground
        {
            get
            {
                return _itemMouseOverBackground;
            }
            set
            {
                if (_itemMouseOverBackground == value)
                {
                    return;
                }

                _itemMouseOverBackground = value;
                RaisePropertyChanged(ItemMouseOverBackgroundPropertyName);
            }
        }
        #endregion


        private RelayCommand _productDocCommand;
        public RelayCommand ProductDocCommand
        {
            get
            {
                return _productDocCommand
                    ?? (_productDocCommand = new RelayCommand(
                    () =>
                    {
                        Process.Start("https://docs.unirpa.com/");
                    }));
            }
        }

        private RelayCommand _helpCenterCommand;
        public RelayCommand HelpCenterCommand
        {
            get
            {
                return _helpCenterCommand
                    ?? (_helpCenterCommand = new RelayCommand(
                    () =>
                    {
                        Process.Start("https://www.unirpa.com/#help-center");
                    }));
            }
        }

        private RelayCommand _aboutUsCommand;
        public RelayCommand AboutUsCommand
        {
            get
            {
                return _aboutUsCommand
                    ?? (_aboutUsCommand = new RelayCommand(
                    () =>
                    {
                        Process.Start("https://www.unirpa.com/#about-us");
                    }));
            }
        }

        public const string StudioVersionPropertyName = "StudioVersion";
        private string _studioVersion;
        public string StudioVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_studioVersion))
                {
                    _studioVersion = "Pegasus_v" + Common.GetProgramVersion();
                }

                return _studioVersion;
            }

            set
            {
                if (_studioVersion == value)
                {
                    return;
                }

                _studioVersion = value;
                RaisePropertyChanged(StudioVersionPropertyName);
            }
        }

    }
}
