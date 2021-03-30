using System;
using System.Activities;
using System.Collections.Generic;
using DateTimeActivity;
using Microsoft.VisualBasic.Activities;

namespace Uni.Core
{
    /// <summary>
    /// 获取时间活动
    /// </summary>
    public class GetDateTimeActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "获取时间";

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
            var getDataTimeActivity = new DateTimeActivity.GetDateTimeActivity() { DateType = DateTimeType.日期时间 };
            ((System.Activities.Activity)getDataTimeActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(getDataTimeActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(getDataTimeActivity.DisplayName):
                            ((System.Activities.Activity)getDataTimeActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(getDataTimeActivity.DateType):
                            getDataTimeActivity.DateType = (DateTimeType)value;
                            break;
                        case nameof(getDataTimeActivity.Date):
                            getDataTimeActivity.Date = new OutArgument<string> { Expression = new VisualBasicValue<Location<string>>(CreateOurArgument<string>(activityVariableContext, variables, key, value)) };
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return getDataTimeActivity;
        }
    }
}
