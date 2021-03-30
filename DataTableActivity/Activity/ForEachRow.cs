using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.Activities.DynamicUpdate;
using System.Activities.Statements;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;

namespace DataTableActivity
{
    [Designer(typeof(ForEachRowDesigner))]
    public sealed class ForEachRow : NativeActivity
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
        [DisplayName("数据表")]
        [Description("执行单行操作的 DataTable 变量。")]
        public InArgument<DataTable> DataTable { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("索引")]
        [Description("当前索引号。")]
        public OutArgument<Int32> CurrentIndex { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public ActivityAction<DataRow> Body { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataTable/for-each-row.png";
            }
        }

        #endregion


        private Variable<IEnumerator<DataRow>> _valueEnumerator;

        private Variable<int> _indexVariable;

        public ForEachRow()
        {
            _valueEnumerator = new Variable<IEnumerator<DataRow>>();
            _indexVariable = new Variable<int>();
            //CurrentIndex = new OutArgument<int>();
            Body = new ActivityAction<DataRow>
            {
                Argument = new DelegateInArgument<DataRow>("row"),
                Handler = new Sequence()
                {
                    DisplayName = "Body"
                }
            };
        }

        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return DisplayName;
        }

        private CompletionCallback onChildComplete;

        private CompletionCallback OnChildComplete
        {
            get
            {
                if (onChildComplete == null)
                {
                    onChildComplete = GetStateAndExecute;
                }
                return onChildComplete;
            }
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("DataTable", typeof(DataTable), ArgumentDirection.In, isRequired: true);
            metadata.Bind(DataTable, argument);
            RuntimeArgument argument1 = new RuntimeArgument("CurrentIndex", typeof(int), ArgumentDirection.Out);
            metadata.Bind(CurrentIndex, argument1);
            metadata.AddArgument(argument);
            metadata.AddArgument(argument1);
            metadata.AddDelegate(Body);
            metadata.AddImplementationVariable(_indexVariable);
            metadata.AddImplementationVariable(_valueEnumerator);
        }

        protected override void Execute(NativeActivityContext context)
        {
            try
            {
                var dataTable = DataTable.Get(context);
                var enumerable = dataTable.AsEnumerable();

                var enumerator = enumerable.GetEnumerator();
                _valueEnumerator.Set(context, enumerator);

                if (Body == null || Body.Handler == null)
                {
                    while (enumerator.MoveNext())
                    {
                    }
                    enumerator.Dispose();
                }
                else
                {
                    InternalExecute(context, null, enumerator);
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
        }

        private void GetStateAndExecute(NativeActivityContext context, ActivityInstance completedInstance)
        {
            var enumerator = _valueEnumerator.Get(context);
            InternalExecute(context, completedInstance, enumerator);
        }

        private void InternalExecute(NativeActivityContext context, ActivityInstance completedInstance, IEnumerator<DataRow> valueEnumerator)
        {

            if (!valueEnumerator.MoveNext())
            {
                if (completedInstance != null && (completedInstance.State == ActivityInstanceState.Canceled || (context.IsCancellationRequested && completedInstance.State == ActivityInstanceState.Faulted)))
                {
                    context.MarkCanceled();
                }
                valueEnumerator.Dispose();
            }
            else if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
                valueEnumerator.Dispose();
            }
            else
            {
                var index = _indexVariable.Get(context);
                CurrentIndex?.Set(context, index);
                _indexVariable.Set(context, index + 1);

                context.ScheduleAction(Body, valueEnumerator.Current, OnChildComplete);
            }
        }
    }
}