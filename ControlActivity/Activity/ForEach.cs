using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlActivity.Designer;

namespace ControlActivity
{
    [Designer(typeof(ForEachDesigner))]
    public class ForEach<T> : InterruptibleLoopBase
    {
        private PropertyDescriptor _currentValueDescriptor;

        protected Variable<IEnumerator> valueEnumerator;

        private CompletionCallback _onChildComplete;

        [Browsable(false)]
        public ActivityAction<T> Body { get; set; }
        [Category("输出")]
        [DisplayName("CurrentIndex")]
        [Description("执行循环时的当前索引")]
        public OutArgument<int> CurrentIndex { get; set; }

        private Variable<int> IndexVariable { get; set; }
        private Variable<object> CurrentValue { get; set; }
        [DefaultValue(null)]
        [DisplayName("Values")]
        public InArgument<IEnumerable> Values { get; set; }

        [Browsable(false)]
        public string IcoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/System/ForEach.png";
            }
        }

        static ForEach()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            Type attrType = Type.GetType("System.Activities.Presentation.FeatureAttribute, System.Activities.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            Type argType = Type.GetType("System.Activities.Presentation.UpdatableGenericArgumentsFeature, System.Activities.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            builder.AddCustomAttributes(typeof(ForEach<>), new Attribute[] { Activator.CreateInstance(attrType, new object[] { argType }) as Attribute });
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
        public ForEach()
        {
            IndexVariable = new Variable<int>();
            valueEnumerator = new Variable<IEnumerator>();
            CurrentValue = new Variable<object>();

            Body = new ActivityAction<T>
            {
                DisplayName = "Action",
                Argument = new DelegateInArgument<T>()
                {
                    Name = "item"
                }
            };
        }
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            CurrentValue.Name = (Body?.Argument?.Name ?? "CurrentValue");
            RuntimeArgument argument = new RuntimeArgument("Values", typeof(IEnumerable), ArgumentDirection.In, isRequired: false);
            metadata.Bind(Values, argument);
            RuntimeArgument argument2 = new RuntimeArgument("CurrentIndex", typeof(int), ArgumentDirection.Out);
            metadata.Bind(CurrentIndex, argument2);
            metadata.AddArgument(argument);
            metadata.AddArgument(argument2);
            metadata.AddDelegate(Body);
            metadata.AddVariable(CurrentValue);
            metadata.AddImplementationVariable(IndexVariable);
            metadata.AddImplementationVariable(valueEnumerator);
        }

        protected virtual void SetValues(NativeActivityContext context)
        {
        }
        private CompletionCallback OnChildComplete
        {
            get
            {
                if (_onChildComplete == null)
                {
                    _onChildComplete = GetStateAndExecute;
                }
                return _onChildComplete;
            }
        }
        protected override void StartLoop(NativeActivityContext context)
        {
            SetValues(context);
            IEnumerator enumerator = Values.Get(context).GetEnumerator();
            valueEnumerator.Set(context, enumerator);
            if (Body?.Handler == null)
            {
                OnForEachComplete(enumerator);
                return;
            }
            _currentValueDescriptor = context.DataContext.GetProperties().Find(CurrentValue.Name, ignoreCase: false);
            InternalExecute(context, enumerator);
        }

        private void GetStateAndExecute(NativeActivityContext context, ActivityInstance completedInstance)
        {
            IEnumerator enumerator = valueEnumerator.Get(context);
            InternalExecute(context, enumerator);
        }

        private void InternalExecute(NativeActivityContext context, IEnumerator valueEnumerator)
        {
            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
                breakRequested = true;
            }
            if (breakRequested || !valueEnumerator.MoveNext())
            {
                OnForEachComplete(valueEnumerator);
                return;
            }
            int num = IndexVariable.Get(context);
            CurrentIndex?.Set(context, num);
            IndexVariable.Set(context, num + 1);
            _currentValueDescriptor.SetValue(context.DataContext, valueEnumerator.Current);
            context.ScheduleAction(Body, (T)valueEnumerator.Current, OnChildComplete);
        }
        private void OnForEachComplete(IEnumerator valueEnumerator)
        {
            (valueEnumerator as IDisposable)?.Dispose();
        }
    }
}
