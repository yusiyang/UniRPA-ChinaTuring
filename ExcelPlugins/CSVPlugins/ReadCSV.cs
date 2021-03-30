using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CSVPlugins
{
    [Designer(typeof(ReadCSVDesigner))]
    public sealed class ReadCSV : CodeActivity
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


        #region 属性分类：文件

        InArgument<string> _PathUrl;
        [Category("文件")]
        [RequiredArgument]
        [DisplayName("文件路径")]
        [Description("CSV文件的全路径。必须将文本放入引号中。")]
        public InArgument<string> PathUrl
        {
            get
            {
                return _PathUrl;
            }
            set
            {
                _PathUrl = value;
            }
        }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [RequiredArgument]
        [DisplayName("数据表")]
        [Description("表示包含从 CSV 文件中获取的信息的输出数据表。")]
        public OutArgument<DataTable> OutDataTable { get; set; }

        #endregion


        #region 属性分类：选项

        public enum DelimiterEnums
        {
            Tab制表符,
            Comma逗号,
            Semicolon分号,
            Caret插入符号,
            Pipe竖线
        }
        DelimiterEnums _Delimiter = DelimiterEnums.Comma逗号;
        [Category("选项")]
        [DisplayName("分隔符")]
        [Description("指定 CSV 文件中的分隔符。可以是逗号（','）、分号（';'）、制表符、竖线符号（'|'）或插入符号（'^'）。")]
        public DelimiterEnums Delimiter
        {
            get { return _Delimiter; }
            set { _Delimiter = value; }
        }

        [Category("选项")]
        [DisplayName("包含表头")]
        [Description("指定是否应考虑 CSV 文件中的第一行，以包含列名称。如果设置为 false，则输出数据表将包含带有默认名称的列。")]
        public bool IncludeColumnNames { get; set; }

        [Category("选项")]
        [DisplayName("编码")]
        public InArgument<string> EncodingType { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/CSV/csv-read.png"; } }

        #endregion


        public ReadCSV()
        {
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            Encoding csvEncoding;
            string filePath = PathUrl.Get(context);
            string encodingType = EncodingType.Get(context);
            string delimiter = ",";
            if (Delimiter == DelimiterEnums.Caret插入符号)
                delimiter = "^";
            else if(Delimiter == DelimiterEnums.Comma逗号)
                delimiter = ",";
            else if (Delimiter == DelimiterEnums.Pipe竖线)
                delimiter = "|";
            else if (Delimiter == DelimiterEnums.Semicolon分号)
                delimiter = ";";
            else if (Delimiter == DelimiterEnums.Tab制表符)
                delimiter = "	";

            try
            {
                if (!File.Exists(filePath))
                {
                    //SharedObject.Instance.Output(SharedObject.enOutputType.Error, "文件不存在，请检查路径有效性");

                    //Thread.Sleep(delayAfter);
                    //return;
                    throw new Exception("文件不存在，请检查路径有效性");

                }

                /*取字符编码 如果为空则取文件编码 异常则取系统默认编码*/
                try
                {
                    if (encodingType == null)
                        csvEncoding = CSVEncoding.GetEncodingType(filePath);
                    else
                        csvEncoding = Encoding.GetEncoding(encodingType);
                }
                catch (Exception)
                {
                    csvEncoding = System.Text.Encoding.Default;
                }

                /*设置DataTable*/
                try
                {
                    DataTable dataTable = ReadCSVFile(filePath, csvEncoding, delimiter);
                    OutDataTable.Set(context, dataTable);
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            System.Diagnostics.Debug.WriteLine("dt : " + dr[i]);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
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

        public DataTable ReadCSVFile(string filePath, Encoding encodingType, string delimiter)
        {
            DataTable dt = new DataTable();
            FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            StreamReader sr = new StreamReader(fs, encodingType);
            //string fileContent = sr.ReadToEnd();
            //encoding = sr.CurrentEncoding;
            //记录每次读取的一行记录
            string strLine = "";
            //记录每行记录中的各字段内容
            string[] aryLine = null;
            string[] tableHead = null;
            //标示列数
            int columnCount = 0;
            bool headFlag = false;
            //标示是否是读取的第一行
            //bool IsFirst = true;
            //逐行读取CSV中的数据
            while ((strLine = sr.ReadLine()) != null)
            {
                if (IncludeColumnNames == true)
                {
                    if(delimiter == "   ")
                    {
                        tableHead = Regex.Split(strLine, delimiter, RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        char cDelimiter = delimiter[0];
                        tableHead = strLine.Split(cDelimiter);
                    }
                    IncludeColumnNames = false;
                    headFlag = true;
                    columnCount = tableHead.Length;
                    //创建列
                    for (int i = 0; i < columnCount; i++)
                    {
                        DataColumn dc = new DataColumn(tableHead[i]);
                        dt.Columns.Add(dc);
                    }
                }
                else
                {
                    aryLine = strLine.Split(',');
                    columnCount = aryLine.Length;
                    if (headFlag == false)
                    {
                        headFlag = true;
                        string nameBuff = "列";
                        tableHead = new string[columnCount];
                        for (int i = 0; i < columnCount; i++)
                        {
                            string colName = nameBuff + i;
                            tableHead[i] = colName;
                            DataColumn dc = new DataColumn(tableHead[i]);
                            dt.Columns.Add(dc);
                        }
                    }
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < columnCount; j++)
                    {
                        dr[j] = aryLine[j];
                    }
                    dt.Rows.Add(dr);
                }
            }
            //if (aryLine != null && aryLine.Length > 0)
            //{
            //    dt.DefaultView.Sort = tableHead[0] + " " + "asc";
            //}

            sr.Close();
            fs.Close();
            return dt;
        }
    }
}
