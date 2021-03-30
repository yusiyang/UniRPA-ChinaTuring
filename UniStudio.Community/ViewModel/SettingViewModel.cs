using System.Windows;
using System.Windows.Media;
using System.Xml;
using ActiproSoftware.Windows.Themes;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using UniStudio.Community.UserControls;

namespace UniStudio.Community.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        public SettingContent m_view { get; set; }

        /// <summary>
        /// 标志是否是由代码触发的主题更改
        /// </summary>
        public bool IsCodeTriggerThemeChange { get; set; }

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
                        m_view = (SettingContent)p.Source;

                        // 初始化主题选项
                        XmlDocument doc = new XmlDocument();
                        var path = ViewModelLocator.instance.Main.GlobalSettingsXmlPath;
                        doc.Load(path);
                        var rootNode = doc.DocumentElement;
                        XmlElement themeNode = rootNode.SelectNodes("Theme").Item(0) as XmlElement;
                        string themeName = themeNode.GetAttribute("Name");
                        IsCodeTriggerThemeChange = true;  // 设置标志状态
                        m_view._themeComboBox.Text = themeName;
                        IsCodeTriggerThemeChange = false;  // 恢复标志状态
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


        public const string CurrentSettingOptionsTitlePropertyName = "CurrentSettingOptionsTitle";
        private string _currentSettingOptionsTitle = "常规";
        public string CurrentSettingOptionsTitle
        {
            get
            {
                return _currentSettingOptionsTitle;
            }
            set
            {
                _currentSettingOptionsTitle = value;
                RaisePropertyChanged(CurrentSettingOptionsTitlePropertyName);
            }
        }

        private RelayCommand<string> _settingOptionsCommand;
        public RelayCommand<string> SettingOptionsCommand
        {
            get
            {
                return _settingOptionsCommand
                    ?? (_settingOptionsCommand = new RelayCommand<string>(
                    param =>
                    {
                        switch (param)
                        {
                            case "GeneralSettings":
                                CollapsedAllSettingOptionsContent();
                                m_view._generalSettings.Visibility = Visibility.Visible;
                                CurrentSettingOptionsTitle = "常规";
                                break;

                            case "MoreSettings":
                                CollapsedAllSettingOptionsContent();
                                m_view._moreSettings.Visibility = Visibility.Visible;
                                CurrentSettingOptionsTitle = "更多";
                                break;
                        }
                    }));
            }
        }

        /// <summary>
        /// 隐藏所有设置选项的具体内容页面
        /// </summary>
        private void CollapsedAllSettingOptionsContent()
        {
            m_view._generalSettings.Visibility = Visibility.Collapsed;
            m_view._moreSettings.Visibility = Visibility.Collapsed;
        }

    }
}
