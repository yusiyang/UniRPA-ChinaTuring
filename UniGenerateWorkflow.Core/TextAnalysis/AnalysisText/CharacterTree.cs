using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 词库树结构类
    /// </summary>
    public class CharacterTree
    {
        /// <summary>
        /// 单个字符
        /// </summary>
        public char Character { get; set; }

        /// <summary>
        /// 终结符标志
        /// </summary>
        public bool IsEnd { get; set; }

        /// <summary>
        /// 子字符树
        /// </summary>
        public List<CharacterTree> Children { get; set; }
    }
}
