using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.Librarys;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UniExplorer.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private const string ITEM_CODE_ICON = "pack://application:,,,/Resource/Image/Dock/item-code.png";
        private const string ITEM_WINDOW_ICON = "pack://application:,,,/Resource/Image/Dock/item-window.png";
        private const string ITEM_CONTROL_ICON = "pack://application:,,,/Resource/Image/Dock/item-control.png";

        private UiElement currentUiElement = null;

        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private readonly string prefixTitle = "UI 探测器";
        private string _titleProperty = "";

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {
                if (_titleProperty == "")
                {
                    return prefixTitle;
                }
                else
                {
                    return prefixTitle + @" - " + _titleProperty;
                }

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

        public const string IsBuildLoadingPropertyName = "IsBuildLoading";
        private bool _isBuildLoading = false;
        public bool IsBuildLoading
        {
            get
            {
                return _isBuildLoading;
            }

            set
            {
                if (_isBuildLoading == value)
                {
                    return;
                }

                _isBuildLoading = value;
                RaisePropertyChanged(IsBuildLoadingPropertyName);
            }
        }

        public const string OutputDataControlIsVisibilyPropertyName = "OutputDataControlIsVisibily";
        private Visibility _outputDataControlIsVisibily = Visibility.Hidden;
        public Visibility OutputDataControlIsVisibily
        {
            get
            {
                return _outputDataControlIsVisibily;
            }
            set
            {
                _outputDataControlIsVisibily = value;
                RaisePropertyChanged(OutputDataControlIsVisibilyPropertyName);
            }
        }

        public const string ValidateElementBackgroundPropertyName = "ValidateElementBackground";
        private SolidColorBrush _validateElementBackground = new BrushConverter().ConvertFrom("#f4b800") as SolidColorBrush;
        public SolidColorBrush ValidateElementBackground
        {
            get
            {
                return _validateElementBackground;
            }
            set
            {
                _validateElementBackground = value;
                RaisePropertyChanged(ValidateElementBackgroundPropertyName);
            }
        }

        public const string ValidateElementStatusImagePropertyName = "ValidateElementStatusImage";
        private BitmapImage _validateElementStatusImage = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/Windows/validate-unknown.png"));
        public BitmapImage ValidateElementStatusImage
        {
            get
            {
                return _validateElementStatusImage;
            }
            set
            {
                _validateElementStatusImage = value;
                RaisePropertyChanged(ValidateElementStatusImagePropertyName);
            }
        }

        public const string ValidateElementStatusToolTipPropertyName = "ValidateElementStatusToolTip";
        private string _validateElementStatusToolTip = "选取器已修改，请重新验证。";
        public string ValidateElementStatusToolTip
        {
            get
            {
                return _validateElementStatusToolTip;
            }
            set
            {
                _validateElementStatusToolTip = value;
                RaisePropertyChanged(ValidateElementStatusToolTipPropertyName);
            }
        }

        public const string HighlightElementIsEnabledPropertyName = "HighlightElementIsEnabled";
        private bool _highlightElementIsEnabled = false;
        public bool HighlightElementIsEnabled
        {
            get
            {
                return _highlightElementIsEnabled;
            }
            set
            {
                _highlightElementIsEnabled = value;
                RaisePropertyChanged(HighlightElementIsEnabledPropertyName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
        }

        private RelayCommand _validateElementCommand;
        public RelayCommand ValidateElementCommand
        {
            get
            {
                return _validateElementCommand
                    ?? (_validateElementCommand = new RelayCommand(
                    () =>
                    {
                        ValidateElementIsExist();
                    }));
            }
        }

        private RelayCommand _startPickElementCommand;
        public RelayCommand StartPickElementCommand
        {
            get
            {
                return _startPickElementCommand
                    ?? (_startPickElementCommand = new RelayCommand(
                    () =>
                    {
                        UiElement.OnSelected = UiElement_OnSelected;
                        UiElement.StartElementHighlight();
                    }));
            }
        }

        private RelayCommand _highlightElementCommand;
        public RelayCommand HighlightElementCommand
        {
            get
            {
                return _highlightElementCommand
                    ?? (_highlightElementCommand = new RelayCommand(
                    () =>
                    {
                        if (currentUiElement != null)
                        {
                            // 将窗口置前
                            currentUiElement.SetForeground();
                            // 高亮一个元素
                            currentUiElement.DrawHighlight();
                        }
                        else
                        {
                            UniMessageBox.Show("未找到元素。修复选取器，或选取一个新元素。");
                        }
                    }));
            }
        }

        private RelayCommand _saveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand
                    ?? (_saveCommand = new RelayCommand(
                        () =>
                        {
                            string data = SerializeObj.Serialize(new SelectorStatusModel(ViewModelLocator.instance.MainDock.SelectorItems));

                            Process currentProcess = Process.GetCurrentProcess();
                            StreamWriter writer = new StreamWriter(Console.OpenStandardOutput(), Encoding.GetEncoding("GBK"));
                            writer.WriteLine(data);
                            writer.Flush();

                            Application.Current.MainWindow.Close();
                        }));
            }
        }

        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(
                        () =>
                        {
                            Application.Current.MainWindow.Close();
                        }));
            }
        }

        private void UiElement_OnSelected(UiElement uiElement)
        {
            ViewModelLocator.instance.MainDock.SelectorStatusModel = InExplorerOpen.BuildSelectorStatusModelFromStr(uiElement.Selector.ToString());
            Application.Current.MainWindow.Activate();
            ValidateElementIsExist();
        }

        public void ValidateElementIsExist()
        {
            Task.Run(() =>
            {
                // 打开加载动画
                IsBuildLoading = true;

                // 需要从不同的线程更新在UI线程创建的对象，所以将代理放在UI调度程序，委托它到UI线程
                Application.Current.Dispatcher.Invoke((Action)
                delegate
                {
                    // 验证中……
                    ValidateElementBackground = new BrushConverter().ConvertFrom("#b3b3b3") as SolidColorBrush;
                    ValidateElementStatusImage = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/Windows/validate-doing.gif"));
                    HighlightElementIsEnabled = false;
                });

                string selStr = InExplorerOpen.BuildElementSelectorFromSelectorStatusModel(new SelectorStatusModel(ViewModelLocator.instance.MainDock.SelectorItems));
                if (string.IsNullOrEmpty(selStr))
                {
                    // 需要从不同的线程更新在UI线程创建的对象，所以将代理放在UI调度程序，委托它到UI线程
                    Application.Current.Dispatcher.Invoke((Action)
                    delegate
                    {
                        // 验证为空
                        ValidateElementBackground = new BrushConverter().ConvertFrom("#b3b3b3") as SolidColorBrush;
                        ValidateElementStatusImage = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/Windows/validate-null.png"));
                        ValidateElementStatusToolTip = "当前选择器为空。在屏幕上选取另一个元素，或填充选取器编辑框。";
                        HighlightElementIsEnabled = false;
                    });
                }
                else
                {
                    currentUiElement = UiElement.FromSelector(selStr, 3000);
                    if (currentUiElement != null)
                    {
                        // 验证成功 生成可视化树
                        BuildVisualTree();

                        // 需要从不同的线程更新在UI线程创建的对象，所以将代理放在UI调度程序，委托它到UI线程
                        Application.Current.Dispatcher.Invoke((Action)
                        delegate
                        {
                            // 验证成功
                            ValidateElementBackground = new BrushConverter().ConvertFrom("#008001") as SolidColorBrush;
                            ValidateElementStatusImage = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/Windows/validate-succeed.png"));
                            ValidateElementStatusToolTip = "当前选取器有效。选择高亮显示，以查看该选取器的元素。";
                            HighlightElementIsEnabled = true;
                        });
                    }
                    else
                    {
                        // 需要从不同的线程更新在UI线程创建的对象，所以将代理放在UI调度程序，委托它到UI线程
                        Application.Current.Dispatcher.Invoke((Action)
                        delegate
                        {
                            // 验证失败
                            ValidateElementBackground = new BrushConverter().ConvertFrom("#cb0000") as SolidColorBrush;
                            ValidateElementStatusImage = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/Windows/validate-failed.png"));
                            ValidateElementStatusToolTip = "当前选取器无效。修复选取器，或选取一个新元素。";
                            HighlightElementIsEnabled = false;
                        });
                    }
                }

                // 关闭加载动画
                IsBuildLoading = false;
            });
        }

        private void BuildVisualTree()
        {
            VisualTreeItem parentItem;
            if (currentUiElement.Parent != null)
            {
                // 当前节点的父亲节点，包含当前节点及兄弟节点的所有子节点
                parentItem = BuildVisualTreeUiElementChildren(currentUiElement.Parent, true);

                // 追溯到根节点
                parentItem = BuildVisualTreeUiElementRoot(currentUiElement.Parent, parentItem);
            }
            else
            {
                parentItem = BuildVisualTreeUiElementChildren(currentUiElement, false);

                // 追溯到根节点
                parentItem = BuildVisualTreeUiElementRoot(currentUiElement, parentItem);
            }

            ObservableCollection<VisualTreeItem> visualTreeItems = new ObservableCollection<VisualTreeItem>();
            visualTreeItems.Add(parentItem);
            ViewModelLocator.instance.MainDock.VisualTreeItems = visualTreeItems;
        }

        private VisualTreeItem BuildVisualTreeUiElementChildren(UiElement uiElement, bool isExpanded)
        {
            VisualTreeItem currentItem = new VisualTreeItem();

            if (!string.IsNullOrEmpty(uiElement.Name))
            {
                currentItem.Name = uiElement.Name;
            }
            else if (!string.IsNullOrEmpty(uiElement.Role))
            {
                currentItem.Name = uiElement.Role;
            }
            else if (!string.IsNullOrEmpty(uiElement.ProcessName))
            {
                currentItem.Name = uiElement.ProcessName;
            }
            else if (!string.IsNullOrEmpty(uiElement.ClassName))
            {
                currentItem.Name = uiElement.ClassName;
            }

            currentItem.CurrentUiElement = uiElement;
            currentItem.IsExpanded = isExpanded;
            if (uiElement.Selector.Equals(currentUiElement.Selector))
            {
                currentItem.IsSelected = true;
                VisualTreeItem.BuildAndShowAttributeManager(currentItem);
            }

            try
            {
                if (uiElement.Children.Count > 0)
                {
                    currentItem.Icon = ITEM_WINDOW_ICON;

                    foreach (UiElement element in uiElement.Children)
                    {
                        currentItem.Children.Add(BuildVisualTreeUiElementChildren(element, false));
                    }
                }
                else
                {
                    currentItem.Icon = ITEM_CONTROL_ICON;
                }
            }
            catch
            {
                currentItem.Icon = ITEM_CONTROL_ICON;
            }

            return currentItem;
        }

        private VisualTreeItem BuildVisualTreeUiElementRoot(UiElement currentUiElement, VisualTreeItem currentItem)
        {
            // 特别处理 SAP 的情况（SAP 节点 不存在父节点）
            if (currentUiElement.IsSAPUiNode)
            {
                currentItem.Name = "SAP Node";
                currentItem.Icon = ITEM_CODE_ICON;
                return currentItem;
            }

            while (currentUiElement.Parent != null)
            {
                UiElement parentUiElement = currentUiElement.Parent;

                VisualTreeItem parentItem = new VisualTreeItem();

                if (!string.IsNullOrEmpty(parentUiElement.Name))
                {
                    parentItem.Name = parentUiElement.Name;
                }
                else if (!string.IsNullOrEmpty(parentUiElement.Role))
                {
                    parentItem.Name = parentUiElement.Role;
                }
                else if (!string.IsNullOrEmpty(parentUiElement.ProcessFullPath))
                {
                    parentItem.Name = parentUiElement.ProcessName;
                }
                else if (!string.IsNullOrEmpty(parentUiElement.ClassName))
                {
                    parentItem.Name = parentUiElement.ClassName;
                }

                parentItem.CurrentUiElement = parentUiElement;
                parentItem.IsExpanded = true;
                parentItem.Icon = ITEM_WINDOW_ICON;
                parentItem.Children.Add(currentItem);

                currentItem = parentItem;
                currentUiElement = parentUiElement;
            }

            // 根节点特别处理
            currentItem.Name = "桌面";
            currentItem.Icon = ITEM_CODE_ICON;
            return currentItem;
        }
    }
}