using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;

namespace Uni.Core
{
    /// <summary>
    /// 活动工厂
    /// </summary>
    public class ActivityFactory
    {
        /// <summary>
        /// 活动名称和活动对象集合 [活动名称/活动对象集合]
        /// </summary>
        private readonly Dictionary<string, IActivity> _map;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ActivityFactory()
        {
            _map = new Dictionary<string, IActivity>();
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IActivity))));
            foreach (var t in types)
            {
                var obj = (IActivity)Activator.CreateInstance(t);
                _map.Add(t.Name, obj);
            }
        }

        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="activityVariableContext">活动上下文</param>
        /// <param name="activityName">活动名称</param>
        /// <param name="properties">活动属性集合</param>
        /// <returns></returns>
        public Activity GetActivity(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables)
        {
            variables = new List<Variable>();
            if (string.IsNullOrEmpty(activityDescription.ActivityName) || !_map.ContainsKey(activityDescription.ActivityName))
            {
                return null;
            }
            var obj = _map[activityDescription.ActivityName];
            if (obj == null)
            {
                return null;
            }
            return obj.CreateInstance(activityVariableContext, activityDescription, out variables, this);
        }
    }
}
