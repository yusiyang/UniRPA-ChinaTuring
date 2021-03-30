using Plugins.Shared.Library.Librarys;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using UniExplorer.ViewModel;

namespace UniExplorer.UserControls
{
    /// <summary>
    /// SelectorItemPropertyContent.xaml 的交互逻辑
    /// </summary>
    public partial class SelectorItemAttributeContent : UserControl
    {
        public SelectorItemAttributeContent()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 当焦点位于Textbox时，当前选择的条目也变为这一条。 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var selectedAttrbute = (sender as TextBox).DataContext;
            _attributesListBox.SelectedItem = selectedAttrbute;
        }

        /// <summary>
        /// 当焦点位于CheckBox时，当前选择的条目也变为这一条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var selectedAttrbute = (sender as CheckBox).DataContext;
            _attributesListBox.SelectedItem = selectedAttrbute;
        }

        /// <summary>
        /// 当属性值改变时发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAttributeValueChanged(object sender, TextChangedEventArgs e)
        {
            SelectorItemAttribute selectedSelectorItemAttribute = ViewModelLocator.instance.MainDock.SelectedSelectorItemAttribute;
            string currentAttributeValue = (sender as TextBox).Text;

            XmlDocument selectorItem = new XmlDocument();
            selectorItem.LoadXml(ViewModelLocator.instance.MainDock.SelectedSelectorItem.ItemContent);
            (selectorItem.FirstChild as XmlElement).SetAttribute(selectedSelectorItemAttribute.Name, currentAttributeValue);

            ViewModelLocator.instance.MainDock.SelectedSelectorItem.ItemContent = selectorItem.OuterXml;
            ViewModelLocator.instance.MainDock.SelectedSelectorItem.ItemContentFull = selectorItem.OuterXml;
        }

        /// <summary>
        /// 当属性选中状态改变时发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAttributeCheckChanged(object sender, RoutedEventArgs e)
        {
            SelectorItemAttribute selectedSelectorItemAttribute = ViewModelLocator.instance.MainDock.SelectedSelectorItemAttribute;
            bool isChecked = (bool)(sender as CheckBox).IsChecked;

            XmlDocument selectorItem = new XmlDocument();
            selectorItem.LoadXml(ViewModelLocator.instance.MainDock.SelectedSelectorItem.ItemContent);

            if (isChecked)
            {
                (selectorItem.FirstChild as XmlElement).SetAttribute(selectedSelectorItemAttribute.Name, selectedSelectorItemAttribute.Value);
            }
            else
            {
                (selectorItem.FirstChild as XmlElement).RemoveAttribute(selectedSelectorItemAttribute.Name);
            }

            ViewModelLocator.instance.MainDock.SelectedSelectorItem.ItemContent = selectorItem.OuterXml;
        }

    }
}
