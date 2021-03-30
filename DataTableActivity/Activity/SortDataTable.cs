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
    [Designer(typeof(SortDataTableDesigner))]
    public sealed class SortDataTable : CodeActivity
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
        [Description("要排序的 DataTable 变量。")]
        public InArgument<DataTable> DataTable { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("DataColumn")]
        [DisplayName("列数据")]
        [Description("按其排序的列的变量。该字段仅支持 DataColumn 变量。在此属性字段中设置变量会禁用其他两个属性。")]
        public InArgument<DataColumn> DataColumn { get; set; }

        [Category("输入")]
        [OverloadGroup("ColumnIndex")]
        [RequiredArgument]
        [DisplayName("列索引")]
        [Description("按其排序的列的变量。该字段仅支持 DataColumn 变量。在此属性字段中设置变量会禁用其他两个属性。")]
        public InArgument<Int32> ColumnIndex { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("ColumnName")]
        [DisplayName("列名称")]
        [Description("按其排序的列的变量。该字段仅支持 DataColumn 变量。在此属性字段中设置变量会禁用其他两个属性。必须将文本放入引号中。")]
        public InArgument<string> ColumnName { get; set; }

        #endregion


        #region 属性分类：选项

        public enum SortTypes
        {
            增序,
            降序
        }
        [Category("选项")]
        [RequiredArgument]
        [DisplayName("排序方式")]
        public SortTypes SortType { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [RequiredArgument]
        [DisplayName("数据表")]
        [Description("排序后的DataTable变量。放置与Input字段中的变量相同的变量会更改初始变量，而添加新变量会使初始变量不受影响。")]
        public OutArgument<DataTable> OutDataTable { get; set; }

        #endregion


        #region 属性分类：杂项

        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataTable/sort-data-table.png";
            }
        }

        #endregion


        public SortDataTable()
        {

        }

        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                DataTable dataTable = DataTable.Get(context);
                DataColumn dataColumn = DataColumn.Get(context);
                string columnName = ColumnName.Get(context);
                Int32 columnIndex = ColumnIndex.Get(context);
                string SortColName = null;
                string SortText = null;

                if (dataColumn != null)
                    SortColName = dataColumn.ColumnName;
                else if (columnName != null && columnName != "")
                    SortColName = columnName;
                else
                    SortColName = dataTable.Columns[columnIndex].ColumnName;

                SortText = SortType == SortTypes.增序 ? SortColName + " ASC" : SortColName + " DESC";
                dataTable.DefaultView.Sort = SortText;
                DataTable dtNew = dataTable.DefaultView.ToTable();
                OutDataTable.Set(context, dtNew);
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