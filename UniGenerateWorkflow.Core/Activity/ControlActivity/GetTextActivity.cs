using Microsoft.VisualBasic.Activities;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 获取文本活动
    /// </summary>
    public class GetTextActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "获取文本";

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
            var getTextActivity = new ControlActivity.GetText();
            ((System.Activities.Activity)getTextActivity).DisplayName = DisplayName;
            var dic = new Dictionary<string, ArgumentTypeEnum>();
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(getTextActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(getTextActivity.DisplayName):
                            ((System.Activities.Activity)getTextActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(getTextActivity.ContinueOnError):
                            getTextActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(getTextActivity.Element):
                            getTextActivity.Element = new InArgument<UiElement> { Expression = new VisualBasicValue<UiElement>(CreateVariable<UiElement>(variables, key, value)) };
                            break;
                        case nameof(getTextActivity.Value):
                            var outVariable = CreateOurArgument<string>(activityVariableContext, variables, key, value);
                            getTextActivity.Value = new OutArgument<string>() { Expression = new VisualBasicValue<Location<string>>(outVariable) };
                            dic.Add(outVariable, ArgumentTypeEnum.Out);
                            break;
                        case nameof(getTextActivity.Selector):
                            var valiable = CreateVariable<string>(variables, key, value);
                            getTextActivity.Selector = new InArgument<string>() { Expression = new VisualBasicValue<string>(valiable) };
                            break;
                        case nameof(getTextActivity.SourceImgPath):
                            getTextActivity.SourceImgPath = value.ToString();
                            break;
                        case nameof(getTextActivity.Timeout):
                            getTextActivity.Timeout = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(getTextActivity.visibility):
                            getTextActivity.visibility = System.Windows.Visibility.Visible;
                            break;
                    }
                }
            }

            SetValueProperty(activityVariableContext, getTextActivity, variables, dic);

            activityVariableContext.Activities.AddLast(new Uni.Core.ActivityContext { Activity = getTextActivity, Variables = dic });
            return getTextActivity;
        }

        /// <summary>
        /// 设置Value属性
        /// </summary>
        /// <param name="activityVariableContext"></param>
        /// <param name="getTextActivity"></param>
        /// <param name="variables"></param>
        /// <param name="dic"></param>
        private void SetValueProperty(ActivityVariableContext activityVariableContext, ControlActivity.GetText getTextActivity, List<Variable> variables, Dictionary<string, ArgumentTypeEnum> dic)
        {
            if (getTextActivity.Value != null)
            {
                return;
            }
            var variable = CreateOurArgument<string>(activityVariableContext, variables, DataTable, DataTable);
            dic.Add(variable, ArgumentTypeEnum.Out);
            getTextActivity.Value = new OutArgument<string>() { Expression = new VisualBasicValue<Location<string>>(variable) };
        }
    }
}
