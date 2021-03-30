using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 移动文件活动
    /// </summary>
    public class MoveFileActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "移动文件";

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
            var moveFileActivity = new FileActivity.MoveFileActivity();
            ((System.Activities.Activity)moveFileActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(moveFileActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(moveFileActivity.DisplayName):
                            ((System.Activities.Activity)moveFileActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(moveFileActivity.ContinueOnError):
                            moveFileActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(moveFileActivity.NewFileName):
                            moveFileActivity.NewFileName = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(moveFileActivity.Overwrite):
                            moveFileActivity.Overwrite = (bool)value;
                            break;
                        case nameof(moveFileActivity.Path):
                            moveFileActivity.Path = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(moveFileActivity.Destination):
                            moveFileActivity.Destination = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return moveFileActivity;
        }
    }
}
