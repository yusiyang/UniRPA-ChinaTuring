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
    [Designer(typeof(RemoveDataRowDesigner))]
    public sealed class RemoveDataRow : CodeActivity
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
        [Description("要从中删除行的 DataTable 对象。")]
        public InArgument<DataTable> DataTable { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("DataRow")]
        [DisplayName("数据行")]
        [Description("要删除的 DataRow 对象。如果设置了此属性，则忽略行索引选项。")]
        public InArgument<DataRow> DataRow { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("RowIndex")]
        [DisplayName("行索引")]
        [Description("要删除的行的索引。如果设置了此属性，则忽略 DataRow 索引选项。")]
        public InArgument<Int32> RowIndex { get; set; }

        #endregion


        #region 属性分类：杂项

        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataTable/remove-data-row.png";
            }
        }

        #endregion


        public RemoveDataRow()
        {

        }

        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                DataRow dataRow = DataRow.Get(context);
                Int32 rowIndex = RowIndex.Get(context);
                if (dataRow == null)
                    DataTable.Get(context).Rows.RemoveAt(rowIndex);
                else
                    DataTable.Get(context).Rows.Remove(dataRow);
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