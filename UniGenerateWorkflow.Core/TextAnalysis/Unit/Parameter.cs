using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 参数单元
    /// </summary>
    public class Parameter : Unit, IUnit
    {
        /// <summary>
        /// 参数标记的起始符号（全角）
        /// </summary>
        public const char StartFlag = '【';

        /// <summary>
        /// 参数终止的结束符号（全角）
        /// </summary>
        public const char EndFlag = '】';

        /// <summary>
        /// 参数单元类型
        /// </summary>
        public override UnitType Type
        {
            get
            {
                return UnitType.Parameter;
            }
        }

        /// <summary>
        /// 活动属性关键字名称集合
        /// </summary>
        public List<string> Keywords { get; set; }
    }
}