using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Plugins.Shared.Library.Librarys;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace UniExplorer.ViewModel
{
    public class MainDockViewModel : ViewModelBase
    {
        public MainDockViewModel()
        {
            Messenger.Default.Register<VisualTreeItem>(this, "SetTargetElement", SetTargetElement);
        }

        public const string VisualTreeItemsPropertyName = "VisualTreeItems";
        private ObservableCollection<VisualTreeItem> _visualTreeItems = new ObservableCollection<VisualTreeItem>();
        public ObservableCollection<VisualTreeItem> VisualTreeItems
        {
            get
            {
                return _visualTreeItems;
            }
            set
            {
                if (_visualTreeItems == value)
                {
                    return;
                }

                _visualTreeItems = value;
                RaisePropertyChanged(VisualTreeItemsPropertyName);
            }
        }

        public const string VisualTreeItemAttributesPropertyName = "VisualTreeItemAttributes";
        private ObservableCollection<VisualTreeItemAttribute> _visualTreeItemAttributes;
        public ObservableCollection<VisualTreeItemAttribute> VisualTreeItemAttributes
        {
            get
            {
                return _visualTreeItemAttributes;
            }
            set
            {
                if (_visualTreeItemAttributes == value)
                {
                    return;
                }

                _visualTreeItemAttributes = value;
                RaisePropertyChanged(VisualTreeItemAttributesPropertyName);
            }
        }

        private SelectorStatusModel _selectorStatusModel = new SelectorStatusModel(new ObservableCollection<SelectorItem>());
        public SelectorStatusModel SelectorStatusModel
        {
            get
            {
                return _selectorStatusModel;
            }
            set
            {
                _selectorStatusModel = value;
                SelectorItems = new ObservableCollection<SelectorItem>();
                SelectorItemAttributes = new ObservableCollection<SelectorItemAttribute>();
            }
        }


        /// <summary>
        /// 为 Xml 选择器中节点的集合，每一个条目对应一个节点。
        /// </summary>
        public const string SelectorItems_PropertyName = "SelectorItems";
        private ObservableCollection<SelectorItem> _selectorItems = new ObservableCollection<SelectorItem>();
        public ObservableCollection<SelectorItem> SelectorItems
        {
            get
            {
                _selectorItems.CollectionChanged += _selectorItems_CollectionChanged;
                if (_selectorItems.Count == 0)
                {
                    foreach (SelectorItem selectorItem in SelectorStatusModel.SelectorItems)
                    {
                        SelectorItem item = new SelectorItem();
                        item.IsChecked = selectorItem.IsChecked;
                        item.ItemContent = selectorItem.ItemContent;
                        item.ItemContentFull = selectorItem.ItemContentFull;
                        item.IsSelected = selectorItem.IsSelected;
                        _selectorItems.Add(item);
                    }
                }
                return _selectorItems;
            }
            set
            {
                if (_selectorItems == value)
                {
                    return;
                }
                _selectorItems = value;

                RaisePropertyChanged(SelectorItems_PropertyName);
            }
        }


        /// <summary>
        /// 选择器集合中的条目发生变化时进行处理，以便通知 UI 更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _selectorItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Object item in e.NewItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged += SelectorItemPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (Object item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= SelectorItemPropertyChanged;
                }
            }
        }
        private void SelectorItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //This will get called when the property of an object inside the collection changes
            RaisePropertyChanged(SelectorItems_PropertyName);
        }


        /// <summary>
        /// 当前在界面上被选中的节点
        /// </summary>
        public const string SelectedSelectorItem_PropertyName = "SelectedSelectorItem";
        private SelectorItem _selectedSelectorItem = new SelectorItem();
        public SelectorItem SelectedSelectorItem
        {
            get
            {
                return _selectedSelectorItem;
            }
            set
            {
                if (_selectedSelectorItem == value)
                {
                    return;
                }

                _selectedSelectorItem = value;
                RaisePropertyChanged(SelectedSelectorItem_PropertyName);
                RaisePropertyChanged(SelectorItemAttributes_PropertyName);
            }
        }


        /// <summary>
        /// 当前被选中节点的属性集合
        /// </summary>
        public const string SelectorItemAttributes_PropertyName = "SelectorItemAttributes";
        private ObservableCollection<SelectorItemAttribute> _selectorItemAttributes = new ObservableCollection<SelectorItemAttribute>();
        public ObservableCollection<SelectorItemAttribute> SelectorItemAttributes
        {
            get
            {
                _selectorItemAttributes.CollectionChanged += _selectorItemAttributes_CollectionChanged;

                if (SelectedSelectorItem != null)
                {
                    string itemContentFull = SelectedSelectorItem.ItemContentFull;
                    if (!string.IsNullOrEmpty(itemContentFull))
                    {
                        _selectorItemAttributes.Clear();
                        itemContentFull.Replace("\'", "\"");
                        XmlDocument selectorItemFull = new XmlDocument();
                        selectorItemFull.LoadXml(itemContentFull);
                        XmlAttributeCollection attributesFull = selectorItemFull.FirstChild.Attributes;

                        string itemContent = SelectedSelectorItem.ItemContent;
                        if (!string.IsNullOrEmpty(itemContent))
                        {
                            itemContent.Replace("\'", "\"");
                            XmlDocument selectorItem = new XmlDocument();
                            selectorItem.LoadXml(itemContent);
                            XmlAttributeCollection attributes = selectorItem.FirstChild.Attributes;

                            foreach (XmlAttribute attributeFull in attributesFull)
                            {
                                SelectorItemAttribute selectorItemAttribute = new SelectorItemAttribute();
                                selectorItemAttribute.Name = attributeFull.Name;
                                selectorItemAttribute.Value = attributeFull.Value;
                                selectorItemAttribute.IsChecked = false;

                                // 对照左上展示区有没有此条目，有的话，标识未勾选状态
                                foreach (XmlAttribute attribute in attributes)
                                {
                                    if (attribute.Name.Equals(attributeFull.Name))
                                    {
                                        selectorItemAttribute.IsChecked = true;
                                    }
                                }

                                _selectorItemAttributes.Add(selectorItemAttribute);
                            }
                        }
                    }
                }

                return _selectorItemAttributes;
            }
            set
            {
                if (_selectorItemAttributes == value)
                {
                    return;
                }

                _selectorItemAttributes = value;
                RaisePropertyChanged(SelectedSelectorItem_PropertyName);
                RaisePropertyChanged(SelectorItemAttributes_PropertyName);
            }
        }


        /// <summary>
        /// 当前被选中节点的属性集合发生变化时进行处理，以便通知 UI 更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _selectorItemAttributes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Object item in e.NewItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged += SelectedItemPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (Object item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= SelectedItemPropertyChanged;
                }
            }
        }
        private void SelectedItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //This will get called when the property of an object inside the collection changes
            RaisePropertyChanged(SelectedSelectorItem_PropertyName);
        }


        /// <summary>
        /// 当前界面被选中节点的属性集合中 被选中的属性条目
        /// </summary>
        public const string SelectedSelectorItemAttribute_PropertyName = "SelectedSelectorItemAttribute";
        private SelectorItemAttribute _selectedSelectorItemAttribute = new SelectorItemAttribute();
        public SelectorItemAttribute SelectedSelectorItemAttribute
        {
            get
            {
                return _selectedSelectorItemAttribute;
            }
            set
            {
                if (_selectedSelectorItemAttribute == value)
                {
                    return;
                }

                _selectedSelectorItemAttribute = value;
                RaisePropertyChanged(SelectedSelectorItemAttribute_PropertyName);
            }
        }

        /// <summary>
        /// 将选中的节点设置为当前目标元素，填充到右侧 Doc 中进行编辑校验
        /// </summary>
        /// <param name="item"></param>
        private void SetTargetElement(VisualTreeItem item)
        {
            ViewModelLocator.instance.MainDock.SelectorStatusModel = InExplorerOpen.BuildSelectorStatusModelFromStr(item.CurrentUiElement.Selector.ToString());
            ViewModelLocator.instance.Main.ValidateElementIsExist();
        }

    }


    /// <summary>
    /// 用于将xml节点集合转化为字符串进行显示
    /// </summary>
    public class SelectorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retStr = "";
            ObservableCollection<SelectorItem> items = (ObservableCollection<SelectorItem>)value;
            foreach (SelectorItem item in items)
            {
                if ((bool)item.IsChecked)
                {
                    retStr = retStr + item.ItemContent + "\n";
                }
            }
            return retStr;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
