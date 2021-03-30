using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using Newtonsoft.Json.Linq;
using Plugins.Shared.Library;
using UniStudio.DataManager;
using UniStudio.Executor;
using UniStudio.ExpressionEditor;
using UniStudio.Librarys;
using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Hosting;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using WorkflowUtils;
using System.Threading.Tasks;
using System.Threading;
using System.Activities.Presentation.Validation;
using System.Reflection;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Presentation.Debug;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using System.Windows.Controls;
using Plugins.Shared.Library.Extensions;
using UniStudio.WorkflowOperation;
using System.Xaml;
using System.Collections;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Plugins.Shared.Library.UiAutomation;

namespace UniStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocumentViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public WorkflowDesigner WorkflowDesignerInstance { get; set; }

        public bool IsAlwaysReadOnly { get; set; }//记录是否一直保持只读状态，比如代码片断文件

        public string ActivityBuilderDisplayName = "";

        /// <summary>
        /// The <see cref="WorkflowDesignerView" /> property's name.
        /// </summary>
        public const string WorkflowDesignerViewPropertyName = "WorkflowDesignerView";

        private object _workflowDesignerViewProperty = null;

        /// <summary>
        /// Sets and gets the WorkflowDesignerView property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public object WorkflowDesignerView
        {
            get
            {
                return _workflowDesignerViewProperty;
            }

            set
            {
                if (_workflowDesignerViewProperty == value)
                {
                    return;
                }

                _workflowDesignerViewProperty = value;
                RaisePropertyChanged(WorkflowDesignerViewPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsDebugging" /> property's name.
        /// </summary>
        public const string IsDebuggingPropertyName = "IsDebugging";

        private bool _isDebuggingProperty = false;

        /// <summary>
        /// Sets and gets the IsDebugging property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsDebugging
        {
            get
            {
                return _isDebuggingProperty;
            }

            set
            {
                if (_isDebuggingProperty == value)
                {
                    return;
                }

                _isDebuggingProperty = value;
                RaisePropertyChanged(IsDebuggingPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsReadOnly" /> property's name.
        /// </summary>
        public const string IsReadOnlyPropertyName = "IsReadOnly";

        private bool _isReadOnlyProperty = false;

        /// <summary>
        /// Sets and gets the IsReadOnly property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return _isReadOnlyProperty;
            }

            set
            {
                if (IsAlwaysReadOnly && !value)
                {
                    return;
                }


                _isReadOnlyProperty = value;
                RaisePropertyChanged(IsReadOnlyPropertyName);
                IsEnable = !value;
                WorkflowDesignerInstance.Context.Items.GetValue<ReadOnlyState>().IsReadOnly = value;
                WorkflowDesignerInstance.Context.Services.GetService<DesignerView>().IsReadOnly = value;

                if (value)
                {
                    CompositeTitle = Title + " (只读)";
                }
                else
                {
                    CompositeTitle = Title;
                }
            }
        }

        /// <summary>
        /// IsEnable = !IsReadOnly
        /// </summary>
        private bool _isEnable;
        public bool IsEnable { get { return _isEnable; } set { Set(ref _isEnable, value); } }

        /// <summary>
        /// The <see cref="IsDirty" /> property's name.
        /// </summary>
        public const string IsDirtyPropertyName = "IsDirty";

        private bool _isDirtyProperty = false;

        /// <summary>
        /// Sets and gets the IsDirty property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsDirty
        {
            get
            {
                return _isDirtyProperty;
            }

            set
            {
                _isDirtyProperty = value;
                RaisePropertyChanged(IsDirtyPropertyName);

                if (value)
                {
                    CompositeTitle = Title + " *";
                }
                else
                {
                    CompositeTitle = Title;
                }
            }
        }


        public void UpdateCompositeTitle()
        {
            IsReadOnly = IsReadOnly;
            IsDirty = IsDirty;
        }


        /// <summary>
        /// The <see cref="CompositeTitle" /> property's name.
        /// </summary>
        public const string CompositeTitlePropertyName = "CompositeTitle";

        private string _compositeTitleProperty = "";

        /// <summary>
        /// Sets and gets the CompositeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string CompositeTitle
        {
            get
            {
                return _compositeTitleProperty;
            }

            set
            {
                if (_compositeTitleProperty == value)
                {
                    return;
                }

                _compositeTitleProperty = value;
                RaisePropertyChanged(CompositeTitlePropertyName);
            }
        }





        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private string _titleProperty = "";

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {
                return _titleProperty;
            }

            set
            {
                if (_titleProperty == value)
                {
                    return;
                }

                _titleProperty = value;
                RaisePropertyChanged(TitlePropertyName);
            }
        }


        /// <summary>
        /// The <see cref="XamlPath" /> property's name.
        /// </summary>
        public const string XamlPathPropertyName = "XamlPath";

        private string _xamlPathProperty = "";

        /// <summary>
        /// Sets and gets the XamlPath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string XamlPath
        {
            get
            {
                return _xamlPathProperty;
            }

            set
            {
                if (_xamlPathProperty == value)
                {
                    return;
                }

                _xamlPathProperty = value;
                RaisePropertyChanged(XamlPathPropertyName);

                //设置RelativeXamlPath
                RelativeXamlPath = Common.MakeRelativePath(SharedObject.Instance.ProjectPath, value);
            }
        }


        /// <summary>
        /// 文件相对于项目的相对路径
        /// </summary>
        public const string RelativeXamlPathPropertyName = "RelativeXamlPath";

        private string _relativeXamlPathProperty = "";

        /// <summary>
        /// Sets and gets the RelativeXamlPath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string RelativeXamlPath
        {
            get
            {
                return _relativeXamlPathProperty;
            }

            set
            {
                if (_relativeXamlPathProperty == value)
                {
                    return;
                }

                _relativeXamlPathProperty = value;
                RaisePropertyChanged(RelativeXamlPathPropertyName);
            }
        }

        public EventHandler ValidationCompleted { get; set; }

        public bool DoCloseDocument()
        {
            if (IsDebugging)
            {
                var ret = UniMessageBox.Show(App.Current.MainWindow, string.Format("当前文档正在被调试，确定终止调试并关闭\"{0}\"吗？", XamlPath), "询问", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (ret == MessageBoxResult.Yes)
                {
                    ViewModelLocator.instance.Main.StopWorkflowCommand.Execute(null);
                }
                else if (ret == MessageBoxResult.No)
                {
                    return false;
                }
            }

            //当前文档窗口关闭
            bool isClose = true;
            if (IsDirty)
            {
                var ret = UniMessageBox.Show(App.Current.MainWindow, string.Format("文件有修改，需要保存文件\"{0}\"吗？", XamlPath), "询问", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                if (ret == MessageBoxResult.Yes)
                {
                    SaveDocument();
                }
                else if (ret == MessageBoxResult.No)
                {

                }
                else
                {
                    isClose = false;
                }
            }

            if (isClose)
            {
                SaveBreakpoints();
                ViewModelLocator.instance.Main.DebuggerService = null;
                DocumentContext.Remove(this);
                Messenger.Default.Send(this, "Close");
                Messenger.Default.Unregister(this);//取消注册
            }

            return isClose;
        }

        private void SaveBreakpoints()
        {
            try
            {
                var context = DocumentContext.GetContext(this);
                var debuggerManager = context.DebuggerManager;

                var locationActivityIdMapping = debuggerManager.GetActivityIdLocationMapping(true).ToDictionary(d => d.Value, d => d.Key);
                if (locationActivityIdMapping.Count == 0)
                {
                    return;
                }

                var breakpointLocations = WorkflowDesignerInstance.DebugManagerView.GetBreakpointLocations();

                ProjectSettingsDataManager.instance.m_projectBreakpointsDataManager.RemoveAllBreakpointsLocation(RelativeXamlPath);
                foreach (var breakpointLocation in breakpointLocations)
                {
                    if (breakpointLocation.Value != (BreakpointTypes.Enabled | BreakpointTypes.Bounded))
                    {
                        continue;
                    }
                    var activityId = locationActivityIdMapping[breakpointLocation.Key];
                    ProjectSettingsDataManager.instance.m_projectBreakpointsDataManager.AddBreakpointLocation(RelativeXamlPath, activityId, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, logger);
            }
        }

        private RelayCommand _closeCommand;

        /// <summary>
        /// Gets the CloseCommand.
        /// </summary>
        public RelayCommand CloseCommand
        {
            get
            {
                return _closeCommand
                    ?? (_closeCommand = new RelayCommand(
                    () =>
                    {
                        DoCloseDocument();
                    },
                    () => true));
            }
        }


        /// <summary>
        /// The <see cref="ContentId" /> property's name.
        /// </summary>
        public const string ContentIdPropertyName = "ContentId";

        private string _contentIdProperty = System.Guid.NewGuid().ToString();

        /// <summary>
        /// Sets and gets the ContentId property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ContentId
        {
            get
            {
                return _contentIdProperty;
            }

            set
            {
                if (_contentIdProperty == value)
                {
                    return;
                }

                _contentIdProperty = value;
                RaisePropertyChanged(ContentIdPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsSelected" /> property's name.
        /// </summary>
        public const string IsSelectedPropertyName = "IsSelected";

        private bool _isSelectedProperty = false;

        /// <summary>
        /// 当前文档是否是用户正在操作的
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelectedProperty;
            }

            set
            {
                //if (_isSelectedProperty == value)
                //{
                //    return;
                //}

                _isSelectedProperty = value;
                RaisePropertyChanged(IsSelectedPropertyName);

                if (value)
                {
                    //当前文档窗口激活
                    Messenger.Default.Send(this, "IsSelected");
                }
            }
        }



        public void SaveDocument()
        {
            //保存xaml到文件中
            if (!IsReadOnly)
            {
                WorkflowDesignerInstance.Flush();
                WorkflowDesignerInstance.Save(XamlPath);
                var xamlText = WorkflowDesignerInstance.Text;
                File.WriteAllText(XamlPath, xamlText);
                IsDirty = false;
            }
        }

        //private void ParseCopyDataProc(string copyData)
        //{
        //    var modelService = this.WorkflowDesignerInstance.Context.Services.GetService<ModelService>();
        //    var modelItem = modelService.Find(modelService.Root, typeof(Activity));

        //    JsonData jsonData = JsonMapper.ToObject(copyData);
        //    if (Equals(jsonData["cmd"].ToString(), "grab"))
        //    {
        //        foreach (ModelItem item in modelItem)
        //        {
        //            if (Equals(item.ItemType.Name, jsonData["className"].ToString()))
        //            {
        //                List<ModelProperty> PropertyList = item.Properties.ToList();
        //                ModelProperty propertyInfo = PropertyList.Find((ModelProperty property) => property.Name.Equals("guid"));
        //                if (Equals(propertyInfo.ComputedValue, jsonData["classID"].ToString()))
        //                {
        //                    ModelProperty imgProperty = PropertyList.Find((ModelProperty property) => property.Name.Equals("SourceImgPath"));

        //                    var imgFilePath = jsonData["savePath"].ToString();
        //                    if (imgFilePath.StartsWith(SharedObject.Instance.ProjectPath+@"\.screenshots\", System.StringComparison.CurrentCultureIgnoreCase))
        //                    {
        //                        //如果在项目目录下，则使用相对路径保存
        //                        imgFilePath = Common.MakeRelativePath(SharedObject.Instance.ProjectPath + @"\.screenshots\", imgFilePath);
        //                    }

        //                    if (imgProperty != null) imgProperty.SetValue(imgFilePath);


        //                    ModelProperty visiProperty = PropertyList.Find((ModelProperty property) => property.Name.Equals("visibility"));
        //                    if (visiProperty != null) visiProperty.SetValue(System.Windows.Visibility.Visible);
        //                    ModelProperty offsetXProperty = PropertyList.Find((ModelProperty property) => property.Name.Equals("offsetX"));
        //                    if (offsetXProperty != null)
        //                    {
        //                        InArgument<Int32> offsetX = Convert.ToInt32(jsonData["offsetX"].ToString());
        //                        offsetXProperty.SetValue(offsetX);
        //                    }
        //                    ModelProperty offsetYProperty = PropertyList.Find((ModelProperty property) => property.Name.Equals("offsetY"));
        //                    if (offsetYProperty != null)
        //                    {
        //                        InArgument<Int32> offsetY = Convert.ToInt32(jsonData["offsetY"].ToString());
        //                        offsetYProperty.SetValue(offsetY);
        //                    }

        //                    //暂定组织的SELECTOR字段
        //                    if (jsonData["className"].ToString().Equals("WindowClose") || jsonData["className"].ToString().Equals("WindowAttach"))
        //                    {
        //                        Int32 offsetX = Convert.ToInt32(jsonData["offsetX"].ToString());
        //                        Int32 offsetY = Convert.ToInt32(jsonData["offsetY"].ToString());

        //                        int hwnd = WindowFromPoint(offsetX, offsetY);
        //                        StringBuilder windowText = new StringBuilder(256);
        //                        GetWindowText(hwnd, windowText, 256);
        //                        StringBuilder className = new StringBuilder(256);
        //                        GetClassName(hwnd, className, 256);

        //                        System.Diagnostics.Debug.WriteLine("formHandle 窗口句柄 : " + hwnd);
        //                        System.Diagnostics.Debug.WriteLine("windowText 窗口的标题: " + windowText);
        //                        System.Diagnostics.Debug.WriteLine("className: " + className);

        //                        ModelProperty SelectorProperty = PropertyList.Find((ModelProperty property) => property.Name.Equals("Selector"));
        //                        InArgument<string> Selector = "<" + "Handle:" + hwnd
        //                            + " WindowText:" + windowText + " ClassName:" + className + ">";
        //                        SelectorProperty.SetValue(Selector);
        //                        ModelProperty hwndProperty = PropertyList.Find((ModelProperty property) => property.Name.Equals("hwnd"));
        //                        hwndProperty.SetValue(hwnd);
        //                    }

        //                    ModelProperty targetHwndProperty = PropertyList.Find((ModelProperty property) => property.Name.Equals("targetHwnd"));
        //                    if (targetHwndProperty != null)
        //                    {
        //                        int targetHwnd = Convert.ToInt32(jsonData["targetHwnd"].ToString());
        //                        targetHwndProperty.SetValue(targetHwnd);
        //                    }
        //                    ModelProperty displayProperty = PropertyList.Find((ModelProperty property) => property.Name.Equals("DisplayName"));
        //                    if (displayProperty != null)
        //                    {
        //                        displayProperty.SetValue(jsonData["DisplayName"].ToString());
        //                    }
        //                    ModelProperty processPathProperty = PropertyList.Find((ModelProperty property) => property.Name.Equals("ProcessPath"));
        //                    if (processPathProperty != null)
        //                    {
        //                        int targetHwnd = Convert.ToInt32(jsonData["targetHwnd"].ToString());
        //                        uint psID;
        //                        char[] buf = new char[65535];
        //                        Win32Api.GetWindowThreadProcessId((IntPtr)targetHwnd, out psID);
        //                        UInt32 len = 65535;
        //                        int calcProcess;
        //                        calcProcess = Win32Api.OpenProcess(Win32Api.PROCESS_QUERY_INFORMATION, false,(int) psID);
        //                        Win32Api.QueryFullProcessImageName((IntPtr)calcProcess, 0, buf, ref len);
        //                        string exeName = new string(buf, 0, (int)len);
        //                        InArgument<string> _value = exeName;
        //                        processPathProperty.SetValue(_value);
        //                    }
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    else if(Equals(jsonData["cmd"].ToString(), "grabErro"))
        //    {
        //        Application.Current.MainWindow.WindowState = WindowState.Normal;
        //       // SharedObject.Instance.Output(SharedObject.enOutputType.Error, "有一个错误产生", "UI Element Erro!");
        //    }
        //}

        /// <summary>
        /// Initializes a new instance of the DocumentViewModel class.
        /// </summary>
        public DocumentViewModel(string title, string xamlPath, bool isReadOnly = false, EventHandler validationCompleted = null)
        {
            ActivityBuilderDisplayName = title;
            Title = title;
            XamlPath = xamlPath;
            ValidationCompleted = validationCompleted;

            initWorkflowDesigner();

            IsAlwaysReadOnly = isReadOnly;

            IsReadOnly = isReadOnly;

            Messenger.Default.Register<MessengerObjects.CopyData>(this, OnCopyData);

            Messenger.Default.Register<RenameViewModel>(this, "Rename", Rename);

            Messenger.Default.Register<ProjectTreeItem>(this, "Delete", Delete);
        }

        private void OnCopyData(MessengerObjects.CopyData obj)
        {
            //Console.WriteLine(obj.data);
            //string json = Base64.DecodeBase64("utf-8", obj.data);

            //Common.RunInUI(()=> {
            //    ParseCopyDataProc(json);
            //});
        }

        private void Delete(ProjectTreeItem obj)
        {
            //有文件被删除，检查下当前文档对应的xamlPath是否还存在，不存在的话强制关闭即可
            if (!File.Exists(XamlPath))
            {
                Messenger.Default.Send(this, "Close");
                Messenger.Default.Unregister(this);//取消注册
            }
        }

        private void Rename(RenameViewModel obj)
        {
            if (obj.IsDirectory)
            {
                if (XamlPath.ContainsIgnoreCase(obj.Path + @"\"))
                {
                    XamlPath = XamlPath.Replace(obj.Path + @"\", obj.NewPath + @"\");
                    UpdateCompositeTitle();
                }
            }
            else
            {
                if (obj.Path.EqualsIgnoreCase(XamlPath))
                {
                    Title = Path.GetFileNameWithoutExtension(obj.NewPath);
                    XamlPath = obj.NewPath;

                    UpdateCompositeTitle();
                }
            }
        }

        private void initWorkflowDesigner()
        {
            if (_workflowDesignerViewProperty == null)
            {
                WorkflowDesignerInstance = new WorkflowDesigner();

                //修改主界面颜色
                Hashtable hashTable = new Hashtable();
                switch (ViewModelLocator.instance.Main.CurrentThemeName)
                {
                    case "浅色":
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarColorGradientBeginKey, new BrushConverter().ConvertFrom("#F2F2F2") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarColorGradientEndKey, new BrushConverter().ConvertFrom("#F2F2F2") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarSelectedColorGradientBeginKey, new BrushConverter().ConvertFrom("#FFFFFF") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarSelectedColorGradientEndKey, new BrushConverter().ConvertFrom("#FFFFFF") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarHoverColorGradientBeginKey, new BrushConverter().ConvertFrom("#E6E6E6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarHoverColorGradientEndKey, new BrushConverter().ConvertFrom("#E6E6E6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarCaptionActiveColorKey, new BrushConverter().ConvertFrom("#00a3ae") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarCaptionColorKey, new BrushConverter().ConvertFrom("#444444") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarControlBackgroundColorKey, new BrushConverter().ConvertFrom("#f2f2f2") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuSeparatorColorKey, new BrushConverter().ConvertFrom("#e4e4e5") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuItemTextDisabledColorKey, new BrushConverter().ConvertFrom("#b1b1b1") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuItemTextColorKey, new BrushConverter().ConvertFrom("#383838") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuItemTextHoverColorKey, new BrushConverter().ConvertFrom("#393939") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuItemTextSelectedColorKey, new BrushConverter().ConvertFrom("#424242") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuMouseOverBeginColorKey, new BrushConverter().ConvertFrom("#e9e9e9") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuMouseOverEndColorKey, new BrushConverter().ConvertFrom("#e9e9e9") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuMouseOverMiddle2ColorKey, new BrushConverter().ConvertFrom("#e9e9e9") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuMouseOverMiddle1ColorKey, new BrushConverter().ConvertFrom("#e9e9e9") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuMouseOverBorderColorKey, new BrushConverter().ConvertFrom("#e9e9e9") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuBorderColorKey, new BrushConverter().ConvertFrom("#a7abb0") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuBackgroundGradientBeginColorKey, new BrushConverter().ConvertFrom("#ffffff") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuBackgroundGradientEndColorKey, new BrushConverter().ConvertFrom("#ffffff") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuIconAreaColorKey, new BrushConverter().ConvertFrom("#ffffff") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.WorkflowViewElementSelectedBackgroundColorKey, new BrushConverter().ConvertFrom("#a3ced1") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.WorkflowViewElementSelectedBorderColorKey, new BrushConverter().ConvertFrom("#83c5ca") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorSelectedBackgroundBrushKey, new BrushConverter().ConvertFrom("#dedede") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorSelectedForegroundBrushKey, new BrushConverter().ConvertFrom("#000000") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.OutlineViewItemHighlightBackgroundColorKey, new BrushConverter().ConvertFrom("#dedede") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationBackgroundGradientBeginColorKey, new BrushConverter().ConvertFrom("#f6f6f6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationBackgroundGradientMiddleColorKey, new BrushConverter().ConvertFrom("#f6f6f6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationBackgroundGradientEndColorKey, new BrushConverter().ConvertFrom("#f6f6f6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationBorderColorKey, new BrushConverter().ConvertFrom("#d4d4d4") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationDockTextColorKey, new BrushConverter().ConvertFrom("#606060") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationUndockTextColorKey, new BrushConverter().ConvertFrom("#606060") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationDockButtonColorKey, new BrushConverter().ConvertFrom("#666666") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationDockButtonHoverBorderColorKey, new BrushConverter().ConvertFrom("#f6f6f6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationDockButtonHoverBackgroundColorKey, new BrushConverter().ConvertFrom("#f6f6f6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationDockButtonHoverColorKey, new BrushConverter().ConvertFrom("#00a3ae") as SolidColorBrush);

                        break;

                    case "深色（实验性）":
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarColorGradientBeginKey, new BrushConverter().ConvertFrom("#2d2d30") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarColorGradientEndKey, new BrushConverter().ConvertFrom("#2d2d30") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarSelectedColorGradientBeginKey, new BrushConverter().ConvertFrom("#252526") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarSelectedColorGradientEndKey, new BrushConverter().ConvertFrom("#252526") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarHoverColorGradientBeginKey, new BrushConverter().ConvertFrom("#3e3e40") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarHoverColorGradientEndKey, new BrushConverter().ConvertFrom("#3e3e40") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarCaptionActiveColorKey, new BrushConverter().ConvertFrom("#00a3ae") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarCaptionColorKey, new BrushConverter().ConvertFrom("#d0d0d0") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewShellBarControlBackgroundColorKey, new BrushConverter().ConvertFrom("#2d2d30") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuSeparatorColorKey, new BrushConverter().ConvertFrom("#333337") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuItemTextDisabledColorKey, new BrushConverter().ConvertFrom("#656565") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuItemTextColorKey, new BrushConverter().ConvertFrom("#e6e6e6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuItemTextHoverColorKey, new BrushConverter().ConvertFrom("#e6e6e6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuItemTextSelectedColorKey, new BrushConverter().ConvertFrom("#f6f6f6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuMouseOverBeginColorKey, new BrushConverter().ConvertFrom("#333334") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuMouseOverEndColorKey, new BrushConverter().ConvertFrom("#333334") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuMouseOverMiddle2ColorKey, new BrushConverter().ConvertFrom("#333334") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuMouseOverMiddle1ColorKey, new BrushConverter().ConvertFrom("#333334") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuMouseOverBorderColorKey, new BrushConverter().ConvertFrom("#333334") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuBorderColorKey, new BrushConverter().ConvertFrom("#333337") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuBackgroundGradientBeginColorKey, new BrushConverter().ConvertFrom("#1b1b1c") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuBackgroundGradientEndColorKey, new BrushConverter().ConvertFrom("#1b1b1c") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ContextMenuIconAreaColorKey, new BrushConverter().ConvertFrom("#1b1b1c") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.WorkflowViewElementSelectedBackgroundColorKey, new BrushConverter().ConvertFrom("#007a82") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.WorkflowViewElementSelectedBorderColorKey, new BrushConverter().ConvertFrom("#007a82") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorSelectedBackgroundBrushKey, new BrushConverter().ConvertFrom("#4e4e56") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorSelectedForegroundBrushKey, new BrushConverter().ConvertFrom("#e6e6e6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.OutlineViewItemHighlightBackgroundColorKey, new BrushConverter().ConvertFrom("#007a82") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationBackgroundGradientBeginColorKey, new BrushConverter().ConvertFrom("#2d2d30") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationBackgroundGradientMiddleColorKey, new BrushConverter().ConvertFrom("#2d2d30") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationBackgroundGradientEndColorKey, new BrushConverter().ConvertFrom("#2d2d30") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationBorderColorKey, new BrushConverter().ConvertFrom("#3c3c3d") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationDockTextColorKey, new BrushConverter().ConvertFrom("#92a099") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationUndockTextColorKey, new BrushConverter().ConvertFrom("#a0a0a0") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationDockButtonColorKey, new BrushConverter().ConvertFrom("#d0d0d0") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationDockButtonHoverBorderColorKey, new BrushConverter().ConvertFrom("#36363a") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationDockButtonHoverBackgroundColorKey, new BrushConverter().ConvertFrom("#4e4e55") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.AnnotationDockButtonHoverColorKey, new BrushConverter().ConvertFrom("#ffffff") as SolidColorBrush);

                        // 以下为深色（实验性）主题需要额外配置颜色的控件
                        hashTable.Add(WorkflowDesignerColors.DesignerViewBackgroundColorKey, new BrushConverter().ConvertFrom("#252526") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewExpandAllCollapseAllButtonColorKey, new BrushConverter().ConvertFrom("#d9dcde") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewExpandAllCollapseAllButtonMouseOverColorKey, new BrushConverter().ConvertFrom("#f1f1f1") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewExpandAllCollapseAllPressedColorKey, new BrushConverter().ConvertFrom("#f1f1f1") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.DesignerViewStatusBarBackgroundColorKey, new BrushConverter().ConvertFrom("#44444a") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.WorkflowViewElementBackgroundColorKey, new BrushConverter().ConvertFrom("#44444a") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.WorkflowViewElementBorderColorKey, new BrushConverter().ConvertFrom("#44444a") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.WorkflowViewElementCaptionColorKey, new BrushConverter().ConvertFrom("#e6e6e6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.WorkflowViewElementSelectedCaptionColorKey, new BrushConverter().ConvertFrom("#e6e6e6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.ActivityDesignerSelectedTitleForegroundColorKey, new BrushConverter().ConvertFrom("#e6e6e6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.FlowchartConnectorColorKey, new BrushConverter().ConvertFrom("#44444a") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.FlowchartExpressionButtonColorKey, new BrushConverter().ConvertFrom("#ff0000") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.FlowchartExpressionButtonMouseOverColorKey, new BrushConverter().ConvertFrom("#ff0000") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.FlowchartExpressionButtonPressedColorKey, new BrushConverter().ConvertFrom("#ff0000") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorBackgroundBrushKey, new BrushConverter().ConvertFrom("#252526") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorBorderBrushKey, new BrushConverter().ConvertFrom("#44444a") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorTextBrushKey, new BrushConverter().ConvertFrom("#e6e6e6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorToolBarTextBoxBorderBrushKey, new BrushConverter().ConvertFrom("#44444a") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorToolBarSeparatorBrushKey, new BrushConverter().ConvertFrom("#44444a") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorToolBarBackgroundBrushKey, new BrushConverter().ConvertFrom("#44444a") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorToolBarItemSelectedBorderBrushKey, new BrushConverter().ConvertFrom("#c2c2c2") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorToolBarItemSelectedBackgroundBrushKey, new BrushConverter().ConvertFrom("#c2c2c2") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorToolBarItemHoverBorderBrushKey, new BrushConverter().ConvertFrom("#d6d6d6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorToolBarItemHoverBackgroundBrushKey, new BrushConverter().ConvertFrom("#d6d6d6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorPopupBrushKey, new BrushConverter().ConvertFrom("#ff0000") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorPaneBrushKey, new BrushConverter().ConvertFrom("#44444a") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyInspectorCategoryCaptionTextBrushKey, new BrushConverter().ConvertFrom("#e6e6e6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.PropertyToolBarHightlightedButtonForegroundColorKey, new BrushConverter().ConvertFrom("#000000") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.OutlineViewBackgroundColorKey, new BrushConverter().ConvertFrom("#252526") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.OutlineViewItemSelectedTextColorKey, new BrushConverter().ConvertFrom("#e6e6e6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.OutlineViewItemTextColorKey, new BrushConverter().ConvertFrom("#e6e6e6") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.OutlineViewCollapsedArrowBorderColorKey, new BrushConverter().ConvertFrom("#989898") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.OutlineViewCollapsedArrowHoverBorderColorKey, new BrushConverter().ConvertFrom("#00a3ae") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.OutlineViewExpandedArrowColorKey, new BrushConverter().ConvertFrom("#007a82") as SolidColorBrush);
                        hashTable.Add(WorkflowDesignerColors.OutlineViewExpandedArrowBorderColorKey, new BrushConverter().ConvertFrom("#007a82") as SolidColorBrush);

                        break;
                }
                WorkflowDesignerInstance.PropertyInspectorFontAndColorData = XamlServices.Save(hashTable);


                //自定义 活动图标
                foreach (PropertyInfo propertyInfo in typeof(WorkflowDesignerIcons.Activities).GetProperties())
                {
                    DrawingBrush drawingBrush = new DrawingBrush();
                    ImageDrawing drawing = new ImageDrawing();
                    drawing.Rect = new Rect(new Point(0, 0), new Point(32, 32));
                    string iconPath = "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/System/" + propertyInfo.Name + ".png";
                    try
                    {
                        drawing.ImageSource = new BitmapImage(new Uri(iconPath, UriKind.RelativeOrAbsolute));
                    }
                    catch (IOException)
                    {
                        // 不存在的活动图标
                        continue;
                    }
                    drawingBrush.Drawing = drawing;
                    typeof(WorkflowDesignerIcons.Activities).GetProperty(propertyInfo.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SetValue(null, drawingBrush);
                }


                var context = DocumentContext.Create(this);

                initExpressionEditor();

                var designerConfigurationService = WorkflowDesignerInstance.Context.Services.GetService<DesignerConfigurationService>();

                designerConfigurationService.TargetFrameworkName = new System.Runtime.Versioning.FrameworkName(".NETFramework", new Version(4, 5));

                //enable AutoSurroundWithSequence
                designerConfigurationService.AutoSurroundWithSequenceEnabled = true;

                // enable annotations 
                designerConfigurationService.AnnotationEnabled = true;

                WorkflowDesignerInstance.Load(XamlPath);


                var designerView = WorkflowDesignerInstance.Context.Services.GetService<DesignerView>();


                new DesignerViewWrapper(context);

                // 订阅 Document 内 Activity 选择事件
                Selection.Subscribe(designerView.Context, OnActivitySelection);

                var modelService = WorkflowDesignerInstance.Context.Services.GetService<ModelService>();
                modelService.ModelChanged += ModelService_ModelChanged;

                ((FrameworkElement)WorkflowDesignerInstance.View).Loaded += (s, e) =>
                {

                    //每次都会被系统覆盖，这就很扯犊子了。如果有好的方法就别用这种
                    var argumentGrid = VisualTreeHelperEx.FindDescendantByName(designerView, "argumentsDataGrid") as DataGrid;
                   
                    var variablesGrid = VisualTreeHelperEx.FindDescendantByName(designerView, "variableDataGrid") as DataGrid;

                    variablesGrid.RowStyle = argumentGrid.RowStyle = (Style)Application.Current.MainWindow.FindResource("RowStyle");
                    //argumentGrid.CellStyle = (Style)Application.Current.MainWindow.FindResource("GridCellStyle");
                    //variablesGrid.CellStyle = (Style)Application.Current.MainWindow.FindResource("GridCellStyle");

                    designerView.WorkflowShellBarItemVisibility =
                        ShellBarItemVisibility.All;
                };

                SetValidationCompletedEvent();
                _workflowDesignerViewProperty = WorkflowDesignerInstance.View;
            }
        }

        // Document 内 Activity 选择事件
        private void OnActivitySelection(Selection item)
        {
            var modelItem = item.PrimarySelection;
            //if (modelItem.ItemType.IsSubclassOf(typeof(Activity)) 
            //    || modelItem.ItemType.IsSubclassOf(typeof(FlowNode))
            //    || modelItem.ItemType.IsSubclassOf(typeof(StateMachine)))
            //{
            //    // ToDo...
            ViewModelLocator.instance.Dock.CustomWorkflowPropertyView = modelItem;
            //}
        }

        public static ArgumentDirection DirectionFromType(Type argumentType)
        {
            if (typeof(InArgument).IsAssignableFrom(argumentType))
            {
                return ArgumentDirection.In;
            }
            if (typeof(OutArgument).IsAssignableFrom(argumentType))
            {
                return ArgumentDirection.Out;
            }
            if (typeof(InOutArgument).IsAssignableFrom(argumentType))
            {
                return ArgumentDirection.InOut;
            }
            return ArgumentDirection.In;
        }
        private static void AddReferenceArguments(ModelItem currentItem)
        {
            var itemArguments = currentItem.Properties.Where(t => typeof(Argument).IsAssignableFrom(t.PropertyType));
            var variableScopeElement = currentItem.GetVariableScopeElement();
            var variableCollection = variableScopeElement.GetVariableCollection();
            if (variableCollection==null)
            {
                return;
            }
            var ignoreChars = new List<string>
                {".", "=", "<", ">", "^", "*", "/", "\\", "+", "-", "&", "Not ", "And ", "Or ", "Xor ", "Eqv ", "Imp "};
            using (ModelEditingScope modelEditingScope = variableCollection.BeginEdit())
            {
                foreach (var argument in itemArguments)
                {
                    if (argument.Value?.Properties["Expression"]?.Value?.GetCurrentValue() is ITextExpression textExpression &&
                        !ignoreChars.Any(t => textExpression.ExpressionText.Contains(t)) && variableCollection.All(t =>
                            (t.GetCurrentValue() as Variable)?.Name != textExpression.ExpressionText))
                    {
                        var name = textExpression.ExpressionText;
                        Type itemType = argument.Value?.Properties["ArgumentType"]?.Value?.GetCurrentValue() as Type;
                        var variable = Variable.Create(name, itemType, VariableModifiers.None);
                        variableCollection.Add(variable);
                    }
                }

                modelEditingScope.Complete();
            }
        }
        private void UpdateArgumentReferences(LocationReference argument, string oldArgumentName, IEnumerable<ModelItem> activitiesToUpdate)
        {
            //AssertDisposed();
            foreach (ModelItem item in activitiesToUpdate)
            {
                item.WalkRecursive(delegate (ModelItem modelItem)
                {
                    modelItem.RenameArgument(argument, oldArgumentName);
                });
            }
        }

        public void UpdateVariableReferences(Variable variable, string oldVariableName, IEnumerable<ModelItem> activitiesToUpdate)
        {
            foreach (ModelItem item in activitiesToUpdate)
            {
                item.WalkRecursive(delegate (ModelItem modelItem)
                {
                    modelItem.RenameVariable(variable, oldVariableName);
                });
            }
        }

        public void UpdateOutArgumentType(Variable variable, IEnumerable<ModelItem> activitiesToUpdate)
        {
            var modelService = WorkflowDesignerInstance.Context.Services.GetService<ModelService>();
            foreach (ModelProperty item in from p in activitiesToUpdate.SelectMany((ModelItem a) => a.Properties)
                where p.PropertyType == typeof(OutArgument)
                select p)
            {
                ITextExpression textExpression = item?.Value?.Properties["Expression"]?.Value?.GetCurrentValue() as ITextExpression;
                if (textExpression != null && item.Value.IsOutArgument() && textExpression.ExpressionText == variable.Name)
                {
                    using (ModelEditingScope modelEditingScope = modelService.Root.BeginEdit())
                    {
                        Type itemType = item.Value.Properties["Expression"].Value.ItemType;
                        if (itemType.IsGenericType)
                        {
                            Type expressionType = itemType.GetGenericTypeDefinition().MakeGenericType(variable.Type);
                            item.SetValue(variable.CreateArgument(expressionType, ArgumentDirection.Out));
                            modelEditingScope.Complete();
                        }
                    }
                }
            }
        }

        private void SetValidationCompletedEvent()
        {
            var validationService = WorkflowDesignerInstance.Context.Services.GetService<ValidationService>();
            var eventInfo = typeof(ValidationService).GetEvent("ValidationCompleted", BindingFlags.Instance | BindingFlags.NonPublic);
            var addMethod = eventInfo.GetAddMethod(true);
            addMethod.Invoke(validationService, new[] { new EventHandler(InitBreakpointsInfo) });
            if (ValidationCompleted != null)
            {
                addMethod.Invoke(validationService, new[] { ValidationCompleted });
            }
        }

        private void InitBreakpointsInfo(object sender, EventArgs e)
        {
            //TODO WJF 根据保存的断点位置信息自动设置断点,无效的断点需要删除
            var breakpointsDict = ProjectSettingsDataManager.instance.m_projectBreakpointsDataManager.m_breakpointsDict;
            if (breakpointsDict.Count > 0)
            {
                if (breakpointsDict.ContainsKey(RelativeXamlPath))
                {
                    JArray jarr = (JArray)breakpointsDict[RelativeXamlPath].DeepClone();

                    foreach (JToken ji in jarr)
                    {
                        var activityId = ((JObject)ji)["ActivityId"].ToString();
                        var IsEnabled = (bool)(((JObject)ji)["IsEnabled"]);

                        BreakpointsManager.SetBreakpoint(this, activityId, IsEnabled);
                    }
                }
            }

            var validationService = WorkflowDesignerInstance.Context.Services.GetService<ValidationService>();
            validationService.RemoveEventHandler("ValidationCompleted");
        }

        private void initExpressionEditor()
        {
            WorkflowDesignerInstance.Context.Services.Publish<IExpressionEditorService>(new RoslynExpressionEditorService());
        }

        private void ModelService_ModelChanged(object sender, ModelChangedEventArgs e)
        {
            IsDirty = true;
            try
            {
                if (e.ModelChangeInfo != null && e.ModelChangeInfo.Value != null && e.ModelChangeInfo.OldValue == null && e.ModelChangeInfo.ModelChangeType != ModelChangeType.CollectionItemRemoved)
                {
                    //新增activity
                    var currentItem = e.ModelChangeInfo.Value;

                    AddReferenceArguments(currentItem);

                    if (currentItem.ItemType.Name == "FlowStep")
                    {
                        //flowchart设计器拖动时要特殊处理
                        currentItem = e.ModelChangeInfo.Value.Content.Value;
                    }
                    else
                    {

                    }

                    //var assemblyQualifiedName = currentItem.ItemType.AssemblyQualifiedName;//Switch<T>的类型需要特殊判断
                    var assemblyQualifiedName = currentItem.ItemType.Namespace + "." + currentItem.ItemType.Name + "," + currentItem.ItemType.Assembly;
                    assemblyQualifiedName = assemblyQualifiedName.Replace(" ", "");

                    if (!ViewModelLocator.instance.Activities.ActivityTreeItemAssemblyQualifiedNameDic.ContainsKey(assemblyQualifiedName))
                    {
                        assemblyQualifiedName = currentItem.ItemType.AssemblyQualifiedName;
                        assemblyQualifiedName = assemblyQualifiedName.Replace(" ", "");
                    }

                    if (ViewModelLocator.instance.Activities.ActivityTreeItemAssemblyQualifiedNameDic.ContainsKey(assemblyQualifiedName) && !assemblyQualifiedName.Contains("System.Activities.Statements.State"))
                    {
                        var item = ViewModelLocator.instance.Activities.ActivityTreeItemAssemblyQualifiedNameDic[assemblyQualifiedName];
                        Messenger.Default.Send(item, "AddToRecent");
                        if (currentItem.Properties["DisplayName"].Value.ToString().Equals(currentItem.ItemType.GetDisplayName())/*currentItem.View==null/* && !item.IsSystem*/)
                        {
                            currentItem.Properties["DisplayName"].SetValue(item.Name);
                        }
                    }

                    //特殊处理foreach、parallelforeach、/*finalstate*/
                    else
                    {
                        if (assemblyQualifiedName.Contains("ForEach"))
                        {
                            if (assemblyQualifiedName.Contains("ParallelForEach"))
                            {
                                if (currentItem.Properties["DisplayName"].Value.ToString().Equals("ParallelForEach<Object>"))
                                {
                                    currentItem.Properties["DisplayName"].SetValue("并行的遍历循环");
                                }
                            }
                            else
                            {
                                if (currentItem.Properties["DisplayName"].Value.ToString().Equals("ForEach<Object>"))
                                {
                                    currentItem.Properties["DisplayName"].SetValue("遍历循环");
                                }
                            }
                        }
                        else if (assemblyQualifiedName.Contains("State"))
                        {
                            if (currentItem.Properties["DisplayName"].Value.ToString().Equals("State1"))
                            {
                                currentItem.Properties["DisplayName"].SetValue("状态");
                            }
                            else if (currentItem.Properties["DisplayName"].Value.ToString().Equals("FinalState"))
                            {
                                currentItem.Properties["DisplayName"].SetValue("最终状态");
                            }
                        }
                    }
                }

                ModelChangeInfo modelChangeInfo = e.ModelChangeInfo;
                if (modelChangeInfo != null)
                {
                    var modelService = WorkflowDesignerInstance.Context.Services.GetService<ModelService>();
                    if (modelChangeInfo.ModelChangeType == ModelChangeType.PropertyChanged && modelChangeInfo.PropertyName == "Implementation")
                    {
                        //WorkflowInformationHelper.UpdateAttachedInformation(_modelService.Root?.GetCurrentValue() as ActivityBuilder, modelChangeInfo.OldValue);
                    }
                    if (modelChangeInfo.ModelChangeType == ModelChangeType.PropertyChanged && modelChangeInfo.PropertyName == "Name" && modelChangeInfo.Subject == modelService.Root)
                    {
                        ModelProperty newProperty = modelChangeInfo.Subject.Properties[modelChangeInfo.PropertyName];
                        //SanitizeNameProperty(modelChangeInfo.Subject, newProperty, modelChangeInfo.Value?.GetCurrentValue()?.ToString() ?? string.Empty);
                    }

                    if (modelChangeInfo.Subject.GetCurrentValue() is Variable variable)
                    {
                        if (modelChangeInfo.OldValue?.GetCurrentValue() is string text)
                        {
                            ModelItem parentContainer = modelChangeInfo.Subject.GetParentContainer();
                            if (parentContainer != null)
                            {
                                UpdateVariableReferences(variable, text, modelService.GetActivities(parentContainer));
                            }
                        }
                    }
                    if (modelChangeInfo.ModelChangeType == ModelChangeType.PropertyChanged && modelChangeInfo.PropertyName == "Name")
                    {
                        if (modelChangeInfo.Subject.GetCurrentValue() is DynamicActivityProperty dynamicActivityProperty && typeof(Argument).IsAssignableFrom(dynamicActivityProperty.Type))
                        {
                            RuntimeArgument argument = new RuntimeArgument(dynamicActivityProperty.Name, dynamicActivityProperty.Type.GetGenericArguments()[0], DirectionFromType(dynamicActivityProperty.Type));
                            string oldArgumentName = (string)modelChangeInfo.OldValue?.GetCurrentValue();
                            UpdateArgumentReferences(argument, oldArgumentName, modelService.GetActivities(modelService.Root));
                        }
                    }
                    if (modelChangeInfo.ModelChangeType == ModelChangeType.PropertyChanged && modelChangeInfo.PropertyName == "Name")
                    {
                        if (modelChangeInfo.Subject.GetCurrentValue() is DelegateInArgument delegateInArgument)
                        {
                            string oldArgumentName2 = (string)modelChangeInfo.OldValue?.GetCurrentValue();
                            UpdateArgumentReferences(delegateInArgument, oldArgumentName2, modelService.GetActivities(modelChangeInfo.Subject.Parent));
                        }
                    }
                    if (modelChangeInfo.ModelChangeType == ModelChangeType.CollectionItemAdded)
                    {
                        //ItemsAdded(modelChangeInfo.Value);
                        if (modelChangeInfo.Value.GetCurrentValue() is Variable variable2)
                        {
                            List<ModelItem> activities = modelService.GetActivities(modelChangeInfo.Subject?.Parent);
                            UpdateOutArgumentType(variable2, activities);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Logger.Warn(err, logger);
            }

        }
    }
}