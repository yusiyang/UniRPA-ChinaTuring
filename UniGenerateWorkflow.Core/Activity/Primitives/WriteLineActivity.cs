using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// WriteLine活动
    /// </summary>
    public class WriteLineActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认活动
        /// </summary>
        private const string DisplayName = "输出";

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
            var writeLineActivity = new WriteLine();
            ((System.Activities.Activity)writeLineActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(writeLineActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(writeLineActivity.DisplayName):
                            ((System.Activities.Activity)writeLineActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(writeLineActivity.Text):
                            writeLineActivity.Text = value.ToString();
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return writeLineActivity;
        }
    }
}
