using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 读取文本文件活动
    /// </summary>
    public class ReadTextFileActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "读取文本文件";

        /// <summary>
        /// 是否含有属性
        /// </summary>
        public override bool IsHasSelector => false;

        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="activityVariableContext"></param>
        /// <param name="activityDescription"></param>
        /// <param name="variables"></param>
        /// <param name="activityFactory"></param>
        /// <returns></returns>
        public Activity CreateInstance(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables, ActivityFactory activityFactory = null)
        {
            variables = new List<Variable>();
            var readTextFileActivity = new FileActivity.ReadTextFileActivity();
            ((System.Activities.Activity)readTextFileActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(readTextFileActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(readTextFileActivity.DisplayName):
                            ((System.Activities.Activity)readTextFileActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(readTextFileActivity.ContinueOnError):
                            readTextFileActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(readTextFileActivity.Content):
                            readTextFileActivity.Content = new OutArgument<string> { Expression = new VisualBasicValue<Location<string>>(CreateOurArgument<string>(activityVariableContext, variables, key, value)) };
                            break;
                        case nameof(readTextFileActivity.Encoding):
                            readTextFileActivity.Encoding = value.ToString();
                            break;
                        case nameof(readTextFileActivity.FileName):
                            readTextFileActivity.FileName = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return readTextFileActivity;
        }
    }
}
