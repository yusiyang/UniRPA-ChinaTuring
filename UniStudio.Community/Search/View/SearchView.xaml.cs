using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Plugins.Shared.Library.Librarys;
using UniStudio.Community.Search.Enums;
using UniStudio.Community.ViewModel;

namespace UniStudio.Community.Search.View
{
    /// <summary>
    /// SearchView.xaml 的交互逻辑
    /// </summary>
    public partial class SearchView : Popup
    {
        public bool ShouldTriggerSelectionChanged { get; set; } = true;

        public SearchView(SearchType searchType)
        {
            InitializeComponent();

            Init(searchType);
        }

        private void Init(SearchType searchType)
        {
            var searchViewModel = this.DataContext as SearchViewModel;
            searchViewModel.SearchType = searchType;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if(listBox.SelectedItem==null)
            {
                return;
            }
            if(listBox.SelectedIndex==0)
            {
                Context.Current.SearchViewManager.Show(SearchType.AddActivity);
            }
            else if (listBox.SelectedIndex == 1)
            {
                Context.Current.SearchViewManager.Show(SearchType.Common);
            }
            else if (listBox.SelectedIndex == 2)
            {
                Context.Current.SearchViewManager.Show(SearchType.GoToFile);
            }
            else if (listBox.SelectedIndex == 3)
            {
                Context.Current.SearchViewManager.Show(SearchType.LocateActivity);
            }
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            var searchViewModel = this.DataContext as SearchViewModel;
            if (searchViewModel.SearchType == SearchType.Panel)
            {
                searchViewModel.SearchText = string.Empty;
                searchViewModel.ShowPanel = true;
                searchPanelList.SelectedIndex = -1;
            }
            else
            {
                searchViewModel.SearchCommand.Execute(null);
            }
        }

        private void commonSearchPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if(!e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                return;
            }
            if(!(e.Key>=Key.D1&&e.Key<=Key.D9||e.Key>=Key.NumPad1&&e.Key<=Key.NumPad9))
            {
                return;
            }
            var keyStr = e.Key.ToString();
            var index = keyStr.Substring(keyStr.Length - 1).ToInt() - 1;

            var searchViewModel = this.DataContext as SearchViewModel;
            if(index>searchViewModel.CommonSearchInfos.Count-1)
            {
                return;
            }

            var listBox = sender as ListBox;
            listBox.SelectedIndex = index;

            var commonSearchType = searchViewModel.CommonSearchInfos[index].CommonSearchType;
            searchViewModel.CommonSearchType = commonSearchType;

            searchViewModel.SearchCommand.Execute(null);
        }

        private void commonSearchPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!ShouldTriggerSelectionChanged)
            {
                return;
            }
            var listBox = sender as ListBox;
            if (listBox.SelectedItem == null)
            {
                return;
            }

            var searchViewModel = this.DataContext as SearchViewModel;

            var commonSearchType = searchViewModel.CommonSearchInfos[listBox.SelectedIndex].CommonSearchType;
            searchViewModel.CommonSearchType = commonSearchType;

            searchViewModel.SearchCommand.Execute(null);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            var searchViewModel = this.DataContext as SearchViewModel;
            searchViewModel.SearchText = textBox.Text;
            searchViewModel.SearchCommand.Execute(null);
        }

        private void Popup_KeyDown(object sender, KeyEventArgs e)
        {
            var searchViewModel = this.DataContext as SearchViewModel;
            if(searchViewModel.SearchType!=SearchType.Common)
            {
                return;
            }

            if (!e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                return;
            }
            if (!(e.Key >= Key.D1 && e.Key <= Key.D9 || e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9))
            {
                return;
            }
            var keyStr = e.Key.ToString();
            var index = keyStr.Substring(keyStr.Length - 1).ToInt() - 1;

            if (index > searchViewModel.CommonSearchInfos.Count - 1)
            {
                return;
            }

            commonSearchPanel.SelectedIndex = index;

            var commonSearchType = searchViewModel.CommonSearchInfos[index].CommonSearchType;
            searchViewModel.CommonSearchType = commonSearchType;

            searchViewModel.SearchCommand.Execute(null);
        }
    }
}
