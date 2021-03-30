namespace Uni.Core
{
    /// <summary>
    /// 循环体
    /// </summary>
    public class While : ControlFlow
    {
        /// <summary>
        /// 单元类型
        /// </summary>
        public override UnitType Type
        {
            get
            {
                return UnitType.While;
            }
        }

        /// <summary>
        /// 条件分支
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// 执行体
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 执行体的命令单元
        /// </summary>
        public Command BodyCommand { get; set; }
    }
}
