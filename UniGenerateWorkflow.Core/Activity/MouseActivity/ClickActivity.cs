using Microsoft.VisualBasic.Activities;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 单击活动
    /// </summary>
    public class ClickActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "单击活动";

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
            var clickActivity = new MouseActivity.ClickActivity();
            ((System.Activities.Activity)clickActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(clickActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(clickActivity.DisplayName):
                            ((System.Activities.Activity)clickActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(clickActivity.ContinueOnError):
                            clickActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(clickActivity.ClickType):
                            clickActivity.ClickType = Convert.ToInt32(value);
                            break;
                        case nameof(clickActivity.Element):
                            clickActivity.Element = new InArgument<UiElement> { Expression = new VisualBasicValue<UiElement>(CreateVariable<UiElement>(variables, key, value)) };
                            break;
                        case nameof(clickActivity.ElementPosition):
                            clickActivity.ElementPosition = Convert.ToInt32(value.ToString());
                            break;
                        case nameof(clickActivity.KeyModifiers):
                            clickActivity.KeyModifiers = value.ToString();
                            break;
                        case nameof(clickActivity.MouseButton):
                            clickActivity.MouseButton = Convert.ToInt32(value.ToString());
                            break;
                        case nameof(clickActivity.offsetX):
                            clickActivity.offsetX = Convert.ToInt32(value.ToString());
                            break;
                        case nameof(clickActivity.offsetY):
                            clickActivity.offsetY = Convert.ToInt32(value.ToString());
                            break;
                        case nameof(clickActivity.Selector):
                            clickActivity.Selector = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(clickActivity.SendWindowMessage):
                            clickActivity.SendWindowMessage = (bool)value;
                            break;
                        case nameof(clickActivity.SimulateSingleClick):
                            clickActivity.SimulateSingleClick = (bool)value;
                            break;
                        case nameof(clickActivity.SourceImgPath):
                            clickActivity.SourceImgPath = value.ToString();
                            break;
                        case nameof(clickActivity.Timeout):
                            clickActivity.Timeout = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(clickActivity.visibility):
                            clickActivity.visibility = System.Windows.Visibility.Visible;
                            break;
                    }
                }
            }
            return clickActivity;
        }
    }
}
