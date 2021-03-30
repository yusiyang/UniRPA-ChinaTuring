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
    [Designer(typeof(MergeDataTableDesigner))]
    public sealed class MergeDataTable : CodeActivity
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
        [DisplayName("目标")]
        [Description("合并源 DataTable 的 DataTable 对象。")]
        public InArgument<DataTable> Destination { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("源")]
        [Description("要添加到目标 DataTable 的 DataTable 对象。")]
        public InArgument<DataTable> Source { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("合并操作")]
        [Description("指定合并两个 DataTable 时要执行的操作。")]
        public MissingSchemaAction MergeType { get; set; } = MissingSchemaAction.Add;

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataTable/merge-data-table.png";
            }
        }

        #endregion


        public MergeDataTable()
        {

        }

        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                DataTable destination = Destination.Get(context);
                DataTable source = Source.Get(context);

                destination.Merge(source, true, MergeType);
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