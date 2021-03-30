using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 文件是否存在活动
    /// </summary>
    public class PathExistsActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "文件是否存在";

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
            var pathExistsActivity = new FileActivity.PathExistsActivity();
            ((System.Activities.Activity)pathExistsActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(pathExistsActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(pathExistsActivity.DisplayName):
                            ((System.Activities.Activity)pathExistsActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(pathExistsActivity.ContinueOnError):
                            pathExistsActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(pathExistsActivity.Exists):
                            pathExistsActivity.Exists = new OutArgument<bool> { Expression = new VisualBasicValue<Location<bool>>(CreateOurArgument<bool>(activityVariableContext, variables, key, value)) };
                            break;
                        case nameof(pathExistsActivity.Path):
                            pathExistsActivity.Path = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(pathExistsActivity.PathType):
                            pathExistsActivity.PathType = Convert.ToInt32(value);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return pathExistsActivity;
        }
    }
}
