using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Uni.Core
{
    /// <summary>
    /// 开启进程活动
    /// </summary>
    public class StartProcessActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认活动 
        /// </summary>
        private const string DisplayName = "开启进程";

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
            var startProcessActivity = new ApplicationActivity.StartProcessActivity();
            ((System.Activities.Activity)startProcessActivity).DisplayName = DisplayName;
            var parameters = activityDescription.UnrecognizedParameters;

            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(startProcessActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(startProcessActivity.DisplayName):
                            ((System.Activities.Activity)startProcessActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(startProcessActivity.ContinueOnError):
                            startProcessActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(startProcessActivity.Arguments):
                            startProcessActivity.Arguments = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(startProcessActivity.FileName):
                            startProcessActivity.FileName = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(startProcessActivity.WorkingDirectory):
                            startProcessActivity.WorkingDirectory = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                    }
                }
            }

            //设置未赋值但需要赋值的属性
            var unrecognizedParameters = activityDescription.UnrecognizedParameters;
            SetFileNameProperty(startProcessActivity, unrecognizedParameters);

            return startProcessActivity;
        }

        /// <summary>
        /// 设置FileName属性
        /// </summary>
        /// <param name="startProcessActivity"></param>
        /// <param name="unrecognizedParameters"></param>
        private void SetFileNameProperty(ApplicationActivity.StartProcessActivity startProcessActivity, List<string> unrecognizedParameters)
        {
            if (startProcessActivity.FileName != null || unrecognizedParameters == null)
            {
                return;
            }
            for (var i = unrecognizedParameters.Count - 1; i >= 0; i--)
            {
                var match = Regex.Match(unrecognizedParameters[i], FilePattern);
                if (!match.Success)
                {
                    continue;
                }

                startProcessActivity.FileName = match.Value ?? "";
                unrecognizedParameters.RemoveAt(i);
                break;
            }
        }

    }
}
