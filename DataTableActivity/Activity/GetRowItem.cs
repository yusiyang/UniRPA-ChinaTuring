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
    [Designer(typeof(GetRowItemDesigner))]
    public sealed class GetRowItem : AsyncCodeActivity
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
        [DisplayName("数据行")]
        [Description("要从中检索值的 DataRow 对象。")]
        public InArgument<DataRow> DataRow { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("DataColumn")]
        [DisplayName("数据列")]
        [Description("要从 DataRow 检索其值的 DataColumn 对象。如果设置了此属性，则忽略 ColumnName 和 ColumnIndex 属性。")]
        public InArgument<DataColumn> DataColumn { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("ColumnIndex")]
        [DisplayName("列索引")]
        [Description("要从 DataRow 检索其值的列的索引。")]
        public InArgument<Int32> ColumnIndex { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("ColumnNames")]
        [DisplayName("列名称")]
        [Description("要从 DataRow 检索其值的列的名称。如果设置了此属性，则忽略 ColumnIndex 属性。必须将文本放入引号中。")]
        public InArgument<string> ColumnName { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("值")]
        [Description("指定 DataRow 的列值")]
        public OutArgument<object> Value { get; set; }

        #endregion


        #region 属性分类：杂项

        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataTable/get-row-item.png";
            }
        }

        #endregion


        public GetRowItem()
        {

        }

        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return DisplayName;
        }


        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            DataRow dataRow = DataRow.Get(context);
            DataColumn dataColumn = DataColumn.Get(context);
            Int32 columnIndex = ColumnIndex.Get(context);
            string columnName = ColumnName.Get(context);
            object value = null;
            try
            {
                if(dataColumn != null)
                {
                    value = dataRow[dataColumn];
                }
                else if(columnName != null && columnName != "")
                {
                    value = dataRow[columnName];
                }
                else
                {
                    value = dataRow[columnIndex];
                }
                Value.Set(context, value);
            }
            catch (Exception e)
            {
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

        }
    }
}