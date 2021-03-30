using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.Activities;

namespace Uni.Core
{
    /// <summary>
    /// 赋值活动
    /// </summary>
    public class AssignActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "赋值";

        /// <summary>
        /// 是否含有属性
        /// </summary>
        public override bool IsHasSelector => false;

        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="activityProperties">属性属性值映射</param>
        /// <returns></returns>
        public Activity CreateInstance(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables, ActivityFactory activityFactory = null)
        {
            variables = new List<Variable>();
            var assignActivity = new Assign();
            ((System.Activities.Activity)assignActivity).DisplayName = DisplayName;
            var dic = new Dictionary<string, ArgumentTypeEnum>();
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(assignActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(assignActivity.DisplayName):
                            ((System.Activities.Activity)assignActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(assignActivity.Value):
                            assignActivity.Value = new InArgument<object> { Expression = new VisualBasicValue<object>(CreateVariable<object>(variables, key, value)) };
                            break;
                        case nameof(assignActivity.To):
                            assignActivity.To = new OutArgument<object> { Expression = new VisualBasicValue<Location<object>>(CreateOurArgument<object>(activityVariableContext, variables, key, value)) };
                            break;
                    }
                }
            }

            //设置未赋值但需要赋值的属性
            var unrecognizedParameters = activityDescription.UnrecognizedParameters;
            SetValueProperty(activityVariableContext, assignActivity, unrecognizedParameters);
            SetToProperty(activityVariableContext, assignActivity, variables, dic);
            activityVariableContext.Activities.AddLast(new Uni.Core.ActivityContext { Activity = assignActivity, Variables = dic });
            return assignActivity;
        }

        /// <summary>
        /// 设置Value属性
        /// </summary>
        /// <param name="activityVariableContext"></param>
        /// <param name="assignActivity"></param>
        /// <param name="unrecognizedParameters"></param>
        private void SetValueProperty(ActivityVariableContext activityVariableContext, Assign assignActivity, List<string> unrecognizedParameters)
        {
            if (assignActivity.Value != null || unrecognizedParameters == null || activityVariableContext.Activities.Last == null)
            {
                return;
            }
            for (int i = unrecognizedParameters.Count - 1; i >= 0; i--)
            {
                var activity = activityVariableContext.Activities.Last.Value;
                if (!activity.Variables.ContainsValue(ArgumentTypeEnum.DataRow))
                {
                    break;
                }
                var variable = activity.Variables.First(a => a.Value == ArgumentTypeEnum.DataRow);
                assignActivity.Value = new InArgument<string> { Expression = new VisualBasicValue<string>(variable.Key + "(\"" + unrecognizedParameters[i] + "\").ToString()") };
                unrecognizedParameters.RemoveAt(i);
                break;
            }
        }

        /// <summary>
        /// 设置To属性
        /// </summary>
        /// <param name="activityVariableContext"></param>
        /// <param name="assignActivity"></param>
        /// <param name="variables"></param>
        /// <param name="dic"></param>
        private void SetToProperty(ActivityVariableContext activityVariableContext, Assign assignActivity, List<Variable> variables, Dictionary<string, ArgumentTypeEnum> dic)
        {
            if (assignActivity.To != null)
            {
                return;
            }
            var variable = CreateOurArgument<string>(activityVariableContext, variables, To, To);
            dic.Add(variable, ArgumentTypeEnum.Out);
            assignActivity.To = new OutArgument<string>() { Expression = new VisualBasicValue<Location<string>>(variable) };
        }
    }
}
