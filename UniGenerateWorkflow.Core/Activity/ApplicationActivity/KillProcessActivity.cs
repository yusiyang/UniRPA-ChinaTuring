using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 杀死进程活动
    /// </summary>
    public class KillProcessActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认活动 
        /// </summary>
        private const string DisplayName = "杀死进程";

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
            var killProcessActivity = new ApplicationActivity.KillProcessActivity();
            ((System.Activities.Activity)killProcessActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(killProcessActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(killProcessActivity.DisplayName):
                            ((System.Activities.Activity)killProcessActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(killProcessActivity.ContinueOnError):
                            killProcessActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(killProcessActivity.Processes):
                            killProcessActivity.Processes = new InArgument<System.Diagnostics.Process> { Expression = new VisualBasicValue<System.Diagnostics.Process>(CreateVariable<System.Diagnostics.Process>(variables, key, value)) };
                            break;
                        case nameof(killProcessActivity.ProcessName):
                            killProcessActivity.ProcessName = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                    }
                }
            }
            //设置未赋值但需要赋值的属性
            var unrecognizedParameters = activityDescription.UnrecognizedParameters;
            SetProcessNameProperty(killProcessActivity, unrecognizedParameters);

            return killProcessActivity;
        }

        /// <summary>
        /// 设置ProcessName属性
        /// </summary>
        /// <param name="killProcessActivity"></param>
        /// <param name="unrecognizedParameters"></param>
        private void SetProcessNameProperty(ApplicationActivity.KillProcessActivity killProcessActivity, List<string> unrecognizedParameters)
        {
            if (killProcessActivity.ProcessName != null || unrecognizedParameters == null)
            {
                return;
            }
            for (var i = unrecognizedParameters.Count - 1; i >= 0; i--)
            {
                killProcessActivity.ProcessName = unrecognizedParameters[i];
                unrecognizedParameters.RemoveAt(i);
                break;
            }
        }
    }
}
