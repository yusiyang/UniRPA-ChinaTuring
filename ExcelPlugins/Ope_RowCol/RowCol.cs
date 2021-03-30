using System.Activities;
using System.ComponentModel;
using System;
using System.Windows.Data;
using System.Globalization;
using Excel = Microsoft.Office.Interop.Excel;
using Plugins.Shared.Library;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library.Exceptions;

namespace ExcelPlugins
{
    public enum Operations
    {
        删除 = 1,
        隐藏 = 2,
        添加 = 3,
        获取 = 4
    }

    /*RadioButton到Enum转换器*/
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return value == null ? false : value.Equals(parameter);
            if (value == null)
                return false;
            else
                return value.Equals(parameter);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return value != null && value.Equals(true) ? parameter : Binding.DoNothing;
            if (value != null && value.Equals(true))
                return parameter;
            else
                return Binding.DoNothing;
        }
    }



    [Designer(typeof(RowColDesigner))]
    public sealed class RowCol : AsyncCodeActivity
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

        InArgument<Int32> _RowColBegin;
        [Category("输入")]
        [RequiredArgument]
        [DisplayName("行号/列号(开始)")]
        public InArgument<Int32> RowColBegin
        {
            get
            {
                return _RowColBegin;
            }
            set
            {
                _RowColBegin = value;
            }
        }

        InArgument<Int32> _RowColEnd;
        [Category("输入")]
        [RequiredArgument]
        [DisplayName("行号/列号(结束)")]
        public InArgument<Int32> RowColEnd
        {
            get
            {
                return _RowColEnd;
            }
            set
            {
                _RowColEnd = value;
            }
        }

        [Category("输入")]
        [DisplayName("工作表名称")]
        [Description("要操作的对象所在工作的表名称。为空代表当前活动工作表。必须将文本放入引号中。")]
        public InArgument<string> SheetName { get; set; }

        #endregion


        #region 属性分类：选项

        Operations _Operation = (Operations)1;
        [Category("选项")]
        [DisplayName("行列操作")]
        public Operations Operation
        {
            get
            {
                return _Operation;
            }
            set
            {
                _Operation = value;
            }
        }

        public enum RowCols
        {
            行 = 1,
            列 = 2
        }
        RowCols _RowColSel = (RowCols)1;
        [Category("选项")]
        [DisplayName("行列选择")]
        public RowCols RowColSel
        {
            get
            {
                return _RowColSel;
            }
            set
            {
                _RowColSel = value;
            }
        }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/excel/rowcol.png"; } }

        [Browsable(false)]
        public string ClassName { get { return "RowCol"; } }

        #endregion


        public RowCol()
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
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            PropertyDescriptor property = context.DataContext.GetProperties()[ExcelCreate.GetExcelAppTag];
            Excel::Application excelApp = property.GetValue(context.DataContext) as Excel::Application;
            try
            {
                m_Delegate = new runDelegate(Run);
                string sheetName = SheetName.Get(context);
                Excel::_Worksheet sheet = excelApp.ActiveSheet;
                try
                {
                    if (!string.IsNullOrWhiteSpace(sheetName))
                    {
                        sheet = excelApp.ActiveWorkbook.Sheets[sheetName];
                    }
                }
                catch
                {
                    throw new Exception("Sheet页不存在！");
                }



                Int32 rowColBegin = RowColBegin.Get(context);
                Int32 rowColEnd = RowColEnd.Get(context);
                if (rowColBegin > rowColEnd)
                {
                    throw new Exception("EXCEL行列操作开始值不能大于结束值");
                }

                switch ((int)Operation)
                {
                    case 1:
                        {
                            if (RowColSel == (RowCols)1)
                            {
                                sheet.Range[
                                   sheet.Cells[rowColBegin, 1],
                                   sheet.Cells[rowColEnd, sheet.Columns.Count]].
                                   Delete(Excel.XlDeleteShiftDirection.xlShiftUp);
                            }
                            else
                            {
                                sheet.Range[
                                    sheet.Cells[1, rowColBegin],
                                    sheet.Cells[sheet.Rows.Count, rowColEnd]]
                                    .Delete(Excel.XlDeleteShiftDirection.xlShiftToLeft);
                            }
                            break;
                        }
                    case 2:
                        {
                            if (RowColSel == (RowCols)1)
                            {
                                sheet.Range[
                                    sheet.Cells[rowColBegin, 1],
                                    sheet.Cells[rowColEnd, 1]].
                                    EntireRow.Hidden = true;
                            }
                            else
                            {
                                sheet.Range[
                                    sheet.Cells[1, rowColBegin],
                                    sheet.Cells[sheet.Rows.Count, rowColEnd]].
                                    EntireColumn.Hidden = true;
                            }
                            break;
                        }
                    case 3:
                        {
                            if (RowColSel == (RowCols)1)
                            {
                                sheet.Range[
                                   sheet.Cells[rowColBegin, 1],
                                   sheet.Cells[rowColEnd, sheet.Columns.Count]].
                                   Insert(Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove);
                            }
                            else
                            {
                                sheet.Range[
                                    sheet.Cells[1, rowColBegin],
                                    sheet.Cells[sheet.Rows.Count, rowColEnd]].
                                    Insert(Excel.XlInsertShiftDirection.xlShiftToRight);
                            }
                            break;
                        }
                    default:
                        return m_Delegate.BeginInvoke(callback, state);
                }
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

            return m_Delegate.BeginInvoke(callback, state);
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            Thread.Sleep(delayAfter);
        }
    }
}