using GalaSoft.MvvmLight.Command;
using UniStudio.Community.Search.Enums;

namespace UniStudio.Community.ViewModel
{
    public partial class MainViewModel
    {
        #region 搜索
        private RelayCommand<SearchType> _openSearchViewCommand;

        /// <summary>
        /// Gets the OpenSearchViewCommand.
        /// </summary>
        public RelayCommand<SearchType> OpenSearchViewCommand
        {
            get
            {
                return _openSearchViewCommand
                    ?? (_openSearchViewCommand = new RelayCommand<SearchType>(
                    searchType =>
                    {
                        if(ViewModelLocator.instance.Main.IsOpenStartScreen)
                        {
                            return;
                        }
                        Context.Current.SearchViewManager.Show(searchType);
                    }));
            }
        }
        #endregion
    }
}
