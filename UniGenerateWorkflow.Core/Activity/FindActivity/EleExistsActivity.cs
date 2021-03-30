using Microsoft.VisualBasic.Activities;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 元素是否存在活动
    /// </summary>
    public class EleExistsActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "元素是否存在";

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
            var eleExistsActivity = new FindActivity.EleExists();
            ((System.Activities.Activity)eleExistsActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(eleExistsActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(eleExistsActivity.DisplayName):
                            ((System.Activities.Activity)eleExistsActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(eleExistsActivity.ContinueOnError):
                            eleExistsActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(eleExistsActivity.Element):
                            eleExistsActivity.Element = new InArgument<UiElement> { Expression = new VisualBasicValue<UiElement>(CreateVariable<UiElement>(variables, key, value)) };
                            break;
                        case nameof(eleExistsActivity.IsExist):
                            eleExistsActivity.IsExist = new OutArgument<bool>() { Expression = new VisualBasicValue<Location<bool>>(CreateOurArgument<bool>(activityVariableContext, variables, key, value)) };
                            break;
                        case nameof(eleExistsActivity.Selector):
                            eleExistsActivity.Selector = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(eleExistsActivity.SourceImgPath):
                            eleExistsActivity.SourceImgPath = value.ToString();
                            break;
                        case nameof(eleExistsActivity.Timeout):
                            eleExistsActivity.Timeout = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(eleExistsActivity.visibility):
                            eleExistsActivity.visibility = System.Windows.Visibility.Visible;
                            break;
                    }
                }
            }
            return eleExistsActivity;
        }
    }
}
