using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 写入文本文件活动
    /// </summary>
    public class WriteTextFileActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "写入文本文件";

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
            var writeTextFileActivity = new FileActivity.WriteTextFileActivity();
            ((System.Activities.Activity)writeTextFileActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(writeTextFileActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(writeTextFileActivity.DisplayName):
                            ((System.Activities.Activity)writeTextFileActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(writeTextFileActivity.ContinueOnError):
                            writeTextFileActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(writeTextFileActivity.Text):
                            writeTextFileActivity.Text = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(writeTextFileActivity.Encoding):
                            writeTextFileActivity.Encoding = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) }; ;
                            break;
                        case nameof(writeTextFileActivity.FileName):
                            writeTextFileActivity.FileName = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return writeTextFileActivity;
        }
    }
}
