using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.ComponentModel;
using System.Data;
using System.Threading;

namespace DataTableActivity
{
    [Designer(typeof(LookupDataTableDesigner))]
    public sealed class LookupDataTable : CodeActivity
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


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("数据表")]
        [Description("要在其中执行查找的 DataTable 变量。")]
        public InArgument<DataTable> DataTable { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("查找值")]
        [Description("要在指定 DataTable 变量中搜索的值。若查找的值为字符串类型，则必须将文本放入引号中。")]
        public InArgument<object> LookupValue { get; set; }

        #endregion


        #region 属性分类：开始列

        [Category("开始列")]
        [RequiredArgument]
        [OverloadGroup("DataColumn")]
        [DisplayName("数据列")]
        [Description("要从 DataRow 检索其值的 DataColumn 对象。在此属性字段中设置变量会禁用其他两个属性。")]
        public InArgument<DataColumn> DataColumn { get; set; }

        [Category("开始列")]
        [RequiredArgument]
        [OverloadGroup("ColumnIndex")]
        [DisplayName("列索引")]
        [Description("要从 DataRow 检索其值的列的索引。在此属性字段中设置变量会禁用其他两个属性。")]
        public InArgument<Int32> ColumnIndex { get; set; }

        [Category("开始列")]
        [RequiredArgument]
        [OverloadGroup("ColumnNames")]
        [DisplayName("列名称")]
        [Description("要从 DataRow 检索其值的列的名称。在此属性字段中设置变量会禁用其他两个属性。必须将文本放入引号中。")]
        public InArgument<string> ColumnName { get; set; }

        #endregion


        #region 属性分类：结束列

        [Category("结束列")]
        [OverloadGroup("TargetDataColumn")]
        [DisplayName("数据列")]
        [Description("返回在此列与 RowIndex 属性中的值之间的坐标处找到的单元格。")]
        public InArgument<DataColumn> TargetDataColumn { get; set; }

        [Category("结束列")]
        [OverloadGroup("TargetColumnIndex")]
        [DisplayName("列索引")]
        [Description("返回在此列与 RowIndex 属性值之间的坐标处找到的单元格的列索引。")]
        public InArgument<Int32> TargetColumnIndex { get; set; }

        [Category("结束列")]
        [OverloadGroup("TargetColumnName")]
        [DisplayName("列名称")]
        [Description("返回在此列与 RowIndex 属性中的值之间的坐标处找到的单元格的列名称。必须将文本放入引号中。")]
        public InArgument<string> TargetColumnName { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("单元格值")]
        [Description("单元格中找到的值。")]
        public OutArgument<object> CellValue { get; set; }

        [Category("输出")]
        [DisplayName("行索引")]
        [Description("单元格的 Row 索引。")]
        public OutArgument<Int32> RowIndex { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataTable/lookup-data-table.png";
            }
        }

        #endregion


        public LookupDataTable()
        {

        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        protected override void Execute(CodeActivityContext context)
        {
            DataTable dataTable = DataTable.Get(context);
            string lookupValue = (LookupValue.Get(context)).ToString();
            DataColumn dataColumn = DataColumn.Get(context);
            DataColumn targetDataColumn = TargetDataColumn.Get(context);
            Int32 columnIndex = ColumnIndex.Get(context);
            Int32 targetColumnIndex = ColumnIndex.Get(context);
            string columnName = ColumnName.Get(context);
            string targetColumnName = TargetColumnName.Get(context);

            object cellValue = null;
            Int32 rowIndex = 0;

            try
            {
                int beginIndex = 0, endInex = 0;

                DataColumn beginColumn = new DataColumn();
                if (dataColumn != null) beginIndex = dataTable.Columns.IndexOf(dataColumn);
                else if (columnName != null && columnName != "") beginIndex = dataTable.Columns.IndexOf(columnName);
                else beginIndex = columnIndex;
                if (targetDataColumn != null) endInex = dataTable.Columns.IndexOf(targetDataColumn);
                else if (targetColumnName != null && targetColumnName != "") endInex = dataTable.Columns.IndexOf(targetColumnName);
                else endInex = targetColumnIndex;

                if (endInex == 0)
                {
                    endInex = dataTable.Columns.Count - 1;
                }

                if (beginIndex < 0 || endInex < 0 || beginIndex > endInex)
                {
                    throw new Exception("数据表列索引有误,请检查开始列与结束列");
                }

                DataRowCollection dataRows = dataTable.Rows;
                for (int index = beginIndex; index <= endInex; index++)
                {
                    foreach (DataRow datarow in dataRows)
                    {
                        object data = datarow[index];
                        string dataStr = data.ToString();
                        if (dataStr==lookupValue)
                        {
                            rowIndex = dataRows.IndexOf(datarow);
                            cellValue = data;
                            break;
                        }
                    }

                    if (cellValue!=null)
                    {
                        break;
                    }
                }
                if (CellValue != null)
                    CellValue.Set(context, cellValue);
                if (RowIndex != null)
                    RowIndex.Set(context, rowIndex);
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

        }
    }
}