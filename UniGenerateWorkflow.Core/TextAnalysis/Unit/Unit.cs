using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 单元基类
    /// </summary>
    public class Unit : IUnit
    {
        /// <summary>
        /// 单元类型
        /// </summary>
        public virtual UnitType Type
        {
            get
            {
                return UnitType.Command;
            }
        }

        /// <summary>
        /// 位于命令单元中的索引位置
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 前一个单元对象
        /// </summary>
        public IUnit Previous { get; set; }

        /// <summary>
        /// 后一个单元对象
        /// </summary>
        public IUnit Next { get; set; }

        /// <summary>
        /// 父单元
        /// </summary>
        public IUnit Parent { get; set; }
    }
}