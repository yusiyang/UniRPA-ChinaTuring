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
    [Designer(typeof(RemoveDataColumnDesigner))]
    public sealed class RemoveDataColumn : CodeActivity
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
        [Description("要从中删除列的 DataTable 对象。")]
        public InArgument<DataTable> DataTable { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("DataColumn")]
        [DisplayName("数据列")]
        [Description("要从 DataTable 的列集合中删除的 DataColumn 对象。如果设置了此属性，则忽略其它两个列索引选项。")]
        public InArgument<DataColumn> DataColumn { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("ColumnIndex")]
        [DisplayName("列索引")]
        [Description("要从 DataTable 的列集合中删除的列的索引。如果设置了此属性，则忽略其它两个列索引选项。")]
        public InArgument<Int32> ColumnIndex { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("ColumnName")]
        [DisplayName("列名称")]
        [Description("要从 DataTable 的列集合中删除的列的名称。如果设置了此属性，则忽略其它两个列索引选项。必须将文本放入引号中。")]
        public InArgument<string> ColumnName { get; set; }

        #endregion


        #region 属性分类：杂项

        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataTable/remove-data-column.png";
            }
        }

        #endregion


        public RemoveDataColumn()
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

                if (dataColumn != null)
                    dataTable.Columns.Remove(dataColumn);
                else if (columnName == null || columnName == "")
                    dataTable.Columns.RemoveAt(columnIndex);
                else
                    dataTable.Columns.Remove(columnName);
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