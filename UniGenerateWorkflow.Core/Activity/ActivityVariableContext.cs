using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 活动变量上下文
    /// </summary>
    public class ActivityVariableContext
    {
        /// <summary>
        /// 活动识别属性参数名和活动变量名集合 [参数/活动变量名]
        /// </summary>
        private Dictionary<string, string> _variables = new Dictionary<string, string>();

        /// <summary>
        /// 活动集合
        /// </summary>
        public LinkedList<ActivityContext> Activities = new LinkedList<ActivityContext>();

        /// <summary>
        /// 附加资源文件目录
        /// </summary>
        public string AdditionalResourceDir = string.Empty;

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="key">活动识别属性参数名</param>
        /// <returns></returns>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            if (_variables.ContainsKey(key))
            {
                return _variables[key];
            }
            return null;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key">活动识别属性参数名</param>
        /// <param name="value">活动变量名</param>
        public void Add(string key, string value)
        {
            if (_variables.ContainsKey(key))
            {
                return;
            }
            _variables.Add(key, value);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key">活动识别属性参数名</param>
        /// <returns></returns>
        public string Remove(string key)
        {
            var value = Get(key);
            if (value == null)
            {
                return null;
            }
            _variables.Remove(key);
            return value;
        }

        /// <summary>
        /// 是否包含活动
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private bool Contains(Activity activity)
        {
            var current = Activities.First;
            while (current != null)
            {
                if (current.Value.Activity == activity)
                {
                    return true;
                }
                current = current.Next;
            }
            return false;
        }

        /// <summary>
        /// 添加活动
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private ActivityContext Find(Activity activity)
        {
            if (!Contains(activity))
            {
                return null;
            }
            var result = Activities.First;
            while (result != null)
            {
                if (result.Value.Activity == activity)
                {
                    return result.Value;
                }
                result = result.Next;
            }
            return null;
        }

        /// <summary>
        /// 添加活动以及活动变量
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="dic"></param>
        public void AddActivity(Activity activity, Dictionary<string, ArgumentTypeEnum> dic)
        {
            if (Contains(activity))
            {
                var result = Find(activity);
                if (result != null)
                {
                    foreach (var item in dic.Keys)
                    {
                        if (!result.Variables.ContainsKey(item))
                        {
                            result.Variables.Add(item, dic[item]);
                        }
                        else
                        {
                            result.Variables[item] = dic[item];
                        }
                    }
                }
            }
            else
            {
                Activities.AddLast(new ActivityContext { Activity = activity, Variables = dic });
            }

        }
    }
}
