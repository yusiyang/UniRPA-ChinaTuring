using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniStudio.Search.View;
using Plugins.Shared.Library.Librarys;
using UniStudio.Search.Enums;
using UniStudio.ViewModel;
using System.Windows.Controls.Primitives;

namespace UniStudio.Search
{
    public class SearchViewManager
    {
        private Dictionary<SearchType, SearchView> _searchViewDic = new Dictionary<SearchType, SearchView>();

        private static object _lockObj = new object();

        public SearchView Current { get; private set; }

        public SearchView GetSearchView(SearchType searchType)
        {
            return _searchViewDic.Locking(d =>
            {
                if (!d.TryGetValue(searchType, out var searchView))
                {
                    searchView = new SearchView(searchType);
                    d.Add(searchType, searchView);
                }
                return searchView;
            });
        }

        public void Show(SearchType searchType)
        {
            if (Current != null)
            {
                Current.IsOpen = false;
            }
            var searchView = GetSearchView(searchType);
            searchView.PlacementTarget = ViewModelLocator.instance.Dock.m_view;
            searchView.Placement = PlacementMode.Relative;
            searchView.Width = ViewModelLocator.instance.Dock.m_view.ActualWidth / 2;
            searchView.HorizontalOffset = ViewModelLocator.instance.Dock.m_view.ActualWidth / 4+ searchView.Width;
            searchView.VerticalOffset = 0;
            searchView.IsOpen = true;
            searchView.searchBox.Focus();
            Current = searchView;
        }
    }
}
