using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Plugins.Shared.Library.UiAutomation.DataExtract
{
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
    [XmlRoot("extract-table")]
    public class ExtractTable
    {
        public ExtractTable()
        {
            GetColumnsName = 1;
            GetEmptyColumns = 1;
            ColumnsNameSource = "Longest";
        }
        [XmlAttribute("get_columns_name")]
        public int GetColumnsName { get; set; }
        [XmlAttribute("get_empty_columns")]
        public int GetEmptyColumns { get; set; }
        [XmlAttribute("columns_name_source")]
        public string ColumnsNameSource { get; set; }
    }
}
