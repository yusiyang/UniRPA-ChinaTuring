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
    [Designer(typeof(AreaSettingDesigner))]
    public sealed class AreaSetting : AsyncCodeActivity
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
        public InArgument<int?> SheetIndex { get; set; }

        [Category("输入")]
        [DisplayName("工作表名称")]
        [Description("要操作的对象所在工作表的名称。必须将文本放入引号中。")]
        public InArgument<string> SheetName { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("单元格区域")]
        [Description("例：\"A1:D5\"。必须将文本放入引号中。")]
        public InArgument<string> CellRange { get; set; }

        ColorIndexEnum _CellColor = (ColorIndexEnum)0;
        [Category("选项")]
        [DisplayName("单元格填充色")]
        public ColorIndexEnum CellColor
        {
            get
            {
                return _CellColor;
            }
            set
            {
                _CellColor = value;
            }
        }

        AlignEnum _AlignStyle = (AlignEnum)0;
        [Category("选项")]
        [DisplayName("对齐方式")]
        public AlignEnum AlignStyle
        {
            get
            {
                return _AlignStyle;
            }
            set
            {
                _AlignStyle = value;
            }
        }

        #endregion


        #region 属性分类：字体设置

        ExcelFontEnum _Font = (ExcelFontEnum)0;
        [Category("字体设置")]
        [DisplayName("字体")]
        public ExcelFontEnum Font
        {
            get
            {
                return _Font;
            }
            set
            {
                _Font = value;
            }
        }

        InArgument<Int32> _FontSize = 11;
        [Category("字体设置")]
        [DisplayName("字号")]
        public InArgument<Int32> FontSize
        {
            get
            {
                return _FontSize;
            }
            set
            {
                _FontSize = value;
            }
        }

        ColorIndexEnum _FontColor = (ColorIndexEnum)0;
        [Category("字体设置")]
        [DisplayName("颜色")]
        public ColorIndexEnum FontColor
        {
            get
            {
                return _FontColor;
            }
            set
            {
                _FontColor = value;
            }
        }

        bool _isBold;
        [Category("字体设置")]
        [DisplayName("粗体")]
        public bool isBold
        {
            get
            {
                return _isBold;
            }
            set
            {
                _isBold = value;
            }
        }

        bool _isItalic;
        [Category("字体设置")]
        [DisplayName("斜体")]
        public bool isItalic
        {
            get
            {
                return _isItalic;
            }
            set
            {
                _isItalic = value;
            }
        }

        bool _isUnderLine;
        [Category("字体设置")]
        [DisplayName("下划线")]
        public bool isUnderLine
        {
            get
            {
                return _isUnderLine;
            }
            set
            {
                _isUnderLine = value;
            }
        }

        #endregion


        #region 属性设置：行高列宽

        InArgument<double> _RowHeight = 14.25;
        [Category("行高列宽")]
        [DisplayName("行高")]
        public InArgument<double> RowHeight
        {
            get
            {
                return _RowHeight;
            }
            set
            {
                _RowHeight = value;
            }
        }

        InArgument<double> _ColWidth = 8.38;
        [Category("行高列宽")]
        [DisplayName("列宽")]
        public InArgument<double> ColWidth
        {
            get
            {
                return _ColWidth;
            }
            set
            {
                _ColWidth = value;
            }
        }

        #endregion


        #region 属性分类：边框

        BorderType _BorderType = (BorderType)0;
        [Category("边框")]
        [DisplayName("边框类型")]
        public BorderType BorderType
        {
            get
            {
                return _BorderType;
            }
            set
            {
                _BorderType = value;
            }
        }

        BorderStyle _BorderStyle = (BorderStyle)(-4142);
        [Category("边框")]
        [DisplayName("边框风格")]
        public BorderStyle BorderStyle
        {
            get
            {
                return _BorderStyle;
            }
            set
            {
                _BorderStyle = value;
            }
        }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/excel/setting.png"; } }

        [Browsable(false)]
        public string ClassName { get { return "AreaSetting"; } }

        #endregion


        public AreaSetting()
        {
        }

        public string ConvertFont(string fontName)
        {
            if (fontName == "等线Light")
                return "等线 Light";
            else if (fontName == "ArialBlack")
                return "Arial Black";
            else if (fontName == "ArialNarrow")
                return "Arial Narrow";
            else if (fontName == "ArialRoundedMTBold")
                return "Arial Rounded MT Bold";
            else if (fontName == "ArialUnicodeMS")
                return "Arial Unicode MS";
            else if (fontName == "CalibriLight")
                return "Calibri Light";
            else if (fontName == "MicrosoftYaHeiUI")
                return "Microsoft YaHei UI";
            else if (fontName == "MicrosoftYaHeiUILight")
                return "Microsoft YaHei UI Light";
            else if (fontName == "MicrosoftJhengHei")
                return "Microsoft JhengHei";
            else if (fontName == "MicrosoftJhengHeiLight")
                return "Microsoft JhengHei Light";
            else if (fontName == "MicrosoftJhengHeiUI")
                return "Microsoft JhengHei UI";
            else if (fontName == "MicrosoftJhengHeiUILight")
                return "Microsoft JhengHei UI Light";
            else if (fontName == "MicrosoftMHei")
                return "Microsoft MHei";
            else if (fontName == "MicrosoftNeoGothic")
                return "Microsoft NeoGothic";
            else if (fontName == "MalgunGothic")
                return "Malgun Gothic";
            else if (fontName == "AgencyFB")
                return "Agency FB";
            else if (fontName == "Bauhaus93")
                return "Bauhaus 93";
            else if (fontName == "BellMT")
                return "Bell MT";
            else if (fontName == "BerlinSansFB")
                return "Berlin Sans FB";
            else if (fontName == "BerlinSansFBDemi")
                return "Berlin Sans FB Demi";
            else if (fontName == "BernardMTCondensed")
                return "Bernard MT Condensed";
            else if (fontName == "BlackadderITC")
                return "Blackadder ITC";
            else if (fontName == "BodoniMT")
                return "Bodoni MT";
            else if (fontName == "BodoniMTBlack")
                return "Bodoni MT Black";
            else if (fontName == "YuGothic")
                return "Yu Gothic";
            else
                return fontName;
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
                string cellRange = CellRange.Get(context);
                double rowHeight = RowHeight.Get(context);
                double colWidth = ColWidth.Get(context);
                int fontSize = FontSize.Get(context);
                var sheetIndex = SheetIndex.Get(context);
                string sheetName = SheetName.Get(context);

                Excel.Worksheet sheet = excelApp.ActiveSheet;
                try
                {
                    if (sheetIndex.HasValue)
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


                Excel::Range range = cellRange.IsNullOrWhiteSpace() ? sheet.Cells : sheet.Range[cellRange];

                /*对齐设置*/
                if ((int)_AlignStyle != 0)
                    range.HorizontalAlignment = (AlignEnum)_AlignStyle;

                /*字体*/
                range.Font.Bold = isBold;
                range.Font.Italic = isItalic;
                range.Font.Underline = isUnderLine;
                if (Font != 0)
                    range.Font.Name = ConvertFont(Font.ToString());
                range.Font.Size = fontSize;

                if ((int)_FontColor != 0)
                    range.Font.ColorIndex = (int)_FontColor;

                /*填充色*/
                if ((int)_CellColor != 0)
                    range.Interior.ColorIndex = (int)_CellColor;

                /*行列宽度*/
                range.RowHeight = rowHeight;
                range.ColumnWidth = colWidth;

                /*边框*/
                if ((int)_BorderStyle != 0)
                {
                    switch ((int)_BorderType)
                    {
                        case 0:
                            {
                                range.Borders.LineStyle = (int)_BorderStyle;
                                break;
                            }
                        case 1:
                            {
                                range.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = (int)_BorderStyle;
                                break;
                            }
                        case 2:
                            {
                                range.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = (int)_BorderStyle;
                                break;
                            }
                        case 3:
                            {
                                range.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = (int)_BorderStyle;
                                break;
                            }
                        case 4:
                            {
                                range.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = (int)_BorderStyle;
                                break;
                            }
                        default:
                            break;
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
