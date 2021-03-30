using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 结构化数据活动
    /// </summary>
    public class ExtractDataActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "结构化数据";

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
            var extractDataActivity = new TextActivity.ExtractDataActivity();
            ((System.Activities.Activity)extractDataActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(extractDataActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(extractDataActivity.DisplayName):
                            ((System.Activities.Activity)extractDataActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(extractDataActivity.ContinueOnError):
                            extractDataActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(extractDataActivity.DataTable):
                            extractDataActivity.DataTable = new OutArgument<System.Data.DataTable> { Expression = new VisualBasicValue<Location<System.Data.DataTable>>(CreateOurArgument<System.Data.DataTable>(activityVariableContext, variables, key, value)) };
                            break;
                        case nameof(extractDataActivity.ExtractMetadata):
                            extractDataActivity.ExtractMetadata = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(extractDataActivity.MaxNumber):
                            extractDataActivity.MaxNumber = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(extractDataActivity.NextSelector):
                            extractDataActivity.NextSelector = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(extractDataActivity.Selector):
                            extractDataActivity.Selector = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(extractDataActivity.SendMessage):
                            extractDataActivity.SendMessage = (bool)value;
                            break;
                        case nameof(extractDataActivity.SimulateClick):
                            extractDataActivity.SimulateClick = (bool)value;
                            break;
                        case nameof(extractDataActivity.SourceImgPath):
                            extractDataActivity.SourceImgPath = value.ToString();
                            break;
                        case nameof(extractDataActivity.visibility):
                            extractDataActivity.visibility = System.Windows.Visibility.Visible;
                            break;
                    }
                }
            }
            return extractDataActivity;
        }
    }
}
