using System;
using System.Activities;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Extensions
{
    /// <summary>
    /// 活动扩展
    /// </summary>
    public static class ActivityExtensions
    {
        private static PropertyInfo parentProperty;

        static ActivityExtensions()
        {
            parentProperty= typeof(Activity).GetProperty("Parent", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// 获取父级活动
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static Activity GetParent(this Activity activity,Predicate<Activity> predicate=null)
        {
            var parentActivity = parentProperty.GetValue(activity) as Activity;
            if(parentActivity==null)
            {
                return null;
            }
            if(predicate!=null&&!predicate(parentActivity))
            {
                return GetParent(parentActivity, predicate);
            }
            return parentActivity;
        }

        /// <summary>
        /// 获取根活动
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static Activity GetRoot(this Activity activity)
        {
            if(activity==null)
            {
                return null;
            }
            var root = activity;
            var parent = activity.GetParent();
            while (parent!=null)
            {
                root = parent;
                parent = parent.GetParent();
            }
            return root;
        }

        public static Activity GetActivity(this object obj)
        {
            var activity = obj as Activity;
            if(activity!=null)
            {
                return activity;
            }
            //处理ForEach和ParallelForEach的情况
            var activityTemplateFactory = obj as IActivityTemplateFactory;
            if (activityTemplateFactory != null)
            {
                activity = activityTemplateFactory.Create(null);
            }
            return activity;
        }
    }
}
