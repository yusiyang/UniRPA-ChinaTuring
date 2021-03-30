using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.Data;
using System.Threading;

namespace DataTableActivity
{
    [Designer(typeof(AddDataColumnDesigner))]
    public sealed class AddDataColumn<T> : AsyncCodeActivity
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
        [OverloadGroup("Column")]
        [DisplayName("数据列")]
        [Description("要附加到 DataTable 的列集合的 DataColumn 对象。如果设置了此属性，则会忽略“选项”类别下的所有属性。")]
        public InArgument<DataColumn> Column { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("ColumnName")]
        [DisplayName("列名")]
        [Description("新列的名称。必须将文本放入引号中。")]
        public InArgument<string> ColumnName { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("数据表")]
        [Description("要添加列的 DataTable 对象。")]
        public InArgument<DataTable> DataTable { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("允许为空")]
        [Description("指定新列中字段是否允许为空。")]
        public bool AllowNull { get; set; } = true;

        [Category("选项")]
        [DisplayName("自动递增")]
        [Description("指定在添加新行时列的值是否自动递增。")]
        public bool AutoIncrement { get; set; }

        [Category("选项")]
        [DisplayName("唯一约束")]
        [Description("指定新列的每一行中的值必须是唯一的。")]
        public bool Unique { get; set; }

        [Category("选项")]
        [DisplayName("最大长度")]
        [Description("指定新列的值的最大长度。")]
        public Int32 MaxLength { get; set; }

        [Category("选项")]
        [DisplayName("默认值")]
        [Description("指定新列的默认值。")]
        public InArgument<object> DefaultValue { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataTable/add-data-column.png";
            }
        }

        #endregion


        public AddDataColumn()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            Type attrType = Type.GetType("System.Activities.Presentation.FeatureAttribute, System.Activities.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            Type argType = Type.GetType("System.Activities.Presentation.UpdatableGenericArgumentsFeature, System.Activities.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            Type psType = Type.GetType("");
            builder.AddCustomAttributes(typeof(AddDataColumn<>), new Attribute[] { Activator.CreateInstance(attrType, new object[] { argType, }) as Attribute });
            builder.AddCustomAttributes(typeof(AddDataColumn<>), new DefaultTypeArgumentAttribute(typeof(object)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return DisplayName;
        }


        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            Type type = typeof(T);
            DataTable dataTable = DataTable.Get(context);
            DataColumn column = Column.Get(context);
            string columnName = ColumnName.Get(context);
            object defaultValue = DefaultValue.Get(context);
            try
            {
                if (column != null)
                {
                    dataTable.Columns.Add(column);
                }
                else
                {
                    DataColumn newColumn = new DataColumn(columnName);
                    newColumn.DataType = type;
                    if (defaultValue != null)
                        newColumn.DefaultValue = defaultValue;
                    if (MaxLength >= 0)
                        newColumn.MaxLength = MaxLength;
                    newColumn.AllowDBNull = AllowNull;
                    newColumn.Unique = Unique;
                    newColumn.AutoIncrement = AutoIncrement;
                    dataTable.Columns.Add(newColumn);
                }
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