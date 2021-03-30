using log4net;
using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.Librarys;
using ReflectionMagic;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.Validation;
using System.Activities.Presentation.ViewState;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using UniStudio.Librarys;
using UniStudio.Search.Enums;
using UniStudio.Search.Models;
using UniStudio.Search.Models.SearchLocations;
using UniStudio.ViewModel;

namespace UniStudio.Search.Data
{
    public class SearchDataManager
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Timer _timer;

        private List<SearchDataUnit> _tempSearchData;

        private List<string> _excludePropperties;

        private List<Type> _availableActivityTypes=new List<Type>();

        private Dictionary<Type, ActivityTreeItem> _activityTypeTreeItemDic = new Dictionary<Type, ActivityTreeItem>();

        private UnOpenedDocumentManager unOpenedDocumentManager = new UnOpenedDocumentManager();

        private object lockObj = new object();

        private List<SearchDataUnit> _searchData;

        public List<SearchDataUnit> SearchData
        {
            get
            {
                lock (lockObj)
                {
                    return _searchData;
                }
            }
            set
            {
                lock (lockObj)
                {
                    _searchData = value;
                }
            }
        }

        public SearchDataManager()
        {
            _excludePropperties = new List<string>
            {
                "DisplayName",
            };

            var typeNames = ViewModelLocator.instance.Activities.ActivityTreeItemAssemblyQualifiedNameDic.Keys;
            foreach(var item in ViewModelLocator.instance.Activities.ActivityTreeItemAssemblyQualifiedNameDic)
            {
                var type = Type.GetType(item.Key);
                if(type!=null)
                {
                    _availableActivityTypes.Add(type);
                    _activityTypeTreeItemDic.Add(type, item.Value);
                }
            }

            //_timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
            //{
            //    Interval = TimeSpan.FromMilliseconds(300000),                
            //};
            //_timer.Tick += BuildSearchData;
            _timer = new Timer(BuildSearchData);
        }

        public void BuildSearchIndexes()
        {

        }

        #region 构建搜索数据

        private void BuildUnOpenedSearchData()
        {
            var workflows = unOpenedDocumentManager.CreateWorkflows();
            foreach(var workflow in workflows)
            {
                var context = workflow.Context;
                var modelService = context.Services.GetService<ModelService>();

                List<ModelItem> activities = null;
                Application.Current.Dispatcher.Invoke(() => 
                {
                    activities = modelService.FindAllActivities(IsAvailableActivity);
                });
                foreach (var activityItem in activities)
                {
                    BuildDesignerActivity(activityItem, workflow.RelativeXmalPath);
                }
                BuildArguments(modelService.Root, workflow.RelativeXmalPath);

                #region 构建Imports
                var imports = TextExpression.GetNamespacesForImplementation(modelService.Root.GetCurrentValue());

                foreach (var import in imports)
                {
                    _tempSearchData.Add(new SearchDataUnit
                    {
                        CommonDataType = CommonDataType.Import,
                        SearchText = import,
                        DisplayText = import,
                        Path = $"{workflow.RelativeXmalPath.Replace('\\', '>')}",
                        RelativeFilePath = workflow.RelativeXmalPath,
                        SearchLocation = new ImportSearchLocation { FilePath = workflow.RelativeXmalPath, ImportName = import },
                        Icon = "pack://application:,,,/Resource/Image/Search/import.png"
                    });
                }
                #endregion
            }
        
        }

        private void BuildSearchData(object state)
        {
            StopBuildSearchData();
            try
            {
                _tempSearchData = new List<SearchDataUnit>();

                //未打开文档数据
                BuildUnOpenedSearchData();

                #region 已打开档数据
                foreach (var docuemnt in ViewModelLocator.instance.Dock.Documents.ToList())
                {
                    var documentContext = DocumentContext.GetContext(docuemnt);
                    var modelService = documentContext.Services.GetService<ModelService>();

                    List<ModelItem> activities = null;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        activities = modelService.FindAllActivities(IsAvailableActivity);
                    });
                    foreach (var activityItem in activities)
                    {
                        BuildDesignerActivity(activityItem, documentContext.RelativeFilePath);
                    }
                    BuildArguments(modelService.Root, documentContext.RelativeFilePath);
                    BuildImports(modelService.Root, documentContext.RelativeFilePath);
                }
                #endregion

                BuildActivities();

                SearchData = _tempSearchData;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, logger);
            }
            StartBuildSearchData();
        }

        private bool IsAvailableActivity(ModelItem modelItem)
        {
            var activityType = modelItem.ItemType;
            return _availableActivityTypes.Contains(activityType);
        }

        private void BuildDesignerActivity(ModelItem modelItem, string relativeFilePath)
        {
            if (modelItem == modelItem.Root)
            {
                return;
            }
            string displayName = modelItem.GetCurrentValue().AsDynamic().DisplayName;
            _tempSearchData.Add(new SearchDataUnit
            {
                CommonDataType = CommonDataType.DesignerActivity,
                SearchText = displayName,
                DisplayText = displayName,
                Path = relativeFilePath.Replace('\\', '>'),
                RelativeFilePath=relativeFilePath,
                SearchLocation = new DesignerActivitySearchLocation 
                { 
                    ActivityId = modelItem.Properties["Id"]?.ComputedValue?.ToString(),
                    IdRef= WorkflowViewState.GetIdRef(modelItem.GetCurrentValue()),
                    FilePath = relativeFilePath
                },
                Icon= _activityTypeTreeItemDic[modelItem.ItemType].Icon
            });
            BuildVariables(modelItem, relativeFilePath);
            BuildProperties(modelItem, relativeFilePath);
        }

        private string GetActivityItemPath(ActivityTreeItem activityTreeItem)
        {
            var path = new StringBuilder();
            var parent = activityTreeItem.Parent;
            while(parent!=null)
            {
                path = path.Insert(0, $"{parent.Name}>");
                parent = parent.Parent;
            }
            if(path.Length>0)
            {
                path.Remove(path.Length - 1,1);
            }
            return path.ToString();
        }

        private void BuildActivities()
        {
            var itemFavorite = ViewModelLocator.instance.Activities.ItemFavorites;
            foreach (var item in itemFavorite.Children.ToList())
            {
                _tempSearchData.Add(new SearchDataUnit
                {
                    CommonDataType = CommonDataType.Activity,
                    SearchText = $"{item.Name}$${(item.NameEnglish ?? "")}$${(item.NamePinYin ?? "")}$${(item.NamePinYinAbbr ?? "")}",
                    DisplayText = item.Name,
                    Path = itemFavorite.Name,
                    SearchLocation = new ActivitySearchLocation { ActivityName = item.Name },
                    Icon=item.Icon
                });
            }

            var itemRecent = ViewModelLocator.instance.Activities.ItemRecent;
            foreach (var item in itemRecent.Children.ToList())
            {
                _tempSearchData.Add(new SearchDataUnit
                {
                    CommonDataType = CommonDataType.Activity,
                    SearchText = $"{item.Name}$${(item.NameEnglish??"")}$${(item.NamePinYin ?? "")}$${(item.NamePinYinAbbr ?? "")}",
                    DisplayText = item.Name,
                    Path = itemRecent.Name,
                    SearchLocation=new ActivitySearchLocation { ActivityName=item.Name},
                    Icon = item.Icon
                });
            }

            var availableItems = ViewModelLocator.instance.Activities.ActivityTreeItemTypeOfDic.Values.ToList();
            foreach (var item in availableItems)
            {
                _tempSearchData.Add(new SearchDataUnit
                {
                    CommonDataType = CommonDataType.Activity,
                    SearchText = $"{item.Name}$${(item.NameEnglish ?? "")}$${(item.NamePinYin ?? "")}$${(item.NamePinYinAbbr ?? "")}",
                    DisplayText = item.Name,
                    Path = GetActivityItemPath(item),
                    SearchLocation = new ActivitySearchLocation { ActivityName = item.Name },
                    Icon = item.Icon
                });
            }
        }

        private void BuildVariables(ModelItem modelItem, string relativeFilePath)
        {
            var variables = modelItem.Properties["Variables"]?.Collection;
            if(variables==null||!variables.Any())
            {
                return;
            }
            string activityName = modelItem.GetCurrentValue().AsDynamic().DisplayName;
            foreach (var item in variables)
            {
                var varName = item.Properties["Name"].ComputedValue as string;
                _tempSearchData.Add(new SearchDataUnit
                {
                    CommonDataType = CommonDataType.Variable,
                    SearchText = varName,
                    DisplayText = varName,
                    Path = $"{relativeFilePath.Replace('\\', '>')}>{activityName}",
                    RelativeFilePath = relativeFilePath,
                    SearchLocation = new VariableSearchLocation
                    {
                        ActivityId = modelItem.Properties["Id"]?.ComputedValue?.ToString(),
                        IdRef = WorkflowViewState.GetIdRef(modelItem.GetCurrentValue()),
                        FilePath = relativeFilePath,
                        VariableName=varName
                    },
                    Icon= "pack://application:,,,/Resource/Image/Search/variable.png"
                });
            }
        }

        private void BuildArguments(ModelItem root, string relativeFilePath)
        {
            var properties = root.Properties["Properties"]?.Collection;
            if(properties==null||!properties.Any())
            {
                return;
            }
            foreach (var item in properties)
            {
                var name = item.Properties["Name"].ComputedValue as string;
                _tempSearchData.Add(new SearchDataUnit
                {
                    CommonDataType = GetCommonDataTypeFromModelItem(item),
                    SearchText = name,
                    DisplayText = name,
                    Path = relativeFilePath.Replace('\\', '>'),
                    RelativeFilePath = relativeFilePath,
                    SearchLocation =new ArgumentSearchLocation { Name=name,FilePath=relativeFilePath},
                    Icon = "pack://application:,,,/Resource/Image/Search/argument.png"
                });
            }
        }

        private CommonDataType GetCommonDataTypeFromModelItem(ModelItem modelItem)
        {
            var type = (modelItem.GetCurrentValue() as DynamicActivityProperty)?.Type;
            if(type!=null&&type.IsSubclassOf(typeof(Argument)))
            {
                var genericType= type.GetGenericTypeDefinition();
                if (genericType == typeof(InArgument<>))
                {
                    return CommonDataType.InArgument;
                }
                if(genericType==typeof(OutArgument<>))
                {
                    return CommonDataType.OutArgument;
                }
                return CommonDataType.InOutArgument;
            }
            return CommonDataType.PropertyArgument;
        }

        private void BuildProperties(ModelItem modelItem, string relativeFilePath)
        {
            var properties = modelItem.Properties;
            if (properties == null || !properties.Any())
            {
                return;
            }
            string activityName = modelItem.GetCurrentValue().AsDynamic().DisplayName;
            foreach (var item in properties)
            {
                if (item.IsBrowsable && !item.IsReadOnly&& !_excludePropperties.Contains(item.Name))
                {
                    var displayNameAttr = item.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault();
                    var displayName = displayNameAttr?.DisplayName ?? item.Name;
                    var expression = (item.ComputedValue as Argument)?.Expression?.ToString() ?? "Empty Value";
                    _tempSearchData.Add(new SearchDataUnit
                    {
                        CommonDataType = CommonDataType.Property,
                        SearchText = $"{displayName}$${expression}",
                        DisplayText = $"{displayName}: {expression}",
                        Path = $"{relativeFilePath.Replace('\\', '>')}>{activityName}",
                        RelativeFilePath = relativeFilePath,
                        SearchLocation =new PropertySearchLocation
                        {
                            ActivityId = modelItem.Properties["Id"]?.ComputedValue?.ToString(),
                            IdRef = WorkflowViewState.GetIdRef(modelItem.GetCurrentValue()),
                            FilePath =relativeFilePath,
                            PropertyName = item.Name
                        },
                        Icon = "pack://application:,,,/Resource/Image/Search/property.png"
                    });
                }
            }
        }

        private void BuildImports(ModelItem root, string relativeFilePath)
        {
            var imports = root.Properties["Imports"]?.Collection;
            if (imports == null || !imports.Any())
            {
                return;
            }
            foreach (var import in imports)
            { 
                var namespaceText=import.Properties["Namespace"]?.ComputedValue?.ToString() ?? "";
                _tempSearchData.Add(new SearchDataUnit
                {
                    CommonDataType = CommonDataType.Import,
                    SearchText = namespaceText,
                    DisplayText = namespaceText,
                    Path = $"{relativeFilePath.Replace('\\', '>')}",
                    RelativeFilePath = relativeFilePath,
                    SearchLocation =new ImportSearchLocation { FilePath=relativeFilePath,ImportName=namespaceText},
                    Icon = "pack://application:,,,/Resource/Image/Search/import.png"
                });
            }
        }

        /// <summary>
        /// 开始构建数据
        /// </summary>
        public void StartBuildSearchData(bool immediate=false)
        {
            //_timer.Start();
            if (immediate)
            {
                _timer.Change(0, 0);
            }
            else
            {
                _timer.Change(20000, 0);
            }
        }

        /// <summary>
        /// 停止构建数据
        /// </summary>
        public void StopBuildSearchData()
        {
            //_timer.Stop();

            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        #endregion
    }
}
