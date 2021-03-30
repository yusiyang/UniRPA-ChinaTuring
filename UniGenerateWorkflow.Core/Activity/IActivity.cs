using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 活动接口
    /// </summary>
    public interface IActivity
    {
        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="activityDescription">活动描述</param>
        /// <param name="activityFactory">活动工厂</param>
        /// <returns></returns>
        Activity CreateInstance(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables, ActivityFactory activityFactory = null);
    }
}
