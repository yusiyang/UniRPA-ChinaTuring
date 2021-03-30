using System;
using UniStudio.Community.Search;
using UniStudio.Community.Search.Data;
using UniStudio.Community.Search.Services;

namespace UniStudio.Community
{
    public class Context
    {
        private static object _lockObj = new object();
        private static bool _isCreated = false;
        private static Context _instance;

        public static Context Current => _instance;

        public Context()
        {
            lock (_lockObj)
            {
                if (_isCreated)
                {
                    throw new InvalidOperationException("实例只能创建一次");
                }
                _instance = this;
                _isCreated = true;
            }
        }

        private SearchService _searchService;

        public SearchService SearchService
        {
            get
            {
                if(_searchService == null)
                {
                    _searchService = new SearchService();
                }
                return _searchService;
            }
        }

        private SearchDataManager _searchDataManager;

        public SearchDataManager SearchDataManager
        {
            get
            {
                if(_searchDataManager == null)
                {
                    _searchDataManager = new SearchDataManager();
                }
                return _searchDataManager;
            }
        }

        private LocateOperationService _locateOperationService;

        public LocateOperationService LocateOperationService
        {
            get
            {
                if(_locateOperationService==null)
                {
                    _locateOperationService = new LocateOperationService();
                }
                return _locateOperationService;
            }
        }

        private SearchViewManager _searchViewManager;

        public SearchViewManager SearchViewManager
        {
            get
            {
                if (_searchViewManager == null)
                {
                    _searchViewManager = new SearchViewManager();
                }
                return _searchViewManager;
            }
        }
    }
}
