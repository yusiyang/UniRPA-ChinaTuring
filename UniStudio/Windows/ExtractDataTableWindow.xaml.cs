using ActiproSoftware.Windows.Controls.Ribbon.Controls;
using ActiproSoftware.Windows.Controls.Wizard;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using System.Xml.Serialization;
using Plugins.Shared.Library.Librarys;
using TextActivity;
using UniStudio.ViewModel;
using static Plugins.Shared.Library.UiAutomation.UiElement;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Data;
using Plugins.Shared.Library.UiAutomation.DataExtract;
using Plugins.Shared.Library.Extensions;

// ReSharper disable DelegateSubtraction

namespace UniStudio.Windows
{
    /// <summary>
    /// ExtractDataTableWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExtractDataTableWindow
    {
        private int _extractRelatedDataCount;

        private string _currentPage;

        private UiElement _currentElement;//当前选中的元素
        private readonly UiElement[] _currentPair = new UiElement[3];//当前轮次选中的元素
        private readonly List<UiElement[]> _pairList = new List<UiElement[]>();//所有轮次选中的元素
        private ExtractData _data;//当前显示的数据
        private readonly ExtractInput _input = new ExtractInput();

        private ExtractTable _extractTableOpt;
        private string _optStr;
        private int _maxRows;

        private bool _selecting;
        private bool _extralSelecting;

        private static IDataExtract dataExtract;

        public ExtractDataTableWindow()
        {
            InitializeComponent();
            _currentPage = "_page0";
            OnSelected += UiElement_OnSelected;
            OnCancel += UiElement_OnCanceled;
        }

        /// <summary>
        /// 预览数据
        /// </summary>
        private void Display()
        {
            DataGrid.Columns.Clear();
            DataGrid.ItemsSource = null;
            for (var i = 0; i < _data.Rows[0].Columns.Count; i++)
            {
                var columnName = _data.Rows[0].Columns[i].Name;
                DataGrid.Columns.Add(new DataGridTextColumn()
                {
                    Header = columnName,
                    Binding = new Binding($"Columns[{i}].Value")
                });
            }

            DataGrid.ItemsSource = _data.Rows;
        }

        public DataTable ExtractResult(ExtractData data = null)
        {
            var ret = data ?? _data;
            DataTable dt = new DataTable();
            foreach (var column in ret.Rows[0].Columns)
            {
                var columnName = column.Name;
                dt.Columns.Add(new DataColumn()
                {
                    ColumnName = columnName,
                    Caption = columnName
                });
            }

            foreach (RowData row in ret.Rows)
            {
                DataRow dr = dt.NewRow();
                foreach (var column in row.Columns)
                {
                    dr[column.Name] = column.Value;
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }

        #region 选择元素
        /// <summary>
        /// 开始选择元素
        /// </summary>
        private void WaitElement()
        {
            this.WindowState = WindowState.Minimized;
            _selecting = true;
            StartElementHighlight();
        }
        private void UiElement_OnSelected(UiElement uiElement)
        {
            _selecting = false;
            _currentElement = uiElement;
            this.WindowState = WindowState.Normal;
            this.Show();
            var pageName = _currentPage;

            if (!IsHtmlElement(_currentElement))
            {
                if (pageName == "_page3")
                {
                    _extralSelecting = false;
                }
                UniMessageBox.Show("该控件不支持数据提取。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            switch (pageName)
            {
                case "_page0":
                    // 选中的是 Table 类型元素
                    var tableEle = GetParentTable(_currentElement);
                    if (tableEle != null &&
                        UniMessageBox.Show("您已选择了一个表格单元，是否从整个表格中提取数据？", "提取表", MessageBoxButton.YesNo, MessageBoxImage.Question).Equals(MessageBoxResult.Yes))
                    {
                        // 按照 Table 类型元素处理
                        ExtractRelatedDataButton.Visibility = Visibility.Collapsed;

                        _pairList.Add(new[]
                        {
                            _currentElement,_currentElement,tableEle
                        });
                        _currentPair[0] = _currentElement;
                        _currentPair[1] = _currentElement;
                        _currentPair[2] = tableEle;

                        _wizard.GoToPage(_page3);
                        break;
                    }
                    //List元素处理方式
                    if (IsListElement(_currentElement))
                    {
                        _currentPair[0] = _currentElement;
                        ExtractRelatedDataButton.Visibility = Visibility.Visible;
                    }
                    _wizard.GoToNextPage();
                    break;
                case "_page1":
                    if (_currentElement.Role != _currentPair[0].Role)
                    {
                        UniMessageBox.Show("1. 无法找到模式，元素因标签而异",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                    var commonParent = GetCommonParent(_currentPair[0], _currentElement);
                    if (commonParent == null)
                    {
                        UniMessageBox.Show("2. 请知名与您指明用于定义第一列的字段相关的字段",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                    var option = GetOption(_currentPair[0], _currentElement, out var err);
                    if (option?.Columns == null)
                    {
                        _input.Columns.RemoveAt(_input.Columns.Count - 1);
                        UniMessageBox.Show(err);
                        return;
                    }
                    SetAttribute(commonParent, "data-uni-top-container", "");
                    SetAttribute(commonParent.Parent, "data-uni-top-container", "");
                    SetAttribute(commonParent.Parent, "data-uni-common-parent", "");

                    option = GetOption(_currentPair[0], _currentElement, out err);
                    _currentPair[1] = _currentElement;
                    _currentPair[2] = commonParent;
                    _pairList.Add(new[]
                    {
                        _currentPair[0],_currentPair[1],commonParent
                    });

                    var extractOption = XmlSerialize(option);
                    var result = dataExtract.GetColumnData(_currentPair[2], extractOption);
                    _data = DeSerializer<ExtractData>(result);
                    //跳转到配置页时高亮
                    dataExtract.ColorColumn(_currentPair[0], _input.Columns.Last().Name);

                    _wizard.GoToNextPage();
                    break;
                case "_page2":
                    _wizard.GoToNextPage();
                    break;
                case "_page3":
                    _currentPair[0] = _currentElement;
                    _wizard.GoToPage(_page1);
                    break;
                default:
                    _wizard.GoToNextPage();
                    break;
            }
        }
        private void UiElement_OnCanceled()
        {
            OnSelected -= NextPageButtonSelected;
            this.WindowState = WindowState.Normal;
            this.Show();
            var pageName = _currentPage;
            if (pageName == "_page3")
            {
                _extralSelecting = false;
            }
            _selecting = false;
            _currentElement = null;
        }
        #endregion

        #region 私有
        private UiElement GetCommonParent(UiElement element1, UiElement element2)
        {
            if (element1 == null || element2 == null)
            {
                return null;
            }
            if (dataExtract.Compare(element1, element2))
            {
                return null;
            }
            if (element1.Parent == null || element2.Parent == null)
            {
                return null;
            }

            if (element1.Parent.Role.Equals("body", StringComparison.OrdinalIgnoreCase) || element2.Parent.Role.Equals("body", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            if (dataExtract.Compare(element1.Parent, element2.Parent))
            {
                return element1.Parent;
            }
            return GetCommonParent(element1.Parent, element2.Parent);
        }
        private bool IsListElement(UiElement uiElement)
        {
            return true;
        }

        private bool IsHtmlElement(UiElement uiElement)
        {
            if (uiElement.ControlType == "HtmlNode")
            {
                dataExtract = new ChromeExtract();
                return true;
            }

            if (uiElement.ControlType == "IEHtmlNode")
            {
                dataExtract = new IeExtract();
                return true;
            }

            return false;
        }

        private string GetTableData()
        {
            _extractTableOpt = new ExtractTable();
            _optStr = XmlSerialize(_extractTableOpt);
            var tableEle = _currentPair[2];
            return dataExtract.GetColumnData(tableEle, _optStr);
        }
        /// <summary>
        /// 获取元素父table
        /// </summary>
        /// <param name="uiElement"></param>
        /// <returns></returns>
        private UiElement GetParentTable(UiElement uiElement)
        {
            if (uiElement.Parent == null)
            {
                return null;
            }
            return uiElement.IsTable() ? uiElement : GetParentTable(uiElement.Parent);
        }
        private ExtractData GetOption(UiElement ele1, UiElement ele2, out string err)
        {
            var uniId1 = ele1.GetAttributeValue("TEXT_CAPTURE_X_CUSTOM_ID").ToString();
            if (string.IsNullOrEmpty(uniId1))
            {
                uniId1 = $"{DateTime.Now:MM/dd/yyyy HH:mm:ss}-1";
                if (_input.Columns.Any())
                {
                    uniId1 = $"{DateTime.Now:MM/dd/yyyy HH:mm:ss}-101";
                }
                SetAttribute(ele1, "TEXT_CAPTURE_X_CUSTOM_ID", uniId1);
            }

            var uniId2 = ele2.GetAttributeValue("TEXT_CAPTURE_X_CUSTOM_ID").ToString();
            if (string.IsNullOrEmpty(uniId2))
            {
                uniId2 = $"{DateTime.Now:MM/dd/yyyy HH:mm:ss}-2";
                if (_input.Columns.Any())
                {
                    uniId2 = $"{DateTime.Now:MM/dd/yyyy HH:mm:ss}-102";
                }
                SetAttribute(ele2, "TEXT_CAPTURE_X_CUSTOM_ID", uniId2);
            }

            if (_input.Columns.All(t => t.UniId1 != uniId1 && t.UniId2 != uniId2))
            {
                _input.Columns.Add(new InputColumn
                {
                    Attr = "text",
                    Name = $"Column{_input.Columns.Count + 1}",
                    UniId1 = uniId1,
                    UniId2 = uniId2
                });
                TextName.Text = _input.Columns.Last().Name;
            }

            _input.Columns.Last().Name = TextName.Text;
            var input = XmlSerialize(_input);

            var extractOption = dataExtract.GetSameColumn(ele1, input);
            err = extractOption;
            try
            {
                var option = DeSerializer<ExtractData>(extractOption);
                option.AddResAttr = "1";
                return option;
            }
            catch (Exception e)
            {
                return null;
            }

        }

        public static bool ShowEdit(ref string strText)
        {
            var strTemp = string.Empty;

            var edit = new EditWindow
            {
                TextHandler = str => { strTemp = str; },
                TextBox =
                {
                    Text = strText
                }
            };
            var result = edit.ShowDialog() ?? false;
            strText = strTemp;

            return result;
        }
        #endregion

        private void CreateActivity(UiElement nextButtonEle = null)
        {
            var designer = ViewModelLocator.instance.Dock.ActiveDocument.WorkflowDesignerInstance;
            var designerView = designer.Context.Services.GetService<DesignerView>();
            var modelService = designer.Context.Services.GetService<ModelService>();
            var modelRoot = modelService.Root;
            int.TryParse(MaxResultsTextBox.Text, out _maxRows);


            Activity activity = new ExtractDataActivity
            {
                //SourceImgPath = nextButtonEle?.CaptureInformativeScreenshotToFile(),
                Selector = new InArgument<string>(_currentPair[2].Selector),
                SelectorOrigin = SerializeObj.Serialize(InExplorerOpen.BuildSelectorStatusModelFromStr(_currentPair[2].Selector)),
                ExtractMetadata = _optStr,
                MaxNumber = _maxRows,
                NextSelector = new InArgument<string>(nextButtonEle?.Selector)
            };
            if (Librarys.Common.IsWorkflowDesignerEmpty(designer))
            {
                activity = Librarys.Common.ProcessAutoSurroundWithSequence(activity);
                var item = ModelFactory.CreateItem(modelRoot.GetEditingContext(), activity);
                //designerView.MakeRootDesigner(item);
                modelService.Root.Content?.SetValue(activity);
            }
            else
            {
                var selectedModelItem = modelService.Find(modelRoot, typeof(Sequence)).First();

                using (var modelEditingScope = selectedModelItem.BeginEdit())
                {
                    var modelItemCollection = selectedModelItem.Properties["Activities"]?.Collection;
                    modelItemCollection?.Add(activity);
                    modelEditingScope.Complete();
                }
            }
        }

        #region 事件
        private void OnSelectedPageChanging(object sender, WizardSelectedPageChangeEventArgs e)
        {
            switch (e.NewSelectedPage?.Name)
            {
                case "_page3" when e.OldSelectedPage?.Name == "_page0":
                    {
                        var result = GetTableData();
                        _data = DeSerializer<ExtractData>(result);
                        Display();
                        return;
                    }
                case "_page0" when e.OldSelectedPage?.Name == "_page3":
                    {
                        Array.Clear(_currentPair, 0, 3);
                        _pairList.Clear();
                        _currentElement = null;
                        return;
                    }
                case "_page3":
                    {
                        if (e.OldSelectedPage?.Name == "_page1")
                        {
                            _currentPair[0] = _pairList.LastOrDefault()?[0];
                            _currentPair[1] = _pairList.LastOrDefault()?[1];
                            _currentPair[2] = _pairList.LastOrDefault()?[2];
                        }
                        SetAttribute(_currentPair[2], "data-uni-top-container", "");
                        SetAttribute(_currentPair[2].Parent, "data-uni-top-container", "");
                        SetAttribute(_currentPair[2].Parent, "data-uni-common-parent", "");

                        var option = GetOption(_currentPair[0], _currentElement, out _);
                        var extractOption = XmlSerialize(option);
                        _optStr = extractOption;
                        var ret = dataExtract.GetColumnData(_currentPair[2], extractOption);
                        _data = DeSerializer<ExtractData>(ret);
                        Display();
                        break;
                    }
                case "_page2":
                    {
                        TextName.Text = _input.Columns.Last().Name;
                        UrlName.Text = "Column" + (_input.Columns.Count + 1);
                        TextCheck.IsChecked = true;
                        UrlCheck.IsChecked = false;
                        break;
                    }
                default:
                    break;
            }
        }
        private void OnSelectedPageChanged(object sender, WizardSelectedPageChangeEventArgs e)
        {
            _currentPage = e.NewSelectedPage.Name;
        }
        private void OnExtractRelatedDataButtonClick(object sender, ExecuteRoutedEventArgs e)
        {
            _extractRelatedDataCount++;
            _extralSelecting = true;
            _data = null;
            WaitElement();
        }
        private void NextButtonOnClick(object sender, RoutedEventArgs e)
        {
            var pageName = _wizard.SelectedPage.Name;
            switch (pageName)
            {
                case "_page0":
                    WaitElement();
                    return;
                case "_page1":
                    WaitElement();
                    break;
                case "_page2":
                    dataExtract.StepTo(_currentPair[0]);
                    _wizard.GoToNextPage();
                    break;
                case "_page3":
                    break;
                default:
                    break;
            }
        }

        private void BackButtonOnClick(object sender, RoutedEventArgs e)
        {
            var pageName = _wizard.SelectedPage.Name;
            switch (pageName)
            {
                case "_page0"://选第一个元素
                    break;
                case "_page1"://选第二个元素
                    _currentElement = _currentPair[0];
                    _currentPair[0] = null;
                    break;
                case "_page2"://配置页
                    dataExtract.StepTo(_currentPair[0]);
                    _data = null;
                    _currentElement = _currentPair[1];
                    _currentPair[1] = null;
                    _currentPair[2] = null;
                    _pairList.RemoveAt(_pairList.Count - 1);
                    _input.Columns.RemoveAt(_input.Columns.Count - 1);
                    break;
                case "_page3"://预览页
                    dataExtract.StepTo(_currentPair[0]);
                    _data = null;
                    break;
                default:
                    break;
            }
            _wizard.BacktrackToPreviousPage();
        }
        private void _wizard_OnFinish(object sender, RoutedEventArgs e)
        {
            if (UniMessageBox.Show("是否跨页面提取,指定翻页按钮以自动翻页？", "跨页面", MessageBoxButton.YesNo, MessageBoxImage.Question).Equals(MessageBoxResult.Yes))
            {
                OnSelected += NextPageButtonSelected;
                WaitElement();
            }
            else
            {
                CreateActivity();
            }

        }

        private void NextPageButtonSelected(UiElement element)
        {
            OnSelected -= NextPageButtonSelected;
            if (!IsHtmlElement(element))
            {
                UniMessageBox.Show("所选元素非页面元素");
                CreateActivity();
                ClearAll();
                return;
            }
            CreateActivity(element);
            ClearAll();
        }

        private void _wizard_OnCancel(object sender, RoutedEventArgs e)
        {
            //_currentElement = null;
            //Array.Clear(_currentPair, 0, 3);
            //_pairList.Clear();
            //_input.Columns.Clear();
            //_data = null;
            //_extractRelatedDataCount = 0;
        }

        protected override void OnClosed(EventArgs e)
        {
            if (!_selecting)
            {
                ClearAll();
            }
            Application.Current.MainWindow?.Show();
            OnSelected -= UiElement_OnSelected;
            OnCancel -= UiElement_OnCanceled;
            base.OnClosed(e);
        }

        private void ClearAll()
        {
            _currentElement = null;
            Array.Clear(_currentPair, 0, 3);
            _pairList.Clear();
            _input.Columns.Clear();
            _data = null;
            _extractRelatedDataCount = 0;
        }

        private void EditButton_OnClick(object sender, ExecuteRoutedEventArgs e)
        {
            var option = _optStr;
            if (!ShowEdit(ref option)) return;
            if (option == _optStr) return;

            _optStr = option;
            var ret = dataExtract.GetColumnData(_currentPair[2], option);
            _data = DeSerializer<ExtractData>(ret);
            Display();
        }
        #endregion

        #region Xml序列化
        /// <summary>
        /// 将实体对象转换成XML
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="obj">实体对象</param>
        private static string XmlSerialize<T>(T obj)
        {
            try
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    NewLineChars = "\r\n",
                    Encoding = Encoding.UTF8,
                    OmitXmlDeclaration = true// 不生成声明头
                };
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
                using (var sw = new StringWriter())
                {
                    using (var xmlWriter = XmlWriter.Create(output: sw, settings))
                    {
                        Type t = obj.GetType();
                        var serializer = new XmlSerializer(obj.GetType());
                        serializer.Serialize(xmlWriter, obj, namespaces);
                    }
                    return sw.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("将实体对象转换成XML异常", ex);
            }
        }

        /// <summary>
        /// 将XML转换成实体对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="strXML">XML</param>
        private static T DeSerializer<T>(string strXML) where T : class
        {
            try
            {
                using (StringReader sr = new StringReader(strXML))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(sr) as T;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("将XML转换成实体对象异常", ex);
            }
        }
        #endregion

        #region Ext

        public static void SetAttribute(UiElement element, string attrName, string attrValue)
        {
            dataExtract.SetAttribute(element, attrName, attrValue);
        }

        #endregion
    }
}
