using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 循环体
    /// </summary>
    public class ForeachRow : ControlFlow
    {
        /// <summary>
        /// 参数标记的起始符号
        /// </summary>
        public const string SubCommandFlag = "*";

        /// <summary>
        /// 单元类型
        /// </summary>
        public override UnitType Type
        {
            get
            {
                return UnitType.ForeachRow;
            }
        }

        /// <summary>
        /// 条件分支
        /// </summary>
        public string DataTable { get; set; }

        /// <summary>
        /// 执行体
        /// </summary>
        public List<Command> Body { get; set; }
    }
}
