using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using UniStudio.Librarys;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Activities.Presentation.Model;
using System.Activities.Presentation;
using System.Activities;
using System.Reflection;
using log4net;
using System.Windows.Media;
using System.IO;
using System.Xml;
using UniStudio.WorkflowOperation;
using UniStudio.Search.Enums;
using UniStudio.Search.Models;
using UniStudio.Search.View;
using System.Linq;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Plugins.Shared.Library.Librarys;
using UniStudio.Search.Utils;
using Plugins.Shared.Library.Extensions;

namespace UniStudio.ViewModel
{
    public class SearchViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                
        private SearchType _searchType;

        public SearchType SearchType 
        { 
            get
            {
                return _searchType;
            }
            set
            {
                if(value==SearchType.Panel)
                {
                    ShowPanel = true;
                }
                else if(value==SearchType.Common)
                {
                    ShowCommonPanel = true;
                }
                else
                {
                    ShowCountPanel = true;
                }
                _searchType = value;
            }
        }
        
        /// <summary>
        /// The <see cref="SearchText" /> property's name.
        /// </summary>
        public const string SearchTextPropertyName = "SearchText";

        private string _searchTextProperty;

        /// <summary>
        /// Sets and gets the SearchText property.
        /// 是否是组件节点 
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
        /// The <see cref="ShowPanel" /> property's name.
        /// </summary>
        public const string ShowPanelPropertyName = "ShowPanel";

        private bool _showPanelProperty;

        /// <summary>
        /// Sets and gets the ShowPanel property.
        /// 是否是组件节点 
        /// </summary>
        public bool ShowPanel
        {
            get
            {
                return _showPanelProperty;
            }

            set
            {
                if (_showPanelProperty == value)
                {
                    return;
                }

                _showPanelProperty = value;
                RaisePropertyChanged(ShowPanelPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ShowCountPanel" /> property's name.
        /// </summary>
        public const string ShowCountPanelPropertyName = "ShowCountPanel";

        private bool _showCountPanelProperty;

        /// <summary>
        /// Sets and gets the ShowCountPanel property.
        /// 是否是组件节点 
        /// </summary>
        public bool ShowCountPanel
        {
            get
            {
                return _showCountPanelProperty;
            }

            set
            {
                if (_showCountPanelProperty == value)
                {
                    return;
                }

                _showCountPanelProperty = value;
                RaisePropertyChanged(ShowCountPanelPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="TotalCount" /> property's name.
        /// </summary>
        public const string TotalCountPropertyName = "TotalCount";

        private int _totalCountProperty;

        /// <summary>
        /// Sets and gets the TotalCount property.
        /// 是否是组件节点 
        /// </summary>
        public int TotalCount
        {
            get
            {
                return _totalCountProperty;
            }

            set
            {
                if (_totalCountProperty == value)
                {
                    return;
                }

                _totalCountProperty = value;

                ShowCountPanel = SearchType != SearchType.Common && SearchType != SearchType.Panel && value > 0;
                RaisePropertyChanged(TotalCountPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ShowCommonPanel" /> property's name.
        /// </summary>
        public const string ShowCommonPanelPropertyName = "ShowCommonPanel";

        private bool _showCommonPanelProperty;

        /// <summary>
        /// Sets and gets the ShowCommonPanel property.
        /// 是否是组件节点 
        /// </summary>
        public bool ShowCommonPanel
        {
            get
            {
                return _showCommonPanelProperty;
            }

            set
            {
                if (_showCommonPanelProperty == value)
                {
                    return;
                }

                _showCommonPanelProperty = value;
                RaisePropertyChanged(ShowCommonPanelPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ShowCommonSearchTypes" /> property's name.
        /// </summary>
        public const string ShowCommonSearchTypesPropertyName = "ShowCommonSearchTypes";

        private bool _showCommonSearchTypesProperty;

        /// <summary>
        /// Sets and gets the ShowCommonSearchTypes property.
        /// 是否是组件节点 
        /// </summary>
        public bool ShowCommonSearchTypes
        {
            get
            {
                return _showCommonSearchTypesProperty;
            }

            set
            {
                if (_showCommonSearchTypesProperty == value)
                {
                    return;
                }

                _showCommonSearchTypesProperty = value;
                RaisePropertyChanged(ShowCommonSearchTypesPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="CommonSearchType" /> property's name.
        /// </summary>
        public const string CommonSearchTypePropertyName = "CommonSearchType";

        private CommonSearchType _commonSearchTypeProperty;

        /// <summary>
        /// Sets and gets the CommonSearchType property.
        /// 是否是组件节点 
        /// </summary>
        public CommonSearchType CommonSearchType
        {
            get
            {
                return _commonSearchTypeProperty;
            }

            set
            {
                if (_commonSearchTypeProperty == value)
                {
                    return;
                }

                _commonSearchTypeProperty = value;
                RaisePropertyChanged(CommonSearchTypePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="CommonSearchInfos" /> property's name.
        /// </summary>
        public const string CommonSearchInfosPropertyName = "CommonSearchInfos";

        private ObservableCollection<CommonSearchInfo> _commonSearchInfosProperty = new ObservableCollection<CommonSearchInfo>();

        /// <summary>
        /// Sets and gets the CommonSearchInfos property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<CommonSearchInfo> CommonSearchInfos
        {
            get
            {
                return _commonSearchInfosProperty;
            }

            set
            {
                if (_commonSearchInfosProperty == value)
                {
                    return;
                }

                _commonSearchInfosProperty = value;
                RaisePropertyChanged(CommonSearchInfosPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="SelectedCommonSearchInfo" /> property's name.
        /// </summary>
        public const string SelectedCommonSearchInfoPropertyName = "SelectedCommonSearchInfo";

        private CommonSearchInfo _selectedCommonSearchInfoProperty;

        /// <summary>
        /// Sets and gets the SelectedCommonSearchInfo property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public CommonSearchInfo SelectedCommonSearchInfo
        {
            get
            {
                return _selectedCommonSearchInfoProperty;
            }

            set
            {
                if (_selectedCommonSearchInfoProperty == value)
                {
                    return;
                }

                _selectedCommonSearchInfoProperty = value;
                RaisePropertyChanged(SelectedCommonSearchInfoPropertyName);
            }
        }

        #region Commands

        private RelayCommand _searchViewCommand;

        /// <summary>
        /// Gets the OpenSearchViewCommand.
        /// </summary>
        public RelayCommand SearchCommand
        {
            get
            {
                return _searchViewCommand
                    ?? (_searchViewCommand = new RelayCommand(
                    () =>
                    {
                        //RuntimeWatcherManager.Instance.StartRuntimeWatcher("搜索");
                        if (SearchType == SearchType.Panel)
                        {
                            return;
                        }
                        var searchView = Context.Current.SearchViewManager.GetSearchView(SearchType);
                        var searchResult = searchView.searchResult;
                        var searchResultViewModel = searchResult.DataContext as SearchResultViewModel;
                        List<SearchDataUnit> searchResultData = null;
                        Dictionary<CommonSearchType, int> countResult = null;

                        if (!string.IsNullOrWhiteSpace(SearchText) || SearchType != SearchType.Common)
                        {
                            var searchParams = new SearchParams(SearchType, CommonSearchType, SearchText);
                            searchResultData = Context.Current.SearchService.DoSearch(searchParams, out countResult);
                        }
                        //RuntimeWatcherManager.Instance.Trace("搜索", "查询数据");

                        searchResultViewModel.InitData(searchResultData, SearchText);

                        //RuntimeWatcherManager.Instance.Trace("搜索", "更新数据");

                        if (SearchType == SearchType.Common)
                        {
                            RefreshCommonSearchInfos(countResult);
                        }

                        if (!string.IsNullOrWhiteSpace(SearchText))
                        {
                            searchResult.Visibility = Visibility.Visible;
                        }
                        else if (SearchType != SearchType.Common)
                        {
                            searchResult.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            searchResult.Visibility = Visibility.Collapsed;
                        }

                        TotalCount = searchResultData?.Count ?? 0;


                        //RuntimeWatcherManager.Instance.Trace("搜索", "刷新搜索面板");
                    }));
            }
        }

        #endregion

        public SearchViewModel()
        {

        }

        private void RefreshCommonSearchInfos(Dictionary<CommonSearchType, int> countResult)
        {
            var searchView = Context.Current.SearchViewManager.GetSearchView(SearchType);
            var oldFlag = searchView.ShouldTriggerSelectionChanged;
            try
            {
                searchView.ShouldTriggerSelectionChanged = false;
                CommonSearchInfos.Clear();
                var enumInfos = CommonSearchType.CurrentFile.GetEnumInfos();

                int index = 1;
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    foreach (var enumInfo in enumInfos)
                    {
                        CommonSearchInfos.Add(new CommonSearchInfo
                        {
                            CommonSearchType = enumInfo.Key,
                            Display = enumInfo.Value,
                            Count = 0,
                            Index = index++
                        });
                    }
                    ShowCommonSearchTypes = true;
                    return;
                }

                if (countResult?.Count > 0)
                {
                    foreach (var commonSearchTypeCount in countResult)
                    {
                        if (commonSearchTypeCount.Value <= 0)
                        {
                            continue;
                        }
                        CommonSearchInfos.Add(new CommonSearchInfo
                        {
                            CommonSearchType = commonSearchTypeCount.Key,
                            Display = enumInfos[commonSearchTypeCount.Key],
                            Count = commonSearchTypeCount.Value,
                            Index = index++
                        });
                    }
                }
                ShowCommonSearchTypes = CommonSearchInfos.Count > 0;
                var searchInfo = CommonSearchInfos.FirstOrDefault(i => i.CommonSearchType == CommonSearchType);
                if (searchInfo == null)
                {
                    SelectedCommonSearchInfo = CommonSearchInfos.FirstOrDefault();
                }
                else
                {
                    SelectedCommonSearchInfo = searchInfo;
                }
            }
            catch(Exception ex)
            {
                UniMessageBox.Show(App.Current.MainWindow, ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error(ex, logger);
            }
            finally
            {
                searchView.ShouldTriggerSelectionChanged = oldFlag;
            }
        }
    }
}