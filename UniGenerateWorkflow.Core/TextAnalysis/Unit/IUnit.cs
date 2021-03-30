namespace Uni.Core
{
    /// <summary>
    /// 单元对象接口，文本中所有可执行对象就继承自该接口
    /// </summary>
    public interface IUnit
    {
        /// <summary>
        /// 单元类型
        /// </summary>
        UnitType Type { get; }

        /// <summary>
        /// 位于单元中的索引位置
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// 前一个单元
        /// </summary>
        IUnit Previous { get; set; }

        /// <summary>
        /// 后一个单元
        /// </summary>
        IUnit Next { get; set; }

        /// <summary>
        /// 父单元
        /// </summary>
        IUnit Parent { get; set; } 
    }
}
