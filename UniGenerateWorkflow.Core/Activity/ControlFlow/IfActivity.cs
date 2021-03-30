using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// If活动
    /// </summary>
    public class IfActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "If判断";

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
            var ifActivity = new If();
            ((System.Activities.Activity)ifActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(ifActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(ifActivity.DisplayName):
                            ((System.Activities.Activity)ifActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(ifActivity.Condition):
                            ifActivity.Condition = new InArgument<bool> { Expression = new VisualBasicValue<bool>(CreateVariable<bool>(variables, key, value)) };
                            break;
                        case nameof(ifActivity.Then):
                            ifActivity.Then = GetSequence(activityVariableContext, activityFactory, variables, key, value);
                            break;
                        case nameof(ifActivity.Else):
                            ifActivity.Else = GetSequence(activityVariableContext, activityFactory, variables, key, value);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return ifActivity;
        }
    }
}
