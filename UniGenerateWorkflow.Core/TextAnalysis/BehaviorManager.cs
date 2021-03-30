using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uni.Entity;

namespace Uni.Core
{
    /// <summary>
    /// 知识库管理类
    /// </summary>
    public class BehaviorManager
    {
        /// <summary>
        /// 行为映射数据集合
        /// </summary>
        private readonly Dictionary<string, string> _behaviors = new Dictionary<string, string>();

        /// <summary>
        /// 行为映射数据集合
        /// </summary>
        private readonly Dictionary<string, string> _selectorBehaviors = new Dictionary<string, string>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public BehaviorManager()
        {
            InitBehaviorData(out Dictionary<string, string> behaviors, out Dictionary<string, string> selectorBehaviors);
            _behaviors = behaviors;
            _selectorBehaviors = selectorBehaviors;
        }

        private void InitBehaviorData(out Dictionary<string, string> behaviors, out Dictionary<string, string> selectorBehaviors)
        {
            behaviors = new Dictionary<string, string>();
            selectorBehaviors = new Dictionary<string, string>();
            using (DbContext dbContext = new DbContext())
            {
                List<Behavior> bList = dbContext.Behavior.GetList();
                foreach (var b in bList)
                {
                    if (b.BehaviorType == BehaviorTypeEnum.Selector)
                    {
                        selectorBehaviors.Add(b.Key, b.Value);
                    }
                    else
                    {
                        behaviors.Add(b.Key, b.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 获取关键词在知识库里对应的值
        /// </summary>
        /// <param name="key">关键词</param>
        /// <returns>对应的值</returns>
        public string GetValue(string key)
        {
            if (_behaviors.ContainsKey(key))
            {
                return _behaviors[key];
            }
            return null;
        }

        /// <summary>
        /// 获取Selector选择器在知识库里对应的值
        /// </summary>
        /// <param name="selectorKey">关键词</param>
        /// <returns>对应的值</returns>
        public string GetSelectorValue(string selectorKey)
        {
            if (_selectorBehaviors.ContainsKey(selectorKey))
            {
                return _selectorBehaviors[selectorKey];
            }
            return null;
        }
    }
}
