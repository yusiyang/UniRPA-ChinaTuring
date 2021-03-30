using System.Activities;
using System.ComponentModel;
using System;
using Plugins.Shared.Library;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using Plugins.Shared.Library.Librarys;
using System.Threading;
using System.IO;
using Plugins.Shared.Library.Exceptions;
using System.Activities.Statements;

namespace ExcelPlugins
{

    [Designer(typeof(ExcelWriteCellDesigner))]
    public sealed class ExcelWriteCell : AsyncCodeActivity
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

        InArgument<string> _PathUrl;
        [Category("输入")]
        [DisplayName("Excel文件路径")]
        [Description("要打开的Excel文件的全路径。必须将文本放入引号中。")]
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

        [Category("输入")]
        [DisplayName("工作表次序")]
        [Description("要操作的单元格所在工作表的索引号。从1开始计算。请输入一个整数。")]

        public InArgument<Int32> SheetIndex { get; set; }

        [Category("输入")]
        [DisplayName("工作表名称")]
        [Description("要操作的单元格所在工作表的名称。为空代表当前活动的工作表。必须将文本放入引号中。")]
        public InArgument<string> SheetName { get; set; }

        [Category("输入")]
        [DisplayName("单元格")]
        [Description("例：\"A1\"。必须将文本放入引号中。")]
        public InArgument<string> Cell { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("单元格内容")]
        [Description("要写入的单元格内容，可设置公式。例：\"= Sum(A1 / B1)\"。")]
        public InArgument<string> CellContent { get; set; }

        #endregion

        #region 属性分类：选项
        bool _IsVisible = false;
        [Category("选项")]
        [DisplayName("流程是否可见")]
        public bool IsVisible
        {
            get { return _IsVisible; }
            set { _IsVisible = value; }
        }
        #endregion

        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/excel/setcell.png"; } }

        [Browsable(false)]
        public string ClassName { get { return "ExcelWriteCell"; } }

        #endregion


        public ExcelWriteCell()
        {
            //填写默认值
            //SheetIndex = 1;
            SheetName = "Sheet1";
        }

        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return ClassName;
        }

        private Excel::Application excelApp;

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int delayBefore = MouseActivity.Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                Object cellContent = CellContent.Get(context);
                string filePath = PathUrl.Get(context);
                var cellAddress = Cell.Get(context);
                var sheetIndex = SheetIndex.Get(context);
                string sheetName = SheetName.Get(context);

                excelApp = new Excel::Application();
                excelApp.Visible = IsVisible ? true : false;
                excelApp.DisplayAlerts = false;

                if (!File.Exists(filePath))
                {
                    throw new Exception("文件不存在，请检查路径有效性");
                }
                else
                {
                    //可用Open或Add函数打开文件，但对于执行VBA，Add无保存权限
                    excelApp.Workbooks.Open(filePath);
                }

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



                sheet.Range[cellAddress].Value = cellContent;

                System.Runtime.InteropServices.Marshal.ReleaseComObject(sheet);
                sheet = null;
                excelApp.ActiveWorkbook.Save();
                excelApp.Workbooks.Close();
                new CommonVariable().realaseProcessExit(excelApp);
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
