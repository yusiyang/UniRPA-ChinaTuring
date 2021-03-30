using System.Activities;
using System.ComponentModel;
using System;
using Excel = Microsoft.Office.Interop.Excel;
using Plugins.Shared.Library;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library.Exceptions;

namespace ExcelPlugins
{
    [Designer(typeof(RenameSheetDesigner))]
    public sealed class RenameSheet : AsyncCodeActivity
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

        InArgument<string> _SheetName;
        [Category("输入")]
        [RequiredArgument]
        [DisplayName("工作表新名称")]
        [Description("要操作的单元格所在工作表的新名称。为空代表当前活动的工作表。必须将文本放入引号中。")]
        public InArgument<string> SheetName
        {
            get
            {
                return _SheetName;
            }
            set
            {
                _SheetName = value;
            }
        }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("SheetName")]
        [DisplayName("工作表旧名称")]
        [Description("要操作的单元格所在工作表的旧名称。为空代表当前活动的工作表。必须将文本放入引号中。")]
        public InArgument<string> OldSheetName { get; set; }

        InArgument<Int32> _SheetIndex;
        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("SheetIndex")]
        [DisplayName("工作表次序")]
        [Description("要操作的单元格所在工作表的索引号。从1开始计算。请输入一个整数。")]
        public InArgument<Int32> SheetIndex
        {
            get
            {
                return _SheetIndex;
            }
            set
            {
                _SheetIndex = value;
            }
        }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/excel/renamesheet.png"; } }

        [Browsable(false)]
        public string ClassName { get { return "OpenDelSheet"; } }

        #endregion


        public RenameSheet()
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
                Int32 sheetIndex = SheetIndex.Get(context);
                string oldSheetName = OldSheetName.Get(context);
                string sheetName = SheetName.Get(context);

                Excel::Sheets sheets = excelApp.ActiveWorkbook.Sheets;
                try
                {
                    if (!string.IsNullOrEmpty(oldSheetName))
                    {
                        sheets.Item[oldSheetName].Name = sheetName;
                    }
                    else
                    {
                        sheets.Item[sheetIndex].Name = sheetName;
                    }
                }
                catch
                {
                    throw new Exception("Sheet页不存在！");
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(sheets);
                sheets = null;
                GC.Collect();
            }
            catch (Exception e)
            {
                new CommonVariable().realaseProcessExit(excelApp);
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
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            Thread.Sleep(delayAfter);
        }
    }
}
