using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using UniStudio.Community.Search.Models;
using UniStudio.Community.Search.Models.SearchLocations;

namespace UniStudio.Community.ViewModel
{
    public class SearchResultViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private List<SearchDataUnit> _searchDataUnits;

        /// <summary>
        /// The <see cref="HasResult" /> property's name.
        /// </summary>
        public const string HasResultPropertyName = "HasResult";

        private bool _hasResultProperty;

        /// <summary>
        /// Sets and gets the HasResult property.
        /// </summary>
        public bool HasResult
        {
            get
            {
                return _hasResultProperty;
            }

            set
            {
                if (_hasResultProperty == value)
                {
                    return;
                }

                _hasResultProperty = value;
                RaisePropertyChanged(HasResultPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SearchText" /> property's name.
        /// </summary>
        public const string SearchTextPropertyName = "SearchText";

        private string _searchTextProperty;

        /// <summary>
        /// Sets and gets the SearchText property.
        /// </summary>
        public string SearchText
        {
            get
            {
                return _searchTextProperty;
            }
            set
            {
                if (_searchTextProperty == value)
                {
                    return;
                }

                _searchTextProperty = value;
                RaisePropertyChanged(SearchTextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SearchData" /> property's name.
        /// </summary>
        public const string SearchDataPropertyName = "SearchData";

        private ObservableCollection<SearchDataUnit> _searchDataProperty = new ObservableCollection<SearchDataUnit>();

        /// <summary>
        /// Sets and gets the SearchData property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SearchDataUnit> SearchData
        {
            get
            {
                return _searchDataProperty;
            }

            set
            {
                if (_searchDataProperty == value)
                {
                    return;
                }

                _searchDataProperty = value;
                RaisePropertyChanged(SearchDataPropertyName);
            }
        }

        #region Commands

        private RelayCommand<SearchLocation> _locateCommand;

        /// <summary>
        /// Gets the LocateCommand.
        /// </summary>
        public RelayCommand<SearchLocation> LocateCommand
        {
            get
            {
                return _locateCommand
                    ?? (_locateCommand = new RelayCommand<SearchLocation>(
                    (searchLocation) =>
                    {
                        var searchDataUnit = _searchDataUnits.FirstOrDefault(u => u.SearchLocation == searchLocation);
                        if (searchDataUnit != null)
                        {
                            Context.Current.LocateOperationService.LocateTo(searchDataUnit);
                        }
                    }));
            }
        }

        #endregion

        public SearchResultViewModel()
        {

        }

        public void InitData(List<SearchDataUnit> searchDataUnits,string searchText)
        {
            SearchText = searchText;
            _searchDataUnits = searchDataUnits;
            if (searchDataUnits?.Count > 0)
            {
                HasResult = true;
                SearchData = new ObservableCollection<SearchDataUnit>(searchDataUnits);
            }
            else
            {
                SearchData.Clear();
                HasResult = false;
            }
        }
    }
}