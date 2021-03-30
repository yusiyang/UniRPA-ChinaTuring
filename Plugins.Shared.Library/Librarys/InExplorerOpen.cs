using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Plugins.Shared.Library.Librarys
{
    public class InExplorerOpen
    {
        private static ModelProperty currentSelectorModelProperty;
        private static ModelProperty currentSelectorOriginModelProperty;


        /// <summary>
        /// 在 UI 探测器中打开
        /// </summary>
        public static void Execute(ref ModelProperty selectorModelProperty, ref ModelProperty selectorOriginModelProperty)
        {
            currentSelectorModelProperty = selectorModelProperty;
            currentSelectorOriginModelProperty = selectorOriginModelProperty;

            // 用 UI 探测器.exe 打开
            string currentWorkDirectory = Directory.GetCurrentDirectory();
            Process explorerProcess = new Process();
            explorerProcess.StartInfo.FileName = Path.Combine(currentWorkDirectory, "UniExplorer.exe");
            explorerProcess.StartInfo.Arguments = selectorOriginModelProperty.Value.ToString();
            explorerProcess.StartInfo.UseShellExecute = false;  // 必须为false，不然无法在代码中读标准
            explorerProcess.StartInfo.RedirectStandardOutput = true;  // 重定向标准输出
            explorerProcess.EnableRaisingEvents = true;  // 必须为true，这样才会引发 OutputDataReceived 和 Exited

            explorerProcess.OutputDataReceived += OnDataReceived;

            explorerProcess.Start();
            explorerProcess.BeginOutputReadLine();
        }


        /// <summary>
        /// 当收到 UI 探测器数据时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                // 需要从不同的线程更新在UI线程创建的对象，所以将代理放在UI调度程序，委托它到UI线程
                Application.Current.Dispatcher.Invoke((Action)
                delegate
                {

                    SelectorStatusModel selectorStatusModel = SerializeObj.Desrialize(new SelectorStatusModel(), e.Data);
                    var selectorModelProperty = new InArgument<string>(BuildElementSelectorFromSelectorStatusModel(selectorStatusModel));

                    currentSelectorModelProperty.SetValue(selectorModelProperty);
                    currentSelectorOriginModelProperty.SetValue(e.Data);
                });
            }
        }


        /// <summary>
        /// 从选择元素的xml字符串，构建选择器（勾选）状态模型
        /// </summary>
        /// <param name="selectorOriginStr"></param>
        /// <returns></returns>
        public static SelectorStatusModel BuildSelectorStatusModelFromStr(string selectorOriginStr)
        {
            ObservableCollection<SelectorItem> _selectorItems = new ObservableCollection<SelectorItem>();

            string tempSelector = "<Element>" + selectorOriginStr + "</Element>";
            tempSelector.Replace("\'", "\"");
            XmlDocument selector = new XmlDocument();
            selector.LoadXml(tempSelector);
            XmlNode rootNode = selector.SelectSingleNode("Element");
            XmlNodeList selectorItems = rootNode.ChildNodes;

            foreach (XmlNode selectorItem in selectorItems)
            {
                SelectorItem selectoritem = new SelectorItem();
                selectoritem.IsChecked = true;
                selectoritem.ItemContent = selectorItem.OuterXml.ToString();
                selectoritem.ItemContentFull = selectorItem.OuterXml.ToString();

                string itemContentFull = selectoritem.ItemContentFull;
                if (!string.IsNullOrEmpty(itemContentFull))
                {
                    ObservableCollection<SelectorItemAttribute> _selectorItemAttributes = new ObservableCollection<SelectorItemAttribute>();
                    itemContentFull.Replace("\'", "\"");
                    XmlDocument selectorItemFull = new XmlDocument();
                    selectorItemFull.LoadXml(itemContentFull);
                    XmlAttributeCollection attributesFull = selectorItemFull.FirstChild.Attributes;

                    foreach (XmlAttribute attributeFull in attributesFull)
                    {
                        SelectorItemAttribute selectorItemAttribute = new SelectorItemAttribute();
                        selectorItemAttribute.Name = attributeFull.Name;
                        selectorItemAttribute.Value = attributeFull.Value;
                        selectorItemAttribute.IsChecked = true;

                        _selectorItemAttributes.Add(selectorItemAttribute);
                    }

                    selectoritem.Attributes = _selectorItemAttributes;
                }

                selectoritem.IsSelected = false;

                _selectorItems.Add(selectoritem);
            }

            return new SelectorStatusModel(_selectorItems);
        }

        public static string BuildElementSelectorFromSelectorStatusModel(SelectorStatusModel selectorStatusModel)
        {
            string result = "";
            foreach (SelectorItem selectorItem in selectorStatusModel.SelectorItems)
            {
                if ((bool)selectorItem.IsChecked)
                {
                    result += selectorItem.ItemContent;
                }
            }

            return result.Replace("\"", "'");
        }
    }

    [Serializable]
    public class SelectorStatusModel
    {
        public SelectorStatusModel() { }

        public SelectorStatusModel(ObservableCollection<SelectorItem> selectorItems)
        {
            SelectorItems = selectorItems;
        }

        public ObservableCollection<SelectorItem> SelectorItems { get; set; }
    }

    [Serializable]
    public class SelectorItem : INotifyPropertyChanged
    {
        private bool? _isChecked;
        public bool? IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        private string _itemContent;
        public string ItemContent
        {
            get
            {
                return _itemContent;
            }
            set
            {
                _itemContent = value;
                OnPropertyChanged("ItemContent");
            }
        }

        public String ItemContentFull { get; set; }
        public ObservableCollection<SelectorItemAttribute> Attributes { get; set; }

        private bool? _IsSelected;
        public bool? IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                _IsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    [Serializable]
    public class SelectorItemAttribute : INotifyPropertyChanged
    {
        private bool? _isChecked;
        public bool? IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private string _value;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public class VisualTreeItemAttribute : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private string _value;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
