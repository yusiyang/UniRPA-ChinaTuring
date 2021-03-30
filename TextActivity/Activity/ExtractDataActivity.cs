using MouseActivity;
using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;
using System.Data;
using System.Windows;
using Plugins.Shared.Library.UiAutomation;
using Plugins.Shared.Library;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;
using Plugins.Shared.Library.UiAutomation.Browser;
using Plugins.Shared.Library.UiAutomation.DataExtract;
using Plugins.Shared.Library.Exceptions;

namespace TextActivity
{
    [Designer(typeof(ExtractDataDesigner))]
    public sealed class ExtractDataActivity : CodeActivity
    {
        #region 属性分类：常见

        public string _displayName;
        [Category("常见")]
        [DisplayName("显示名称")]
        public new string DisplayName
        {
            get
            {
                if (_displayName == null)
                {
                    _displayName = base.DisplayName;
                }
                else
                {
                    base.DisplayName = _displayName;
                }

                return _displayName;
            }
            set
            {
                _displayName = value;
            }
        }
        [Browsable(false)]
        public string _DisplayName
        {
            get
            {
                return this.DisplayName;
            }
        }

        [Category("常见")]
        [DisplayName("出错时继续")]
        [Description("指定即使在当前活动失败的情况下，仍继续执行剩余的活动。仅支持布尔值（True,False）。")]
        public bool ContinueOnError { get; set; }

        [Category("常见")]
        [DisplayName("在此之前延迟")]
        [Description("活动开始执行任何操作之前的延迟时间（以毫秒为单位）。默认时间量为200毫秒。")]
        public InArgument<int> DelayBefore { get; set; }

        [Category("常见")]
        [DisplayName("在此之后延迟")]
        [Description("执行活动之后的延迟时间（以毫秒为单位）。默认时间量为300毫秒。")]
        public InArgument<int> DelayAfter { get; set; }

        #endregion


        #region 属性分类：目标

        [Category("目标")]
        [RequiredArgument]
        [OverloadGroup("G1")]
        [DisplayName("选取器")]
        [Description("用于在执行活动时查找特定用户界面元素的“文本”属性。它实际上是XML片段，用于指定您要查找的图形用户界面元素及其一些父元素的属性。必须将文本放入引号中。")]
        public InArgument<string> Selector { get; set; }
        [Browsable(false)]
        public string SelectorOrigin { get; set; }

        [Category("目标")]
        [DisplayName("超时")]
        [Description("每页提取时判断操作超时的时间")]
        public InArgument<Int32> TimeOut
        {
            get
            {
                return _timeOut;
            }
            set
            {
                if (_timeOut == value) return;
                _timeOut = value;
            }
        }
        private InArgument<Int32> _timeOut = 30000;
        [Category("目标")]
        [RequiredArgument]
        [OverloadGroup("G2")]
        [DisplayName("元素")]
        [Description("使用另一个活动返回的用户界面元素变量， 该属性不能与“选取器”属性一起使用。该字段仅支持用户界面元素变量。")]
        public InArgument<UIElement> ActiveWindow { get; set; }

        #endregion


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("提取元数据")]
        [Description("XML字符串，使您能够定义从指定网页中提取的数据。必须将文本放入引号中。")]
        public InArgument<string> ExtractMetadata { get; set; }
        [Category("输出")]
        [DisplayName("数据表")]
        [Description("从指定网页中提取的信息。该字段仅支持数据表变量。")]
        public OutArgument<DataTable> DataTable { get; set; }
        #endregion


        #region 属性分类：选项

        private InArgument<string> _NextSelector;
        [Category("选项")]
        [DisplayName("下一个链接选取器")]
        [Description("识别导航至下一页所用链接/按钮的选取器。应相对于“现有用户界面元素”属性。必须将文本放入引号中。")]
        public InArgument<string> NextSelector
        {
            get
            {
                return _NextSelector;
            }
            set
            {
                _NextSelector = value;
            }
        }

        [Category("选项")]
        [DisplayName("发送窗口消息")]
        [Description("如果选中，则通过向目标应用程序发送一条特定消息的方式执行点击。这种输入方法可在后台工作，且兼容大多数桌面应用程序，但并不是速度最快的方法。默认情况下，该复选框是未选中状态。如果既未选中该复选框，也未选中“模拟点击”复选框，则默认方法通过使用硬件驱动程序模拟点击。默认方法速度最慢，且不能在后台工作，但可兼容所有桌面应用程序。")]
        public bool SendMessage { get; set; }

        private InArgument<Int32> _MaxNumber = 100;
        [Category("选项")]
        [DisplayName("最大结果数")]
        [Description("待提取的最大结果数。如果值为0， 则将所有以确定的元素添加至输出。默认值为 100。")]
        public InArgument<Int32> MaxNumber
        {
            get
            {
                return _MaxNumber;
            }
            set
            {
                if (_MaxNumber == value) return;
                _MaxNumber = value;
            }
        }

        private bool _SimulateClick = true;
        [Category("选项")]
        [DisplayName("模拟单击")]
        [Description("如果选中，则通过使用目标应用程序的技术模拟点击。这种输入方法是三种方法中最快的，且可在后台工作。默认情况下， 该复选框是未选中状态。如果既未选中该复选框，也未选中“发送窗口消息”复选框，则默认方法通过使用硬件驱动程序执行点击。默认方法速度最慢，且不能在后台工作，但可兼容所有桌面应用程序。")]
        public bool SimulateClick
        {
            get
            {
                return _SimulateClick;
            }
            set
            {
                _SimulateClick = value;
            }
        }

        private InArgument<Int32> _DelayPage = 300;
        [Category("选项")]
        [DisplayName("页面之间延迟（毫秒）")]
        [Description("加载下一页之前的等待时间量（以毫秒为单位）。如果页面加载时间较长，则该值较高。")]
        public InArgument<Int32> DelayPage
        {
            get
            {
                return _DelayPage;
            }
            set
            {
                if (_DelayPage == value) return;
                _DelayPage = value;
            }
        }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string SourceImgPath { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/ClickText/extract.png";
            }
        }

        private System.Windows.Visibility visi = System.Windows.Visibility.Hidden;
        [Browsable(false)]
        public System.Windows.Visibility visibility
        {
            get
            {
                return visi;
            }
            set
            {
                visi = value;
            }
        }


        [Browsable(false)]
        public string DefaultName { get { return "提取结构化数据"; } }

        #endregion

        #region Model


        [XmlRoot("extract")]
        public class ExtractData
        {
            [XmlElement("row")]
            public List<RowData> Rows { get; set; }
            [XmlAttribute("add_res_attr")]
            public string AddResAttr { get; set; }
            [XmlElement("column")]
            public List<ColumnData> Columns { get; set; }
        }
        public class RowData
        {
            [XmlElement("column")]
            public List<ColumnData> Columns { get; set; }
            [XmlElement("webctrl")]
            public List<WebCtrl> WebCtrl { get; set; }
            [XmlAttribute("exact")]
            public string Exact { get; set; }
        }

        public class ColumnData
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
            [XmlAttribute("exact")]
            public string Exact { get; set; }
            [XmlAttribute("attr")]
            public string Attr { get; set; }
            [XmlText]
            public string Value { get; set; }
            [XmlElement("webctrl")]
            public List<WebCtrl> WebCtrl { get; set; }
        }

        public class WebCtrl
        {
            [XmlAttribute("tag")]
            public string Tag { get; set; }
            [XmlAttribute("idx")]
            public string Index { get; set; }
            [XmlAttribute("class")]
            public string Class { get; set; }
            [XmlAttribute("text")]
            public string Text { get; set; }
        }

        [XmlRoot("meta-input")]
        public class ExtractInput
        {
            public ExtractInput()
            {
                Columns = new List<InputColumn>();
            }
            [XmlElement("column")]
            public List<InputColumn> Columns { get; set; }
        }

        public class InputColumn
        {
            [XmlAttribute("uniid1")]
            public string UniId1 { get; set; }
            [XmlAttribute("uniid2")]
            public string UniId2 { get; set; }
            [XmlAttribute("name")]
            public string Name { get; set; }
            [XmlAttribute("attr")]
            public string Attr { get; set; }
        }
        #endregion

        public DataTable ExtractResult(ExtractData data = null)
        {
            var ret = data;
            DataTable dt = new DataTable();
            for (var i = 0; i < ret.Rows[0].Columns.Count; i++)
            {
                var columnName = ret.Rows[0].Columns[i].Name;
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

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            int betweenTime = Common.GetValueOrDefault(context, this._DelayPage, 300);
            int timeOut = Common.GetValueOrDefault(context, this.TimeOut, 30000);
            Thread.Sleep(delayBefore);

            try
            {
                DataTable result = null;
                UiElement nextButton = null;
                var maxResult = MaxNumber.Get(context);
                do
                {
                    string extractMetadata = ExtractMetadata.Get(context);
                    string selector = Selector.Get(context);
                    string data = "";
                    IBrowser browser = null;
                    Application.Current.Dispatcher?.Invoke(() =>
                    {
                        IDataExtract dataExtract = null;
                        UiElement tableElement = UiElement.FromSelector(selector, timeOut);
                        if (tableElement==null)
                        {
                            throw new NotImplementedException("查找不到元素");
                        }
                        if (tableElement.ControlType == "HtmlNode")
                        {
                            dataExtract = new ChromeExtract();
                        }
                        else if (tableElement.ControlType == "IEHtmlNode")
                        {
                            dataExtract = new IeExtract();
                        }
                        browser = dataExtract?.GetBrowser(tableElement);
                        data = dataExtract?.GetColumnData(tableElement, extractMetadata);
                    });
                    ExtractData extractData = DeSerializer<ExtractData>(data);
                    var dataTable = ExtractResult(extractData);
                    if (result == null)
                    {
                        result = dataTable.Copy();
                    }
                    else
                    {
                        result.Merge(dataTable);
                    }

                    string nextSelector = NextSelector.Get(context);
                    if (!string.IsNullOrEmpty(nextSelector))
                    {
                        nextButton = UiElement.FromSelector(nextSelector, timeOut);
                    }

                    if (nextButton!=null)
                    {
                        nextButton?.MouseClick(new UiElementClickParams { mouseActionType = MouseActionType.Simulate });
                        browser?.WaitPage(timeOut);
                        Thread.Sleep(betweenTime);
                    }
                    
                    if (!string.IsNullOrEmpty(nextSelector))
                    {
                        nextButton = UiElement.FromSelector(nextSelector, timeOut);
                    }
                } while (nextButton != null && (result.Rows.Count < maxResult || maxResult <= 0));

                maxResult = MaxNumber.Get(context) > result.Rows.Count ? -1 : MaxNumber.Get(context);
                if (maxResult > 0)
                {
                    var newTable = result.Copy();
                    newTable.Rows.Clear();
                    for (int i = 0; i < maxResult; i++)
                    {
                        newTable.ImportRow(result.Rows[i]);
                    }
                    result = newTable;
                }
                DataTable.Set(context, result);
            }

            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            Thread.Sleep(delayAfter);
        }
    }
}
