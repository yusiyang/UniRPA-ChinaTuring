using Plugins.Shared.Library.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using UniStudio.Community.ViewModel;

namespace UniStudio.Community.UserControls
{
    /// <summary>
    /// SettingContent.xaml 的交互逻辑
    /// </summary>
    public partial class SettingContent : UserControl
    {
        public SettingContent()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 当主题选项更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ViewModelLocator.instance.Setting.IsCodeTriggerThemeChange)
            {
                MessageBoxResult result = UniMessageBox.Show("主题已修改。重新启动 Studio 以应用更改。", "信息", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (result.Equals(MessageBoxResult.OK))
                {
                    string themeName = (((sender as ComboBox).SelectedItem) as ComboBoxItem).Content as string;

                    XmlDocument doc = new XmlDocument();
                    var path = ViewModelLocator.instance.Main.GlobalSettingsXmlPath;
                    doc.Load(path);
                    var rootNode = doc.DocumentElement;
                    XmlElement themeNode = rootNode.SelectNodes("Theme").Item(0) as XmlElement;
                    themeNode.SetAttribute("Name", themeName);
                    doc.Save(path);

                    // 重新启动
                    System.Windows.Forms.Application.Restart();
                    Application.Current.Shutdown();
                }
            }
        }

    }
}
