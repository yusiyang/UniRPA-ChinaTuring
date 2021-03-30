using System;
using System.Activities;
using System.Activities.DynamicUpdate;
using System.Activities.Presentation.Metadata;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using ControlActivity.Designer;

namespace ControlActivity
{
    /// <summary>
    ///   为 <see cref="P:System.Activities.Statements.ForEach`1.Values" /> 集合中提供的每个值执行活动操作一次。
    /// </summary>
    [Designer(typeof(ForEachDesigner))]
    [ContentProperty("Body")]
    public sealed class NonGenericForEach<T> : NativeActivity
    {
        private Variable<IEnumerator> valueEnumerator;
        private CompletionCallback onChildComplete;
        static NonGenericForEach()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            Type attrType = Type.GetType("System.Activities.Presentation.FeatureAttribute, System.Activities.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            Type argType = Type.GetType("System.Activities.Presentation.UpdatableGenericArgumentsFeature, System.Activities.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            builder.AddCustomAttributes(typeof(NonGenericForEach<>), new Attribute[] { Activator.CreateInstance(attrType, new object[] { argType }) as Attribute });
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
        /// <summary>
        ///   创建 <see cref="T:System.Activities.Statements.ForEach`1" /> 类的新实例。
        /// </summary>
        public NonGenericForEach()
        {
            this.valueEnumerator = new Variable<IEnumerator>();
            Body = new ActivityAction<T>
            {
                DisplayName = "Action",
                Argument = new DelegateInArgument<T>()
                {
                    Name = "item"
                }
            };
        }

        [Browsable(false)]
        public string IcoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/System/ForEach.png";
            }
        }

        /// <summary>
        ///   要为集合中的每一项执行的。
        /// </summary>
        /// <returns>要执行的操作。</returns>
        [DefaultValue(null)]
        public ActivityAction<T> Body { get; set; }

        /// <summary>
        ///   活动的输入集合，用于执行 <see cref="P:System.Activities.Statements.ForEach`1.Body" /> 活动操作。
        /// </summary>
        /// <returns>值的集合。</returns>
        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<IEnumerable> Values { get; set; }

        private CompletionCallback OnChildComplete
        {
            get
            {
                if (this.onChildComplete == null)
                {
                    this.onChildComplete = new CompletionCallback(GetStateAndExecute);
                }

                return this.onChildComplete;
            }
        }

        protected override void OnCreateDynamicUpdateMap(
          NativeActivityUpdateMapMetadata metadata,
          System.Activities.Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument runtimeArgument = new RuntimeArgument("Values", typeof(IEnumerable), ArgumentDirection.In, true);
            metadata.Bind((Argument)this.Values, runtimeArgument);
            metadata.AddArgument(runtimeArgument);
            metadata.AddDelegate((ActivityDelegate)this.Body);
            metadata.AddImplementationVariable((Variable)this.valueEnumerator);
            
        }

        protected override void Execute(NativeActivityContext context)
        {
            IEnumerable objs = this.Values.Get((ActivityContext)context);
            if (objs == null)
                throw new InvalidOperationException(this.DisplayName);
            IEnumerator enumerator = objs.GetEnumerator();
            this.valueEnumerator.Set(context, enumerator);
            if (this.Body == null || this.Body.Handler == null)
            {
                while (enumerator.MoveNext())
                {
                    // do nothing                
                };
                enumerator.Reset();
                return;
            }
            this.InternalExecute(context, (ActivityInstance)null, enumerator);
        }

        void GetStateAndExecute(NativeActivityContext context, ActivityInstance completedInstance)
        {
            IEnumerator valueEnumerator = this.valueEnumerator.Get(context);
            InternalExecute(context, completedInstance, valueEnumerator);
        }

        void InternalExecute(NativeActivityContext context, ActivityInstance completedInstance, IEnumerator valueEnumerator)
        {
            if (!valueEnumerator.MoveNext())
            {
                if (completedInstance != null)
                {
                    if (completedInstance.State == ActivityInstanceState.Canceled ||
                        (context.IsCancellationRequested && completedInstance.State == ActivityInstanceState.Faulted))
                    {
                        context.MarkCanceled();
                    }
                }
                valueEnumerator.Reset();
                return;
            }

            // After making sure there is another value, let's check for cancelation
            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
                valueEnumerator.Reset();
                return;
            }

            var current = (T)valueEnumerator.Current;
            context.ScheduleAction(this.Body,current , this.OnChildComplete);
        }
    }
}
