using System;
using System.Windows.Controls;
using Plugins.Shared.Library.Librarys;

namespace UniStudio.Community.Search.View
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
