using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace DataTableActivity
{
    [Designer(typeof(BuildDataTableDesigner))]
    public sealed class BuildDataTable : AsyncCodeActivity
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

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [RequiredArgument]
        [DisplayName("数据表")]
        [Description("根据行列信息生成的 DataTable 表。")]
        public OutArgument<DataTable> DataTable { get; set; }

        #endregion


        #region 属性分类：杂项

        [RequiredArgument]
        [Browsable(false)]
        public string TableInfo { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataTable/build-data-table.png";
            }
        }

        #endregion


        public BuildDataTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.TableName = "TableName";
            dataTable.Columns.Add(new DataColumn("Column1", typeof(string))
            {
                MaxLength = 100
            });
            dataTable.Columns.Add(new DataColumn("Column2", typeof(int)));
            DataRow dataRow = dataTable.NewRow();
            dataRow["Column1"] = "text";
            dataRow["Column2"] = 1;
            dataTable.Rows.Add(dataRow);
            StringWriter stringWriter = new StringWriter();
            dataTable.WriteXml(stringWriter, XmlWriteMode.WriteSchema);
            this.TableInfo = stringWriter.ToString();
        }

        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return DisplayName;
        }


        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            try
            {
                DataTable dataTable = new DataTable();
                BuildDataTable.ReadDataTableFromXML(this.TableInfo, dataTable);
                this.DataTable.Set(context, dataTable);
            }

            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            m_Delegate = new runDelegate(Run);

            return m_Delegate.BeginInvoke(callback, state);

        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {

        }


        public static void ReadDataTableFromXML(string text, DataTable dt)
        {
            try
            {
                using (StringReader stringReader = new StringReader(text))
                {
                    dt.ReadXml(stringReader);
                }
            }
            catch (ArgumentException)
            {
                using (XmlReader xmlReader = BuildDataTable.ReplaceAssemblyName(text))
                {
                    dt.ReadXml(xmlReader);
                }
            }
        }

        private static XmlReader ReplaceAssemblyName(string info)
        {
            List<Tuple<string, string>> expr_05 = new List<Tuple<string, string>>();
            expr_05.Add(new Tuple<string, string>("GenericValue", "System"));
            expr_05.Add(new Tuple<string, string>("Image", "UiAutomation"));
            XElement xElement = XElement.Parse(info);
            using (List<Tuple<string, string>>.Enumerator enumerator = expr_05.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Tuple<string, string> tuple = enumerator.Current;
                    using (IEnumerator<XElement> enumerator2 = (from el in xElement.Descendants(BuildDataTable._xsNamespace + "element")
                                                                where BuildDataTable.CheckReplaceCondition(el, tuple.Item1)
                                                                select el).GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            BuildDataTable.Replace(enumerator2.Current, tuple.Item2);
                        }
                    }
                }
            }
            return xElement.CreateReader();
        }

        private static bool CheckReplaceCondition(XElement node, string component)
        {
            if (node == null)
            {
                return false;
            }
            XAttribute expr_1A = node.Attribute(BuildDataTable._msdataNamespace + "DataType");
            string text = (expr_1A != null) ? expr_1A.ToString() : null;
            return !string.IsNullOrWhiteSpace(text) && (text.Contains("UiPath.Core." + component) && text.Contains("Culture") && text.Contains("PublicKeyToken") && text.Contains("Version")) && text.Contains("UiPath.Core,");
        }
        private static readonly XNamespace _xsNamespace = "http://www.w3.org/2001/XMLSchema";
        private static readonly XNamespace _msdataNamespace = "urn:schemas-microsoft-com:xml-msdata";
        private static void Replace(XElement node, string package)
        {
            //XAttribute xAttribute = (node != null) ? node.Attribute(BuildDataTable._msdataNamespace + "DataType") : null;
            //if (xAttribute == null)
            //{
            //    return;
            //}
            //string value = xAttribute.Value;
            //xAttribute.Value = ((value != null) ? value.Replace("UiPath.Core,", "UiPath." + package + ".Activities,") : null);
        }
    }
}