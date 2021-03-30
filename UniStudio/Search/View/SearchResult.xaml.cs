using Plugins.Shared.Library.Librarys;
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

namespace UniStudio.Search.View
{
    /// <summary>
    /// SearchResult.xaml 的交互逻辑
    /// </summary>
    public partial class SearchResult : UserControl
    {
        public SearchResult()
        {
            InitializeComponent();

            this.listBox.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            //RuntimeWatcherManager.Instance.Trace("搜索","列表条目渲染");
            //RuntimeWatcherManager.Instance.EndRuntimeWatcher("搜索");
        }
    }
}
