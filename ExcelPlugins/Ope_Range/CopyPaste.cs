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

    [Designer(typeof(CopyPasteDesigner))]
    public sealed class CopyPaste : AsyncCodeActivity
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


        #region 属性分类：粘贴选项

        [Category("粘贴选项")]
        [RequiredArgument]
        [DisplayName("目标单元格")]
        [Description("粘贴单元格区域的起始单元格。必须将文本放入引号中。")]
        public InArgument<string> DestCell { get; set; }

        [Category("粘贴选项")]
        [DisplayName("目标工作表次序")]
        [Description("要粘贴的单元格所在工作表的索引号。从1开始计算。请输入一个整数。")]
        public InArgument<int?> DestSheetIndex { get; set; }

        [Category("粘贴选项")]
        [DisplayName("目标工作表名称")]
        [Description("要粘贴单元格区域所在工作表的名称。必须将文本放入引号中。")]
        public InArgument<string> DestSheet { get; set; }

        #endregion


        #region 属性分类：复制选项

        [Category("复制选项")]
        [DisplayName("工作表次序")]
        [Description("要操作的单元格所在工作表的索引号。从1开始计算。请输入一个整数。")]
        public InArgument<int?> CopySheetIndex { get; set; }

        [Category("复制选项")]
        [DisplayName("工作表名称")]
        [Description("要复制的单元格区域所在工作表的名称。必须将文本放入引号中。")]
        public InArgument<string> CopySheet { get; set; }

        [Category("复制选项")]
        [RequiredArgument]
        [DisplayName("单元格区域")]
        [Description("如 \"A1:D5\"。必须将文本放入引号中。")]
        public InArgument<string> CellRange { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/excel/copy.png"; } }

        [Browsable(false)]
        public string ClassName { get { return "CopyPaste"; } }

        #endregion


        public CopyPaste()
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
                #region 源sheet
                var copySheetIndex = CopySheetIndex.Get(context);
                string copySheet = CopySheet.Get(context);
                string cellRange = CellRange.Get(context);

                Excel.Worksheet sheet = excelApp.ActiveSheet;
                try
                {
                    if (copySheetIndex.HasValue)
                    {
                        sheet = excelApp.ActiveWorkbook.Sheets[copySheetIndex];
                    }
                    else if (!copySheet.IsNullOrWhiteSpace())
                    {
                        sheet = excelApp.ActiveWorkbook.Sheets[copySheet];
                    }
                }
                catch
                {
                    throw new Exception("源Sheet页不存在！");
                }


                sheet.Range[cellRange].Copy(Type.Missing);

                #endregion

                #region 目标sheet
                var destSheetIndex = DestSheetIndex.Get(context);
                string destSheet = DestSheet.Get(context);
                string destCell = DestCell.Get(context);
                Excel::_Worksheet pasteSheet = excelApp.ActiveSheet;
                try
                {
                    if (destSheetIndex.HasValue)
                    {
                        pasteSheet = excelApp.ActiveWorkbook.Sheets[destSheetIndex];
                    }
                    else if (!destSheet.IsNullOrWhiteSpace())
                    {
                        pasteSheet = excelApp.ActiveWorkbook.Sheets[destSheet];
                    }
                }
                catch
                {
                    throw new Exception("目标Sheet页不存在！");
                }



                Excel::Range pasteRange = pasteSheet.Range[destCell, destCell];
                pasteSheet.Paste(pasteRange);
                #endregion

                System.Runtime.InteropServices.Marshal.ReleaseComObject(sheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pasteSheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pasteRange);
                sheet = null; pasteSheet = null; pasteRange = null;
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
