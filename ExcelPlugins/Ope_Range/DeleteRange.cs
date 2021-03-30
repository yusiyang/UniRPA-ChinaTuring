using System.Activities;
using System.ComponentModel;
using System;
using Excel = Microsoft.Office.Interop.Excel;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Librarys;
using System.Threading;
using Plugins.Shared.Library.Exceptions;

namespace ExcelPlugins
{
    [Designer(typeof(DeleteRangeDesigner))]
    public sealed class DeleteRange : AsyncCodeActivity
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
        public InArgument<int> SheetIndex { get; set; }

        [Category("输入")]
        [DisplayName("工作表名称")]
        [Description("要操作的单元格所在工作表的名称。为空代表当前活动的工作表。必须将文本放入引号中。")]
        public InArgument<string> SheetName { get; set; }

        [Category("输入")]
        [DisplayName("单元格区域")]
        [Description("例：\"A1:D5\"。必须将文本放入引号中。")]
        public InArgument<string> CellRange { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("移动单元格")]
        [Description("不选中该选项，只删除单元格数据，并保持格式不变。如果选中，将根据“移动选项”移动单元格。")]
        public bool ShiftCells { get; set; }

        public enum ShiftTypes
        {
            向上移动,
            向左移动,
            整行,
            整列
            //ShiftUp,
            //ShiftLeft,
            //EntireRow,
            //EntireColumn
        }
        [Category("选项")]
        [DisplayName("移动选项")]
        [Description("设置如何移动单元格以填充已删除的范围。")]
        public ShiftTypes ShiftType { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/excel/rangedelete.png"; } }

        [Browsable(false)]
        public string ClassName { get { return "DeleteRange"; } }

        #endregion


        public DeleteRange()
        {
        }

        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return ClassName;
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int delayBefore = MouseActivity.Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            PropertyDescriptor property = context.DataContext.GetProperties()[ExcelCreate.GetExcelAppTag];
            Excel::Application excelApp = property.GetValue(context.DataContext) as Excel::Application;
            try
            {
                var cellRange = CellRange.Get(context);
                var sheetIndex = SheetIndex.Get(context);
                string sheetName = SheetName.Get(context);
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

                var range = cellRange.IsNullOrWhiteSpace() ? sheet.UsedRange : sheet.Range[cellRange];

                if (!ShiftCells)
                    range.Clear();
                else
                {
                    if (ShiftType == ShiftTypes.向上移动)
                    {
                        range.Delete(Excel.XlDeleteShiftDirection.xlShiftUp);
                    }
                    else if (ShiftType == ShiftTypes.向左移动)
                    {
                        range.Delete(Excel.XlDeleteShiftDirection.xlShiftToLeft);
                    }
                    else if (ShiftType == ShiftTypes.整行)
                    {
                        range.EntireRow.Delete();
                    }
                    else
                    {
                        range.EntireColumn.Delete();
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(sheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                sheet = null;
                range = null;
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
