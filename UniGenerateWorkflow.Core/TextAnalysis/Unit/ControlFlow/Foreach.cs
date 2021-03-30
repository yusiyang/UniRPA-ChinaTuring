namespace Uni.Core
{
    /// <summary>
    /// 循环体
    /// </summary>
    public class Foreach : ControlFlow
    {
        /// <summary>
        /// 参数标记的起始符号（全角）
        /// </summary>
        public const char StartFlag = '（';

        /// <summary>
        /// 参数终止的结束符合（全角）
        /// </summary>
        public const char EndFlag = '）';

        /// <summary>
        /// 单元类型
        /// </summary>
        public override UnitType Type
        {
            get
            {
                return UnitType.Foreach;
            }
        }

        /// <summary>
        /// 条件分支
        /// </summary>
        public string Values { get; set; }

        /// <summary>
        /// 执行体
        /// </summary>
        public string Body { get; set; }
    }
}
