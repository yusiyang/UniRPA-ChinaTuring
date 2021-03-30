using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Uni.Core
{
    /// <summary>
    /// 打开浏览器活动
    /// </summary>
    public class OpenBrowserActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "打开浏览器";

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
            var openBrowserActivity = new BrowserActivity.OpenBrowser()
            {
                //BrowserType = BrowserActivity.BrowserTypes.Chrome
                BrowserType = Plugins.Shared.Library.UiAutomation.Browser.BrowserType.Chrome
            };
            ((System.Activities.Activity)openBrowserActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                //循环属性集合给属性赋值
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    switch (key)
                    {
                        case nameof(openBrowserActivity.DisplayName):
                            ((System.Activities.Activity)openBrowserActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(openBrowserActivity.BrowserType):
                            openBrowserActivity.BrowserType = (Plugins.Shared.Library.UiAutomation.Browser.BrowserType)value;
                            break;
                        case nameof(openBrowserActivity.Url):
                            openBrowserActivity.Url = value.ToString();
                            break;
                        case nameof(openBrowserActivity.Body):
                            openBrowserActivity.Body = GetActivityAction<object>(activityVariableContext, activityFactory, variables, key, value);
                            break;
                        case nameof(openBrowserActivity.Browser):
                            openBrowserActivity.Browser = new OutArgument<Plugins.Shared.Library.UiAutomation.Browser.IBrowser>() { Expression = new VisualBasicValue<Location<Plugins.Shared.Library.UiAutomation.Browser.IBrowser>>(CreateOurArgument<Plugins.Shared.Library.UiAutomation.Browser.IBrowser>(activityVariableContext, variables, key, value)) };
                            break;
                        case nameof(openBrowserActivity.ContinueOnError):
                            openBrowserActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(openBrowserActivity.OverTime):
                            openBrowserActivity.OverTime = new InArgument<int>() { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(openBrowserActivity.Private):
                            openBrowserActivity.Private = (bool)value;
                            break;
                    }
                }
            }

            //设置未赋值但需要赋值的属性
            var unrecognizedParameters = activityDescription.UnrecognizedParameters;
            SetUrlProperty(openBrowserActivity, unrecognizedParameters);

            return openBrowserActivity;
        }

        /// <summary>
        /// 设置Url属性
        /// </summary>
        /// <param name="openBrowserActivity"></param>
        /// <param name="unrecognizedParameters"></param>
        private void SetUrlProperty(BrowserActivity.OpenBrowser openBrowserActivity, List<string> unrecognizedParameters)
        {
            if (openBrowserActivity.Url != null || unrecognizedParameters == null)
            {
                return;
            }
            for (var i = unrecognizedParameters.Count - 1; i >= 0; i--)
            {
                var match = Regex.Match(unrecognizedParameters[i], UrlPattern);
                if (!match.Success)
                {
                    continue;
                }

                openBrowserActivity.Url = match.Value ?? "";
                unrecognizedParameters.RemoveAt(i);
                break;
            }
        }

    }
}
