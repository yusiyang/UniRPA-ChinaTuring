namespace Uni.Core
{
    /// <summary>
    /// 单元对象类型
    /// </summary>
    public enum UnitType
    {
        /// <summary>
        /// 未指定
        /// </summary>
        UnSpecified = -1,

        /// <summary>
        /// 命令单元 
        /// </summary>
        Command = 0,

        /// <summary>
        /// 行为关键字单元
        /// </summary>
        ActionKey = 1,

        /// <summary>
        /// 对象关键字单元
        /// </summary>
        ObjectKey = 2,

        /// <summary>
        /// 参数单元
        /// </summary>
        Parameter = 3,

        /// <summary>
        /// Foreach
        /// </summary>
        Foreach = 4,

        /// <summary>
        /// While 
        /// </summary>
        While = 5,

        /// <summary>
        /// 分支
        /// </summary>
        IfElse = 6,

        /// <summary>
        /// ForeachRow
        /// </summary>
        ForeachRow = 7
    }
}
