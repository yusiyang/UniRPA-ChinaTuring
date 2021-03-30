using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 延迟活动
    /// </summary>
    public class DelayActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "延迟";

        /// <summary>
        /// 是否含有属性
        /// </summary>
        public override bool IsHasSelector => false;

        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public Activity CreateInstance(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables, ActivityFactory activityFactory = null)
        {
            variables = new List<Variable>();
            var delayActivity = new Delay();
            ((System.Activities.Activity)delayActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(delayActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(delayActivity.DisplayName):
                            ((System.Activities.Activity)delayActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(delayActivity.Duration):
                            delayActivity.Duration = new InArgument<TimeSpan>((TimeSpan)value);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return delayActivity;
        }
    }
}
