using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// While循环活动
    /// </summary>
    public class WhileActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "循环";

        /// <summary>
        /// 是否含有属性
        /// </summary>
        public override bool IsHasSelector => false;

        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="activityProperties"></param>
        /// <returns></returns>
        public Activity CreateInstance(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables, ActivityFactory activityFactory = null)
        {
            variables = new List<Variable>();
            var whileActivity = new System.Activities.Statements.While();
            ((System.Activities.Activity)whileActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(whileActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(whileActivity.DisplayName):
                            ((System.Activities.Activity)whileActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(whileActivity.Condition):
                            whileActivity.Condition = new VisualBasicValue<bool>(CreateVariable<bool>(variables, key, value));
                            break;
                        case nameof(whileActivity.Body):
                            whileActivity.Body = GetSequence(activityVariableContext, activityFactory, variables, key, value);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return whileActivity;
        }
    }
}
