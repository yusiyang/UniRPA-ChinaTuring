using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using Uni.Common;

namespace Uni.Core
{
    /// <summary>
    /// ForEach循环活动
    /// </summary>
    public class ForEachActivity : ActivityBase, IActivity
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
        /// <param name="properties"></param>
        /// <returns></returns>
        public Activity CreateInstance(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables, ActivityFactory activityFactory = null)
        {
            variables = new List<Variable>();
            var forEachActivity = new ForEach<object>();
            ((System.Activities.Activity)forEachActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(forEachActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(forEachActivity.DisplayName):
                            ((System.Activities.Activity)forEachActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(forEachActivity.Body):
                            forEachActivity.Body = GetActivityAction<object>(activityVariableContext, activityFactory, variables, key, value);
                            break;
                        case nameof(forEachActivity.Values):
                            var variable = activityVariableContext.Get(value.ToString()) ?? CreateVariable<IEnumerable<object>>(variables, key);
                            forEachActivity.Values = new VisualBasicValue<IEnumerable<object>>(variable);
                            break;
                    }
                }
            }
            return forEachActivity;
        }

        /// <summary>
        /// 创建变量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variables"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override string CreateVariable<T>(List<Variable> variables, string key, object value = null)
        {
            var variableKey = $"{key}_{Utility.GetRandomNumber(RandomNumberLength)}";
            if (value == null)
            {
                variables.Add(new Variable<T>(variableKey));
            }
            else
            {
                var variable = new Variable<T>(variableKey) { Default = new VisualBasicValue<T>("{" + string.Join(",", (T)value) + "}") };
                variables.Add(variable);
            }
            return variableKey;
        }
    }
}
