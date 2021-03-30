using System;
using System.Activities;
using System.Activities.Validation;
using System.Collections.Generic;
using System.ComponentModel;

namespace ControlActivity
{
    internal class LoopChildConstraint : NativeActivity<bool>
    {
        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<IEnumerable<Activity>> ParentChain
        {
            get;
            set;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("ParentChain", typeof(IEnumerable<Activity>), ArgumentDirection.In, isRequired: true);
            metadata.Bind(ParentChain, argument);
            metadata.AddArgument(argument);
        }

        protected override void Execute(NativeActivityContext context)
        {
            foreach (Activity item in ParentChain.Get(context))
            {
                if (IsLoopActivity(item))
                {
                    return;
                }
            }
            //Constraint.AddValidationError(context, new ValidationError(ForEachValidationError, isWarning: false));
        }

        private bool IsLoopActivity(Activity act)
        {
            if (act == null)
            {
                return false;
            }
            Type type = act.GetType();
            if (!typeof(InterruptibleLoopBase).IsAssignableFrom(type))
            {
                //return type.BaseType?.FullName == typeof(BusinessForEach).FullName;
            }
            return true;
        }
    }
}
