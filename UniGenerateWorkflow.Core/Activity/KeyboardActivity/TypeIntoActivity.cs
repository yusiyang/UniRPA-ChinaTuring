using Microsoft.VisualBasic.Activities;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 输入活动
    /// </summary>
    public class TypeIntoActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "输入";

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
            var typeIntoActivity = new KeyboardActivity.TypeIntoActivity
            {
                SimulateInput = true
            };
            ((System.Activities.Activity)typeIntoActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(typeIntoActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(typeIntoActivity.DisplayName):
                            ((System.Activities.Activity)typeIntoActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(typeIntoActivity.ContinueOnError):
                            typeIntoActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(typeIntoActivity.Text):
                            typeIntoActivity.Text = value.ToString();
                            break;
                        case nameof(typeIntoActivity.Activate):
                            typeIntoActivity.Activate = (bool)value;
                            break;
                        case nameof(typeIntoActivity.Element):
                            typeIntoActivity.Element = new InArgument<UiElement> { Expression = new VisualBasicValue<UiElement>(CreateVariable<UiElement>(variables, key, value)) };
                            break;
                        case nameof(typeIntoActivity.EmptyText):
                            typeIntoActivity.EmptyText = (bool)value;
                            break;
                        case nameof(typeIntoActivity.IsRunClick):
                            typeIntoActivity.IsRunClick = (bool)value;
                            break;
                        case nameof(typeIntoActivity.Selector):
                            typeIntoActivity.Selector = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(typeIntoActivity.SimulateInput):
                            typeIntoActivity.SimulateInput = (bool)value;
                            break;
                        case nameof(typeIntoActivity.SourceImgPath):
                            typeIntoActivity.SourceImgPath = value.ToString();
                            break;
                        case nameof(typeIntoActivity.Timeout):
                            typeIntoActivity.Timeout = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(typeIntoActivity.visibility):
                            typeIntoActivity.visibility = System.Windows.Visibility.Visible;
                            break;
                    }
                }
            }
            //检查特定属性
            if (typeIntoActivity.Text == null)
            {
                if (activityDescription.UnrecognizedParameters != null)
                {
                    foreach (var input in activityDescription.UnrecognizedParameters)
                    {
                        typeIntoActivity.Text = input;
                    }
                }
            }

            //检查特定属性
            if (typeIntoActivity.Text == null)
            {
                if (activityVariableContext.Activities.Last != null)
                {
                    var activity = activityVariableContext.Activities.Last.Value;
                    foreach (var item in activity.Variables.Keys)
                    {
                        var value = activity.Variables[item];
                        if (value == ArgumentTypeEnum.Out)
                        {
                            typeIntoActivity.Text = new InArgument<string> { Expression = new VisualBasicValue<string>(item) };
                            break;
                        }
                    }
                }
            }
            activityVariableContext.Activities.AddLast(new Uni.Core.ActivityContext { Activity = typeIntoActivity, Variables = null });
            return typeIntoActivity;
        }
    }
}
