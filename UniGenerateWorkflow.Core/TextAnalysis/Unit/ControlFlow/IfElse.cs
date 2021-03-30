namespace Uni.Core
{
    /// <summary>
    /// 分支体
    /// </summary>
    public class IfElse : ControlFlow
    {
        /// <summary>
        /// 单元类型
        /// </summary>
        public override UnitType Type
        {
            get
            {
                return UnitType.IfElse;
            }
        }

        /// <summary>
        /// 条件分支
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// 左分支
        /// </summary>
        public string Then { get; set; }

        /// <summary>
        /// 右分支
        /// </summary>
        public string Else { get; set; }
         
        /// <summary>
        /// 左分支的命令单元
        /// </summary>
        public Command ThenCommand { get; set; }

        /// <summary>
        /// 右分支的命令单元
        /// </summary>
        public Command ElseCommand { get; set; }
    }
}
