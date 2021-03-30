using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 控制流
    /// </summary>
    public abstract class ControlFlow : Unit, IUnit
    {
        /// <summary>
        /// 模板关键字中的占位符
        /// </summary>
        public const string PlaceholderFlag = "...";

        /// <summary>
        /// 正则匹配两个标记为中间的值模板
        /// </summary>
        public const string AmongRegexModal = @"(?<={0})[\s\S]+?(?={1})";

        /// <summary>
        /// 正则匹配一个标记为之后的值模板
        /// </summary>
        public const string AfterRegexModal = @"(?<={0})[\s\S]*"; 

        /// <summary>
        /// 对应的正则表达式值
        /// </summary>
        public string Regex { get; set; }

        /// <summary>
        /// 模板里的actionkey集合
        /// </summary>
        public List<ActionKey> ActionKeys { get; set; }
    }
}