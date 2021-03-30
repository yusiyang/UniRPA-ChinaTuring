using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 打开应用程序活动
    /// </summary>
    public class OpenApplicationActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认活动 
        /// </summary>
        private const string DisplayName = "打开应用程序";

        /// <summary>
        /// 是否含有属性
        /// </summary>
        public override bool IsHasSelector => true;

        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public Activity CreateInstance(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables, ActivityFactory activityFactory = null)
        {
            variables = new List<Variable>();
            var openApplicationActivity = new ApplicationActivity.OpenApplicationActivity();
            ((System.Activities.Activity)openApplicationActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    switch (key)
                    {
                        case nameof(openApplicationActivity.DisplayName):
                            ((System.Activities.Activity)openApplicationActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(openApplicationActivity.ContinueOnError):
                            openApplicationActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(openApplicationActivity.Arguments):
                            openApplicationActivity.Arguments = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(openApplicationActivity.Body):
                            openApplicationActivity.Body = GetActivityAction<object>(activityVariableContext, activityFactory, variables, key, value);
                            break;
                        case nameof(openApplicationActivity.Selector):
                            openApplicationActivity.Selector = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(openApplicationActivity.SourceImgPath):
                            openApplicationActivity.SourceImgPath = value.ToString();
                            break;
                        case nameof(openApplicationActivity.Timeout):
                            openApplicationActivity.Timeout = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(openApplicationActivity.visibility):
                            openApplicationActivity.visibility = System.Windows.Visibility.Visible;
                            break;
                    }
                }
            }
            return openApplicationActivity;
        }
    }
}
