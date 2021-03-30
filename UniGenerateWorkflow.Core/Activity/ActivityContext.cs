using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 活动Model
    /// </summary>
    public class ActivityContext
    {
        /// <summary>
        /// 活动
        /// </summary>
        public Activity Activity { get; set; }

        /// <summary>
        /// 变量名/变量类型集合 [变量名/变量类型]
        /// </summary>
        public Dictionary<string, ArgumentTypeEnum> Variables { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ActivityContext()
        {
            Variables = new Dictionary<string, ArgumentTypeEnum>();
        }
    }
}
