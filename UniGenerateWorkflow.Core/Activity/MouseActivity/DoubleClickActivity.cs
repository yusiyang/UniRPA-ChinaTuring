using Microsoft.VisualBasic.Activities;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    public class DoubleClickActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "双击活动";

        /// <summary>
        /// 是否含有属性
        /// </summary>
        public override bool IsHasSelector => true;

        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        public Activity CreateInstance(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables, ActivityFactory activityFactory = null)
        {
            variables = new List<Variable>();
            var doubleClickActivity = new MouseActivity.DoubleClickActivity();
            ((System.Activities.Activity)doubleClickActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(doubleClickActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(doubleClickActivity.DisplayName):
                            ((System.Activities.Activity)doubleClickActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(doubleClickActivity.ContinueOnError):
                            doubleClickActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(doubleClickActivity.ClickType):
                            doubleClickActivity.ClickType = (MouseActivity.MouseClickType)value;
                            break;
                        case nameof(doubleClickActivity.Element):
                            doubleClickActivity.Element = new InArgument<UiElement> { Expression = new VisualBasicValue<UiElement>(CreateVariable<UiElement>(variables, key, value)) };
                            break;
                        case nameof(doubleClickActivity.ElementPosition):
                            doubleClickActivity.ElementPosition = Convert.ToInt32(value.ToString());
                            break;
                        case nameof(doubleClickActivity.KeyModifiers):
                            doubleClickActivity.KeyModifiers = value.ToString();
                            break;
                        case nameof(doubleClickActivity.MouseButton):
                            doubleClickActivity.MouseButton = Convert.ToInt32(value.ToString());
                            break;
                        case nameof(doubleClickActivity.offsetX):
                            doubleClickActivity.offsetX = Convert.ToInt32(value.ToString());
                            break;
                        case nameof(doubleClickActivity.offsetY):
                            doubleClickActivity.offsetY = Convert.ToInt32(value.ToString());
                            break;
                        case nameof(doubleClickActivity.Selector):
                            doubleClickActivity.Selector = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(doubleClickActivity.SendWindowMessage):
                            doubleClickActivity.SendWindowMessage = (bool)value;
                            break;
                        case nameof(doubleClickActivity.SimulateSingleClick):
                            doubleClickActivity.SimulateSingleClick = (bool)value;
                            break;
                        case nameof(doubleClickActivity.SourceImgPath):
                            doubleClickActivity.SourceImgPath = value.ToString();
                            break;
                        case nameof(doubleClickActivity.Timeout):
                            doubleClickActivity.Timeout = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(doubleClickActivity.visibility):
                            doubleClickActivity.visibility = System.Windows.Visibility.Visible;
                            break;
                    }
                }
            }

            return doubleClickActivity;
        }
    }
}
