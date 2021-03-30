using System;
using System.Activities;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.Activities;
using Plugins.Shared.Library.UiAutomation;

namespace Uni.Core
{
    /// <summary>
    /// 热键活动
    /// </summary>
    public class HotKeyActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "热键";

        public override bool IsHasSelector => true;

        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public Activity CreateInstance(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables, ActivityFactory activityFactory = null)
        {
            variables = new List<Variable>();
            var hotKeyActivity = new KeyboardActivity.HotKeyActivity();
            ((System.Activities.Activity)hotKeyActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(hotKeyActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(hotKeyActivity.DisplayName):
                            ((System.Activities.Activity)hotKeyActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(hotKeyActivity.ContinueOnError):
                            hotKeyActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(hotKeyActivity.ClickType):
                            hotKeyActivity.ClickType = Convert.ToInt32(value);
                            break;
                        case nameof(hotKeyActivity.Alt):
                            hotKeyActivity.Alt = (bool)value;
                            break;
                        case nameof(hotKeyActivity.Ctrl):
                            hotKeyActivity.Ctrl = (bool)value;
                            break;
                        case nameof(hotKeyActivity.Shift):
                            hotKeyActivity.Shift = (bool)value;
                            break;
                        case nameof(hotKeyActivity.Win):
                            hotKeyActivity.Win = (bool)value;
                            break;
                        case nameof(hotKeyActivity.Element):
                            hotKeyActivity.Element = new InArgument<UiElement> { Expression = new VisualBasicValue<UiElement>(CreateVariable<UiElement>(variables, key, value)) };
                            break;
                        case nameof(hotKeyActivity.SelectedKey):
                            var selectKey = value.ToString();
                            foreach (var virtualKey in Enum.GetValues(typeof(VirtualKey)))
                            {
                                var name = Enum.GetName(typeof(VirtualKey), virtualKey);
                                if (name.ToLower() == selectKey.ToLower() || name.ToLower() == "key_" + selectKey.ToLower())
                                {
                                    selectKey = name;
                                    break;
                                }
                            }
                            hotKeyActivity.SelectedKey = selectKey.ToString();
                            break;
                        case nameof(hotKeyActivity.offsetX):
                            hotKeyActivity.offsetX = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(hotKeyActivity.offsetY):
                            hotKeyActivity.offsetY = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(hotKeyActivity.MouseButton):
                            hotKeyActivity.MouseButton = Convert.ToInt32(value);
                            break;
                        case nameof(hotKeyActivity.IsRunClick):
                            hotKeyActivity.IsRunClick = (bool)value;
                            break;
                        case nameof(hotKeyActivity.KeyTypes):
                            hotKeyActivity.KeyTypes = (List<string>)value;
                            break;
                        case nameof(hotKeyActivity.Selector):
                            hotKeyActivity.Selector = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(hotKeyActivity.SourceImgPath):
                            hotKeyActivity.SourceImgPath = value.ToString();
                            break;
                        case nameof(hotKeyActivity.Timeout):
                            hotKeyActivity.Timeout = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(hotKeyActivity.visibility):
                            hotKeyActivity.visibility = System.Windows.Visibility.Visible;
                            break;
                    }
                }
            }
            //检查特定属性
            if (hotKeyActivity.SelectedKey == null)
            {
                if (activityDescription.UnrecognizedParameters != null)
                {
                    foreach (var input in activityDescription.UnrecognizedParameters)
                    {
                        var selectKey = input.ToLower();
                        foreach (var virtualKey in Enum.GetValues(typeof(VirtualKey)))
                        {
                            var name = Enum.GetName(typeof(VirtualKey), virtualKey);
                            if (name.ToLower() == selectKey || name.ToLower() == "key_" + selectKey)
                            {
                                selectKey = name;
                                break;
                            }
                        }
                        hotKeyActivity.SelectedKey = selectKey;
                    }
                }
            }
            return hotKeyActivity;
        }
    }
}
