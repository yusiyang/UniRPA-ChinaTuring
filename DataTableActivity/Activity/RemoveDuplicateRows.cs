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
    [Designer(typeof(RemoveDuplicateRowsDesigner))]
    public sealed class RemoveDuplicateRows : CodeActivity
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
        [Description("要从中删除重复行的 DataTable 变量。")]
        public InArgument<DataTable> DataTable { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [RequiredArgument]
        [DisplayName("数据表")]
        [Description("输出已删除重复行的 DataTable，存储在 DataTable 变量中。放置与 Input 字段中的变量相同的变量会更改初始变量，而提供新变量会使初始变量不受影响。")]
        public OutArgument<DataTable> OutDataTable { get; set; }

        #endregion


        #region 属性分类：杂项

        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataTable/remove-duplicate-rows.png";
            }
        }

        #endregion


        public RemoveDuplicateRows()
        {

        }

        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                DataTable dataTable = DataTable.Get(context);
                DataView dataView = dataTable.AsDataView();
                DataTable outTable = dataView.ToTable(true);
                OutDataTable.Set(context, outTable);
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