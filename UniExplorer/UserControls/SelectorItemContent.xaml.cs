using System;
using System.Collections.Generic;
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

namespace UniExplorer.UserControls
{
    /// <summary>
    /// SelectorItemContent.xaml 的交互逻辑
    /// </summary>
    public partial class SelectorItemContent : UserControl
    {
        public SelectorItemContent()
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
            _selectorItemsListBox.SelectedItem = selectedAttrbute;
        }
    }
}
