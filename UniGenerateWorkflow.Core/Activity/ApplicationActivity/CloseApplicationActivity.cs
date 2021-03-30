using System;
using System.Activities;
using System.Collections.Generic;
using Microsoft.VisualBasic.Activities;

namespace Uni.Core
{
    /// <summary>
    /// 关闭应用程序活动
    /// </summary>
    public class CloseApplicationActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "关闭应用程序";

        public override bool IsHasSelector => true;

        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public Activity CreateInstance(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables, ActivityFactory activityFactory = null)
        {
            variables = new List<Variable>();
            var closeApplicationActivity = new ApplicationActivity.CloseApplicationActivity();
            ((System.Activities.Activity)closeApplicationActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(closeApplicationActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(closeApplicationActivity.DisplayName):
                            ((System.Activities.Activity)closeApplicationActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(closeApplicationActivity.ContinueOnError):
                            closeApplicationActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(closeApplicationActivity.ProcessName):
                            closeApplicationActivity.ProcessName = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(closeApplicationActivity.Selector):
                            closeApplicationActivity.Selector = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(closeApplicationActivity.SourceImgPath):
                            closeApplicationActivity.SourceImgPath = value.ToString();
                            break;
                        case nameof(closeApplicationActivity.visibility):
                            closeApplicationActivity.visibility = System.Windows.Visibility.Visible;
                            break;
                    }
                }
            }
            //设置未赋值但需要赋值的属性
            var unrecognizedParameters = activityDescription.UnrecognizedParameters;
            SetProcessNameProperty(closeApplicationActivity, unrecognizedParameters);
            return closeApplicationActivity;
        }

        /// <summary>
        /// 设置ProcessName属性
        /// </summary>
        /// <param name="closeApplicationActivity"></param>
        /// <param name="unrecognizedParameters"></param>
        private void SetProcessNameProperty(ApplicationActivity.CloseApplicationActivity closeApplicationActivity, List<string> unrecognizedParameters)
        {
            if (closeApplicationActivity.ProcessName != null || unrecognizedParameters == null)
            {
                return;
            }
            for (var i = unrecognizedParameters.Count - 1; i >= 0; i--)
            {
                closeApplicationActivity.ProcessName = unrecognizedParameters[i];
                unrecognizedParameters.RemoveAt(i);
                break;
            }
        }
    }
}
