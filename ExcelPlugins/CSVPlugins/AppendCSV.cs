using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CSVPlugins
{
    [Designer(typeof(WriteCSVDesigner))]
    public sealed class AppendCSV : CodeActivity
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


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("数据表")]
        [Description("要写入CSV文件的数据表。")]
        public InArgument<DataTable> InDataTable { get; set; }

        [Category("输入")]
        [DisplayName("编码")]
        public InArgument<string> EncodingType { get; set; }

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
        [Description("指定 CSV 文件中的分隔符,可以是逗号（','）、分号（';'）、制表符、竖线符号（'|'）或插入符号（'^'）。")]
        public DelimiterEnums Delimiter
        {
            get { return _Delimiter; }
            set { _Delimiter = value; }
        }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/CSV/csv-append.png"; } }

        #endregion


        public AppendCSV()
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
            else if (Delimiter == DelimiterEnums.Comma逗号)
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

                /*将DataTable内容写入CSV文件*/
                try
                {
                    DataTable inDataTable = InDataTable.Get(context);
                    WriteCSVFile(inDataTable, filePath, csvEncoding, delimiter);
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

        public void WriteCSVFile(DataTable dt, string fullPath, Encoding encodingType, string delimiter)
        {
            FileInfo fi = new FileInfo(fullPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            FileStream fs = new FileStream(fullPath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, encodingType);

            string data = "";
            //写出各行数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                data = "";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string str = dt.Rows[i][j].ToString();
                    str = str.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号
                    if (str.Contains(',') || str.Contains('"') || str.Contains('\r') || str.Contains('\n'))
                    //含逗号 冒号 换行符的需要放到引号中
                    {
                        str = string.Format("\"{0}\"", str);
                    }

                    data += str;
                    if (j < dt.Columns.Count - 1)
                    {
                        data += delimiter;
                    }
                }
                sw.WriteLine(data);
            }
            sw.Close();
            fs.Close();
        }
    }
}
