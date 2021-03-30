using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 一个命令单元
    /// </summary>
    public class Command : Unit, IUnit
    {
        /// <summary>
        /// 命令单元类型
        /// </summary>
        public override UnitType Type
        {
            get
            {
                return UnitType.Command;
            }
        }

        /// <summary>
        /// 当前命令单元子单元集合
        /// </summary>
        public List<IUnit> Units { get; set; }

        /// <summary>
        /// 解析错误信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 当前命令单元包含的控制流组
        /// </summary>
        public ControlFlow ControlFlow { get; set; }
    }
}
