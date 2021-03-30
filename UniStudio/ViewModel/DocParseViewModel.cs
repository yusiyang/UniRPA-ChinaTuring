using ActiproSoftware.Windows.Controls.Wizard;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NPOI.XWPF.UserModel;
using Plugins.Shared.Library.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Uni.Core;
using UniStudio.Librarys;

namespace UniStudio.ViewModel
{
    public class DocParseViewModel : ViewModelBase
    {
        private Window m_view;
        private Wizard m_wizard;
        private ListBox m_listBox;
        private List<ActivityDescription> activityDescriptions;
        private string additionalResourceDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UniStudio", "Cache", "AdditionalResource");
        private string saveXamlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UniStudio", "Cache", "GenerateTargetXaml");


        private RelayCommand<RoutedEventArgs> _loadedCommand;
        public RelayCommand<RoutedEventArgs> LoadedCommand
        {
            get
            {
                return _loadedCommand
                    ?? (_loadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        m_view = (Window)p.Source;

                        // 清空 附件资源缓存 文件夹
                        if (Directory.Exists(additionalResourceDir))
                        {
                            Directory.Delete(additionalResourceDir, true);
                            Directory.CreateDirectory(additionalResourceDir);
                        }
                    }));
            }
        }


        public const string FlowDescribeDocFullPathPropertyName = "FlowDescribeDocFullPath";
        private string _flowDescribeDocFullPathProperty = "";
        public string FlowDescribeDocFullPath
        {
            get
            {
                return _flowDescribeDocFullPathProperty;
            }
            set
            {
                if (_flowDescribeDocFullPathProperty == value)
                {
                    return;
                }

                _flowDescribeDocFullPathProperty = value;
                RaisePropertyChanged(FlowDescribeDocFullPathPropertyName);
            }
        }


        public const string CurrentSelectedAttachmentItemNamePropertyName = "CurrentSelectedAttachmentItemName";
        private string _currentSelectedAttachmentItemNameProperty = "";
        public string CurrentSelectedAttachmentItemName
        {
            get
            {
                return _currentSelectedAttachmentItemNameProperty;
            }
            set
            {
                if (_currentSelectedAttachmentItemNameProperty == value)
                {
                    return;
                }

                _currentSelectedAttachmentItemNameProperty = value;
                RaisePropertyChanged(CurrentSelectedAttachmentItemNamePropertyName);
            }
        }


        public const string CurrentSelectedAttachmentItemPathPropertyName = "CurrentSelectedAttachmentItemPath";
        private string _currentSelectedAttachmentItemPathProperty = "";
        public string CurrentSelectedAttachmentItemPath
        {
            get
            {
                return _currentSelectedAttachmentItemPathProperty;
            }
            set
            {
                if (_currentSelectedAttachmentItemPathProperty == value)
                {
                    return;
                }

                _currentSelectedAttachmentItemPathProperty = value;
                RaisePropertyChanged(CurrentSelectedAttachmentItemPathPropertyName);
            }
        }


        public const string AttachmentItemsPropertyName = "AttachmentItems";
        private ObservableCollection<DocParseAttachmentItem> _attachmentItemsItemsProperty = new ObservableCollection<DocParseAttachmentItem>();
        public ObservableCollection<DocParseAttachmentItem> AttachmentItems
        {
            get
            {
                return _attachmentItemsItemsProperty;
            }

            set
            {
                if (_attachmentItemsItemsProperty == value)
                {
                    return;
                }

                _attachmentItemsItemsProperty = value;
                RaisePropertyChanged(AttachmentItemsPropertyName);
            }
        }


        public const string IsAddAttachmentEnabledPropertyName = "IsAddAttachmentEnabled";
        private bool _isAddAttachmentEnabledProperty = true;
        public bool IsAddAttachmentEnabled
        {
            get
            {
                return _isAddAttachmentEnabledProperty;
            }
            set
            {
                if (_isAddAttachmentEnabledProperty == value)
                {
                    return;
                }

                _isAddAttachmentEnabledProperty = value;
                RaisePropertyChanged(IsAddAttachmentEnabledPropertyName);
            }
        }


        public const string IsRemoveAttachmentEnabledPropertyName = "IsRemoveAttachmentEnabled";
        private bool _isRemoveAttachmentEnabledProperty = false;
        public bool IsRemoveAttachmentEnabled
        {
            get
            {
                return _isRemoveAttachmentEnabledProperty;
            }
            set
            {
                if (_isRemoveAttachmentEnabledProperty == value)
                {
                    return;
                }

                _isRemoveAttachmentEnabledProperty = value;
                RaisePropertyChanged(IsRemoveAttachmentEnabledPropertyName);
            }
        }


        public const string SelectorItemsPropertyName = "SelectorItems";
        private ObservableCollection<DocParseSelectorItem> _selectorItemsItemsProperty = new ObservableCollection<DocParseSelectorItem>();
        public ObservableCollection<DocParseSelectorItem> SelectorItems
        {
            get
            {
                return _selectorItemsItemsProperty;
            }

            set
            {
                if (_selectorItemsItemsProperty == value)
                {
                    return;
                }

                _selectorItemsItemsProperty = value;
                RaisePropertyChanged(SelectorItemsPropertyName);
            }
        }


        public const string IsBusyLoadingPropertyName = "IsBusyLoading";
        private bool _isBusyLoadingProperty = false;
        public bool IsBusyLoading
        {
            get
            {
                return _isBusyLoadingProperty;
            }
            set
            {
                if (_isBusyLoadingProperty == value)
                {
                    return;
                }

                _isBusyLoadingProperty = value;
                RaisePropertyChanged(IsBusyLoadingPropertyName);
            }
        }




        private RelayCommand<RoutedEventArgs> _wizardLoadedCommand;
        public RelayCommand<RoutedEventArgs> WizardLoadedCommand
        {
            get
            {
                return _wizardLoadedCommand
                    ?? (_wizardLoadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        m_wizard = (Wizard)p.Source;
                    }));
            }
        }


        private RelayCommand _selectDocCommand;
        public RelayCommand SelectDocCommand
        {
            get
            {
                return _selectDocCommand
                    ?? (_selectDocCommand = new RelayCommand(
                    () =>
                    {
                        ViewModelLocator.instance.Start.m_view.IsEnabled = false;
                        var fileFullPath = Common.ShowSelectSingleFileDialog("Word 流程描述文档|*.docx", "选择流程描述文档（Word）");

                        //延迟调用，避免双击选择文件时误触发后面的消息
                        Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                        {
                            ViewModelLocator.instance.Start.m_view.IsEnabled = true;
                        }), DispatcherPriority.ContextIdle);

                        if (!string.IsNullOrEmpty(fileFullPath))
                        {
                            FlowDescribeDocFullPath = fileFullPath;
                            var temp = ViewModelLocator.instance.DocParse.FlowDescribeDocFullPath;
                        }
                        else
                        {
                            UniMessageBox.Show(App.Current.MainWindow, "选择待解析的文档出错。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }));
            }
        }


        private RelayCommand<RoutedEventArgs> _listBoxLoadedCommand;
        public RelayCommand<RoutedEventArgs> ListBoxLoadedCommand
        {
            get
            {
                return _listBoxLoadedCommand
                    ?? (_listBoxLoadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        m_listBox = (ListBox)p.Source;
                    }));
            }
        }


        private RelayCommand _addAttachmentCommand;
        public RelayCommand AddAttachmentCommand
        {
            get
            {
                return _addAttachmentCommand
                    ?? (_addAttachmentCommand = new RelayCommand(
                    () =>
                    {
                        ResetSelectToDefault();

                        ViewModelLocator.instance.Start.m_view.IsEnabled = false;
                        var fileFullPath = Common.ShowSelectSingleFileDialog();

                        //延迟调用，避免双击选择文件时误触发后面的消息
                        Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                        {
                            ViewModelLocator.instance.Start.m_view.IsEnabled = true;
                        }), DispatcherPriority.ContextIdle);

                        if (!string.IsNullOrEmpty(fileFullPath))
                        {
                            var fileName = Path.GetFileName(fileFullPath);

                            if (!AttachmentFullPathValid(fileName))
                            {
                                UniMessageBox.Show(m_view, "已添加过同名的附件资源，请选择其他文件。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            // 添加到附件列表
                            AttachmentItems.Add(new DocParseAttachmentItem(this)
                            {
                                Name = fileName,
                                Path = fileFullPath
                            });

                            // 拷贝附件文件到指定缓存目录
                            if (!Directory.Exists(additionalResourceDir))
                            {
                                Directory.CreateDirectory(additionalResourceDir);
                            }
                            var destFullPath = Path.Combine(additionalResourceDir, fileName);
                            File.Copy(fileFullPath, destFullPath, true);

                            // 滚动条滚动到最后面
                            m_listBox.ScrollIntoView(AttachmentItems.LastOrDefault());

                            ResetSelectToDefault();
                        }
                        else
                        {
                            UniMessageBox.Show(App.Current.MainWindow, "选择附件文件出错。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }));
            }
        }


        private RelayCommand _removeAttachmentCommand;
        public RelayCommand RemoveAttachmentCommand
        {
            get
            {
                return _removeAttachmentCommand
                    ?? (_removeAttachmentCommand = new RelayCommand(
                    () =>
                    {
                        DocParseAttachmentItem selectItem = null;
                        foreach (DocParseAttachmentItem item in AttachmentItems)
                        {
                            if (item.Path.ToLower().Equals(CurrentSelectedAttachmentItemPath.ToLower()))
                            {
                                selectItem = item;
                                break;
                            }
                        }

                        AttachmentItems.Remove(selectItem);

                        // 删除拷贝到附件指定缓存目录的文件
                        File.Delete(Path.Combine(additionalResourceDir, selectItem.Name));

                        RefreshAttachmentList();

                        ResetSelectToDefault();
                    }));
            }
        }


        private RelayCommand _selectedPageChangingCommand;
        public RelayCommand SelectedPageChangingCommand
        {
            get
            {
                return _selectedPageChangingCommand
                    ?? (_selectedPageChangingCommand = new RelayCommand(
                    () =>
                    {

                    }));
            }
        }


        private RelayCommand _selectedPageChangedCommand;
        public RelayCommand SelectedPageChangedCommand
        {
            get
            {
                return _selectedPageChangedCommand
                    ?? (_selectedPageChangedCommand = new RelayCommand(
                    () =>
                    {
                        IsBusyLoading = false;
                    }));
            }
        }


        private RelayCommand _backButtonOnClickCommand;
        public RelayCommand BackButtonOnClickCommand
        {
            get
            {
                return _backButtonOnClickCommand
                    ?? (_backButtonOnClickCommand = new RelayCommand(
                    () =>
                    {
                        m_wizard.BacktrackToPreviousPage();
                    }));
            }
        }


        private RelayCommand _nextButtonOnClickCommand;
        public RelayCommand NextButtonOnClickCommand
        {
            get
            {
                return _nextButtonOnClickCommand
                    ?? (_nextButtonOnClickCommand = new RelayCommand(
                    () =>
                    {
                        // 检测请求数据完整性
                        if (string.IsNullOrEmpty(FlowDescribeDocFullPath))
                        {
                            UniMessageBox.Show("尚未选择流程描述文档，请选择流程文档后继续。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // 解析流程描述文档
                        StringBuilder flowDocContentBuilder = ParseWordDoc();

                        // 请求后端接口
                        IsBusyLoading = true;
                        Task.Run(() =>
                        {
                            RequestSelectorList(flowDocContentBuilder.ToString());
                        });
                    }));
            }
        }


        /// <summary>
        /// 解析流程描述文档
        /// </summary>
        /// <param name="flowDocContentBuilder"></param>
        private StringBuilder ParseWordDoc()
        {
            StringBuilder flowDocContentBuilder = new StringBuilder();
            try
            {
                using (FileStream stream = File.OpenRead(FlowDescribeDocFullPath))
                {
                    XWPFDocument doc = new XWPFDocument(stream);
                    if (doc.Tables.Count == 0)
                    {
                        UniMessageBox.Show("选择的流程描述文档不规范。不存在指令表格。请重新选择流程描述文档。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        throw new Exception();
                    }
                    foreach (XWPFTable table in doc.Tables)
                    {
                        if (table.NumberOfRows <= 1)
                        {
                            UniMessageBox.Show("选择的流程描述文档不规范，无指令内容。请重新选择流程描述文档。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            throw new Exception("选择的流程描述文档不规范，无指令内容");
                        }
                        int currentRow = 0;
                        while (++currentRow < table.NumberOfRows)
                        {
                            XWPFTableRow tableRow = table.GetRow(currentRow);
                            XWPFTableCell tableCell = tableRow.GetCell(2);
                            foreach (XWPFParagraph activityRow in tableCell.Paragraphs)
                            {
                                if (activityRow.ParagraphText.Substring(activityRow.ParagraphText.Length - 1).Equals("："))
                                {
                                    flowDocContentBuilder.Append(activityRow.ParagraphText.Substring(0, activityRow.ParagraphText.Length - 1) + "；");
                                }
                                else
                                {
                                    flowDocContentBuilder.Append(activityRow.ParagraphText);
                                }
                            }
                        }
                    }
                }
                return flowDocContentBuilder;
            }
            catch (IOException e)
            {
                UniMessageBox.Show("选择的流程描述文档正由另一进程使用，因此当前进程无法访问此文件。请关闭占用程序或重新选择流程描述文档。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                FlowDescribeDocFullPath = "";
                throw;
            }
            catch (NullReferenceException e)
            {
                UniMessageBox.Show("选择的流程描述文档不规范，缺少“Steps and Rules”列的指令内容。请重新选择流程描述文档。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                FlowDescribeDocFullPath = "";
                throw;
            }
            catch (ICSharpCode.SharpZipLib.Zip.ZipException e)
            {
                UniMessageBox.Show("选择的流程描述文档不规范，文档为空。请重新选择流程描述文档。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                FlowDescribeDocFullPath = "";
                throw;
            }
            catch (Exception e)
            {
                UniMessageBox.Show("选择的流程描述文档不规范。请重新选择流程描述文档。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                FlowDescribeDocFullPath = "";
                throw;
            }
        }


        /// <summary>
        /// 请求后端接口，获取待补录的 Selector 列表
        /// </summary>
        private void RequestSelectorList(string flowDocContentStr)
        {
            GenerateWorkflowManager generateWorkflowManager = GenerateWorkflowManager.Instance;
            activityDescriptions = generateWorkflowManager.InputText(flowDocContentStr, out string error);

            List<DocParseSelectorItem> toDoSelectorItems = new List<DocParseSelectorItem>();
            ParseActivityDescriptionsResult(activityDescriptions, ref toDoSelectorItems);
            SelectorItems = new ObservableCollection<DocParseSelectorItem>(toDoSelectorItems);

            // 请求成功
            Common.RunInUI(() =>
            {
                m_wizard.GoToNextPage();
            });
        }


        /// <summary>
        /// 解析得到的活动描述结果
        /// </summary>
        /// <param name="activityDescriptions"></param>
        /// <param name="toDoSelectorItems"></param>
        private void ParseActivityDescriptionsResult(List<ActivityDescription> activityDescriptions, ref List<DocParseSelectorItem> toDoSelectorItems)
        {
            foreach (ActivityDescription activityDescriptionItem in activityDescriptions)
            {
                if (activityDescriptionItem.AllParameters != null && activityDescriptionItem.IsHasSelector)
                {
                    foreach (string param in activityDescriptionItem.AllParameters)
                    {
                        toDoSelectorItems.Add(new DocParseSelectorItem(this)
                        {
                            Param = param,
                            OriginActivityDescription = activityDescriptionItem
                        });
                    }
                }
                else if (activityDescriptionItem.Properties.ContainsKey("Body"))
                {
                    ParseActivityDescriptionsResult(activityDescriptionItem.Properties["Body"] as List<ActivityDescription>, ref toDoSelectorItems);
                }
            }
        }


        private RelayCommand _finishButtonOnClickCommand;
        public RelayCommand FinishButtonOnClickCommand
        {
            get
            {
                return _finishButtonOnClickCommand
                    ?? (_finishButtonOnClickCommand = new RelayCommand(
                    () =>
                    {
                        // 请求后端接口
                        IsBusyLoading = true;
                        Task.Run(() =>
                        {
                            RequestGenerateXamlFlow();
                        });
                    }));
            }
        }


        /// <summary>
        /// 请求生成 Xaml 流程文件
        /// </summary>
        private void RequestGenerateXamlFlow()
        {
            // 重构 ActivityDescriptions

            foreach (DocParseSelectorItem selectorItem in SelectorItems)
            {
                if (!string.IsNullOrEmpty(selectorItem.Selector))
                {
                    RebuildActivityDescriptions(selectorItem, ref activityDescriptions);
                }
            }

            GenerateWorkflowManager generateWorkflowManager = GenerateWorkflowManager.Instance;
            generateWorkflowManager.GenerateXaml(activityDescriptions, additionalResourceDir, saveXamlPath);

            // 请求成功
            Common.RunInUI(() =>
            {
                m_view.Close();

                // 将生成的 Xaml 流程文件拷贝到当前项目，并打开
                string sourceXamlFileName = Path.Combine(saveXamlPath, ActivityXamlManager.SaveXamlDefaultName);
                string destXamlFileName = Path.Combine(Path.GetDirectoryName(ViewModelLocator.instance.Project.CurrentProjectJsonFile), ActivityXamlManager.SaveXamlDefaultName);
                destXamlFileName = Path.Combine(Path.GetDirectoryName(destXamlFileName), Common.GetValidFileName(Path.GetDirectoryName(destXamlFileName), Path.GetFileName(destXamlFileName), "", "{0}", 1));
                File.Copy(sourceXamlFileName, destXamlFileName);

                ViewModelLocator.instance.Project.RefreshCommand.Execute(null);
                if (!string.IsNullOrEmpty(destXamlFileName))
                {
                    var item = ViewModelLocator.instance.Project.GetProjectTreeItemByFullPath(destXamlFileName);
                    if (item != null)
                    {
                        item.OpenXamlCommand.Execute(null);
                    }
                }
            });
        }


        /// <summary>
        /// 重构 ActivityDescriptions
        /// </summary>
        private void RebuildActivityDescriptions(DocParseSelectorItem selectorItem, ref List<ActivityDescription> activityDescriptions)
        {
            try
            {
                int index = activityDescriptions.IndexOf(selectorItem.OriginActivityDescription);
                if (index >= 0)
                {
                    activityDescriptions[index].Properties.Remove("Selector");
                    activityDescriptions[index].Properties.Add("Selector", selectorItem.Selector);
                    activityDescriptions[index].Properties.Remove("SourceImgPath");
                    activityDescriptions[index].Properties.Add("SourceImgPath", selectorItem.ScreenshotFileName);
                    activityDescriptions[index].Properties.Remove("visibility");
                    activityDescriptions[index].Properties.Add("visibility", Visibility.Visible);
                }
                else
                {
                    for (int i = 0; i < activityDescriptions.Count; i++)
                    {
                        if (activityDescriptions[i].Properties != null && activityDescriptions[i].Properties.ContainsKey("Body"))
                        {
                            List<ActivityDescription> subActivityDescritptions = activityDescriptions[i].Properties["Body"] as List<ActivityDescription>;
                            RebuildActivityDescriptions(selectorItem, ref subActivityDescritptions);
                            activityDescriptions[i].Properties["Body"] = subActivityDescritptions;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }


        private void ResursionControlFlowActivity()
        {

        }


        private RelayCommand _mouseLeftButtonDownCommand;
        public RelayCommand MouseLeftButtonDownCommand
        {
            get
            {
                return _mouseLeftButtonDownCommand
                    ?? (_mouseLeftButtonDownCommand = new RelayCommand(
                    () =>
                    {
                        ResetSelectToDefault();
                    }));
            }
        }


        /// <summary>
        /// 重置 ListBox 状态为未选择
        /// </summary>
        private void ResetSelectToDefault()
        {
            AttachmentItemsUnselectAll();

            CurrentSelectedAttachmentItemName = "";
            CurrentSelectedAttachmentItemPath = "";

            IsAddAttachmentEnabled = true;
            IsRemoveAttachmentEnabled = false;
        }


        /// <summary>
        /// 刷新附件列表
        /// </summary>
        private void RefreshAttachmentList()
        {
            var cacheAttachmentItems = new List<DocParseAttachmentItem>(AttachmentItems);
            AttachmentItems.Clear();
            foreach (var item in cacheAttachmentItems)
            {
                AttachmentItems.Add(item);
            }
        }


        private void AttachmentItemsUnselectAll()
        {
            foreach (var item in AttachmentItems)
            {
                if (item.IsSelected)
                {
                    item.IsSelected = false;
                }
            }
        }


        /// <summary>
        /// 验证是否已经添加过同名附件文件
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <returns></returns>
        private bool AttachmentFullPathValid(string fileName)
        {
            foreach (DocParseAttachmentItem item in AttachmentItems)
            {
                if (item.Name.ToLower().Equals(fileName.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }

    }
}
