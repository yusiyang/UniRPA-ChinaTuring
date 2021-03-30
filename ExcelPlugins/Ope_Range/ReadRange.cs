using System.Activities;
using System.ComponentModel;
using System;
using Plugins.Shared.Library;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using Plugins.Shared.Library.Librarys;
using System.Threading;
using Plugins.Shared.Library.Exceptions;

namespace ExcelPlugins
{

    [Designer(typeof(ReadRangeDesigner))]
    public sealed class ReadRange : AsyncCodeActivity
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


        #region 属性分类：输入

        [Category("输入")]
        [DisplayName("工作表次序")]
        [Description("要操作的单元格所在工作表的索引号。从1开始计算。请输入一个整数。")]

        public InArgument<Int32> SheetIndex { get; set; }

        [Category("输入")]
        [DisplayName("工作表名称")]
        [Description("要操作的单元格所在工作表的名称。为空代表当前活动的工作表。必须将文本放入引号中。")]
        public InArgument<string> SheetName { get; set; }

        [Category("输入")]
        [DisplayName("单元格区域")]
        [Description("例：\"A1:D5\"。此处有三种填写方式。若填写\"\"，则读取表格中有数据的部分所围成的最大范围的数据；若填写确定区域，如\"A1:D5\"，则读取该指定范围的数据；若填写起始单元格+冒号，如\"B2:\"，则读取该单元格开始到右下角最后一个有数据的单元格所围成的范围的数据。必须将文本放入引号中。")]
        public InArgument<string> CellRange { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [RequiredArgument]
        [DisplayName("数据表")]
        [Description("存储读取数据的数据表。")]
        public OutArgument<System.Data.DataTable> DataTable { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("表头")]
        [Description("是否包含表头。")]
        public bool HasTitle { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/excel/rangeread.png"; } }

        [Browsable(false)]
        public string ClassName { get { return "ReadRange"; } }

        #endregion


        public ReadRange()
        {
        }

        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return ClassName;
        }

        public void CompressValueArray(object[,] valueArray, out int iRowCount, out int iColCount)
        {
            iRowCount = 0;
            iColCount = 0;
            if (valueArray == null)
            {
                return;
            }
            int oneDim = valueArray.GetLength(0);
            int twoDim = valueArray.GetLength(1);
            for (int i = 1; i < oneDim + 1; i++)
            {
                for (int j = 1; j < twoDim + 1; j++)
                {
                    if (valueArray[i, j] == null)
                    {
                        continue;
                    }
                    else if (!(string.IsNullOrEmpty(valueArray[i, j].ToString()) || string.IsNullOrWhiteSpace(valueArray[i, j].ToString())))
                    {
                        if (i > iRowCount)
                        {
                            iRowCount = i;
                        }
                        if (j > iColCount)
                        {
                            iColCount = j;
                        }
                    }
                }
            }
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int delayBefore = MouseActivity.Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            PropertyDescriptor property = context.DataContext.GetProperties()[ExcelCreate.GetExcelAppTag];
            Excel::Application excelApp = property.GetValue(context.DataContext) as Excel::Application;
            try
            {
                var sheetIndex = SheetIndex.Get(context);
                string sheetName = SheetName.Get(context);
                string cellRange = CellRange.Get(context);

                Excel.Worksheet sheet = excelApp.ActiveSheet;
                try
                {
                    if (sheetIndex > 0)
                    {
                        sheet = excelApp.ActiveWorkbook.Sheets[sheetIndex];
                    }
                    else if (!sheetName.IsNullOrWhiteSpace())
                    {
                        sheet = excelApp.ActiveWorkbook.Sheets[sheetName];
                    }
                }
                catch
                {
                    throw new Exception("Sheet页不存在！");
                }

                System.Data.DataTable dt = new System.Data.DataTable();
                Excel.Range range;
                if (!cellRange.IsNullOrWhiteSpace() && cellRange.EndsWith(":"))
                {

                    var row = sheet.UsedRange.Row+sheet.UsedRange.Rows.Count-1;
                    var col = sheet.UsedRange.Column + sheet.UsedRange.Columns.Count - 1;
                    range = sheet.Range[cellRange.Replace(":",""),sheet.Cells[row,col]];
                }
                else
                {
                    range = cellRange.IsNullOrWhiteSpace() ? sheet.UsedRange : sheet.Range[cellRange];
                }

                
                int iRowCount = range.Rows.Count;
                int iColCount = range.Columns.Count;
                if (range.Value == null) 
                {
                    iRowCount -= 1;
                    iColCount -= 1;
                }
                if (iRowCount > 1 || iColCount > 1)
                {
                    var valueArray = (object[,])range.Value;
                    CompressValueArray(valueArray, out iRowCount, out iColCount);
                }
                var rowStart = range.Row;
                var rowEnd = rowStart + iRowCount - 1;
                var colStart = range.Column;
                var colEnd = colStart + iColCount - 1;

                var columnNameDic = new Dictionary<string, int>();

                //生成列头
                for (int i = colStart; i <= colEnd; i++)
                {
                    var name = "column" + i;
                    if (HasTitle)
                    {
                        var txt = ((Excel.Range)sheet.Cells[range.Row, i]).Text.ToString();
                        if (!string.IsNullOrEmpty(txt))
                            name = txt;
                    }
                    if (columnNameDic.ContainsKey(name))
                    {
                        var sameColumnIndex = columnNameDic[name];
                        name = $"{name}_{sameColumnIndex}";//重复行名称会报错。
                        columnNameDic[name] = sameColumnIndex + 1;
                    }
                    else
                    {
                        columnNameDic[name] = 1;
                    }
                    dt.Columns.Add(new System.Data.DataColumn(name, typeof(string)));
                }
                //生成行数据
                rowStart = HasTitle ? rowStart + 1 : rowStart;
                for (int iRow = rowStart; iRow <= rowEnd; iRow++)
                {
                    System.Data.DataRow dr = dt.NewRow();
                    var drColIndex = 0;
                    for (int iCol = colStart; iCol <= colEnd; iCol++)
                    {
                        range = (Microsoft.Office.Interop.Excel.Range)sheet.Cells[iRow, iCol];
                        dr[drColIndex++] = (range.Value2 == null) ? "" : range.Text.ToString();
                    }
                    dt.Rows.Add(dr);
                }
                DataTable.Set(context, dt);

                System.Runtime.InteropServices.Marshal.ReleaseComObject(sheet);
                sheet = null;
                GC.Collect();
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                new CommonVariable().realaseProcessExit(excelApp);
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
            int delayAfter = MouseActivity.Common.GetValueOrDefault(context, this.DelayAfter, 300);
            Thread.Sleep(delayAfter);
        }
    }
}
