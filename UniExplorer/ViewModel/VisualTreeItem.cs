using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Plugins.Shared.Library.Librarys;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml;
using UniExplorer.Librarys;

namespace UniExplorer.ViewModel
{
    public class VisualTreeItem : ViewModelBase
    {
        public UiElement CurrentUiElement { get; set; }

        public VisualTreeItem Parent { get; set; }

        public const string ChildrenPropertyName = "Children";
        private ObservableCollection<VisualTreeItem> _children = new ObservableCollection<VisualTreeItem>();
        public ObservableCollection<VisualTreeItem> Children
        {
            get
            {
                return _children;
            }
            set
            {
                if (_children == value)
                {
                    return;
                }

                _children = value;
                RaisePropertyChanged(ChildrenPropertyName);
            }
        }

        public const string IsExpandedPropertyName = "IsExpanded";
        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (_isExpanded == value)
                {
                    return;
                }

                _isExpanded = value;
                RaisePropertyChanged(IsExpandedPropertyName);
            }
        }

        public const string IsSelectedPropertyName = "IsSelected";
        private bool _isSelected = false;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                RaisePropertyChanged(IsSelectedPropertyName);
            }
        }

        public const string NamePropertyName = "Name";
        private string _name = "";
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                RaisePropertyChanged(NamePropertyName);
            }
        }

        public const string ToolTipPropertyName = "ToolTip";
        private string _toolTip = null;
        public string ToolTip
        {
            get
            {
                if (string.IsNullOrEmpty(_toolTip))
                {
                    _toolTip = Name;
                }

                return _toolTip;
            }
            set
            {
                if (_toolTip == value)
                {
                    return;
                }

                _toolTip = value;
                RaisePropertyChanged(ToolTipPropertyName);
            }
        }

        public const string IconPropertyName = "Icon";
        private string _icon = "";
        public string Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                if (_icon == value)
                {
                    return;
                }

                _icon = value;
                RaisePropertyChanged(IconPropertyName);
            }
        }


        private RelayCommand<MouseButtonEventArgs> _treeNodeMouseLeftButtonDownCommand;
        public RelayCommand<MouseButtonEventArgs> TreeNodeMouseLeftButtonDownCommand
        {
            get
            {
                return _treeNodeMouseLeftButtonDownCommand
                    ?? (_treeNodeMouseLeftButtonDownCommand = new RelayCommand<MouseButtonEventArgs>(
                    p =>
                    {
                        TreeViewItem treeViewItem = UIUtils.VisualUpwardSearch<TreeViewItem>(p.OriginalSource as DependencyObject) as TreeViewItem;
                        if (treeViewItem != null)
                        {
                            treeViewItem.Focus();
                            BuildAndShowAttributeManager(treeViewItem.Header as VisualTreeItem);
                        }
                    }));
            }
        }

        private RelayCommand<MouseButtonEventArgs> _treeNodeMouseRightButtonDownCommand;
        public RelayCommand<MouseButtonEventArgs> TreeNodeMouseRightButtonDownCommand
        {
            get
            {
                return _treeNodeMouseRightButtonDownCommand
                    ?? (_treeNodeMouseRightButtonDownCommand = new RelayCommand<MouseButtonEventArgs>(
                    p =>
                    {
                        TreeViewItem treeViewItem = UIUtils.VisualUpwardSearch<TreeViewItem>(p.OriginalSource as DependencyObject) as TreeViewItem;
                        if (treeViewItem != null)
                        {
                            treeViewItem.Focus();
                            BuildAndShowAttributeManager(treeViewItem.Header as VisualTreeItem);
                        }
                    }));
            }
        }

        private RelayCommand<MouseButtonEventArgs> _treeNodeMouseRightButtonUpCommand;
        public RelayCommand<MouseButtonEventArgs> TreeNodeMouseRightButtonUpCommand
        {
            get
            {
                return _treeNodeMouseRightButtonUpCommand
                    ?? (_treeNodeMouseRightButtonUpCommand = new RelayCommand<MouseButtonEventArgs>(
                    p =>
                    {
                        // 控件右击动态弹出菜单
                        var view = App.Current.MainWindow;
                        ContextMenu cm = view.FindResource("VisualTreeContextMenu") as ContextMenu;
                        cm.DataContext = this;
                        cm.Placement = PlacementMode.MousePoint;
                        cm.IsOpen = true;
                    }));
            }
        }


        private RelayCommand _setTargetElementCommand;
        public RelayCommand SetTargetElementCommand
        {
            get
            {
                return _setTargetElementCommand
                    ?? (_setTargetElementCommand = new RelayCommand(
                    () =>
                    {
                        Messenger.Default.Send(this, "SetTargetElement");
                    }));
            }
        }

        /// <summary>
        /// 构建并在“属性资源管理器”中展示当前选中的节点的属性集合
        /// </summary>
        /// <param name="treeViewItem"></param>
        public static void BuildAndShowAttributeManager(VisualTreeItem visualTreeItem)
        {
            ObservableCollection<VisualTreeItemAttribute> visualTreeItemAttributes = new ObservableCollection<VisualTreeItemAttribute>();

            string selector = visualTreeItem.CurrentUiElement.Selector;
            string tempSelector = "<Element>" + selector + "</Element>";
            tempSelector.Replace("\'", "\"");
            XmlDocument selectorDoc = new XmlDocument();
            selectorDoc.LoadXml(tempSelector);
            XmlNode rootNode = selectorDoc.SelectSingleNode("Element");
            XmlNodeList selectorItems = rootNode.ChildNodes;
            foreach (XmlNode selectorItem in selectorItems)
            {
                string tempAttributes = selectorItem.OuterXml.ToString();
                tempAttributes.Replace("\'", "\"");
                XmlDocument attributesDoc = new XmlDocument();
                attributesDoc.LoadXml(tempAttributes);
                XmlAttributeCollection attributesCollection = attributesDoc.FirstChild.Attributes;
                foreach (XmlAttribute attribute in attributesCollection)
                {
                    VisualTreeItemAttribute visualTreeItemAttribute = new VisualTreeItemAttribute()
                    {
                        Name = attribute.Name,
                        Value = attribute.Value
                    };
                    visualTreeItemAttributes.Add(visualTreeItemAttribute);
                }
            }

            ViewModelLocator.instance.MainDock.VisualTreeItemAttributes = visualTreeItemAttributes;
        }

    }
}
