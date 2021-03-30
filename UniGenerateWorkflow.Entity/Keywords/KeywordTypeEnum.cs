namespace Uni.Entity
{
    /// <summary>
    /// 关键字类型
    /// </summary>
    public enum KeywordTypeEnum
    {
        /// <summary>
        /// 未指定
        /// </summary>
        UnSpecified = -1,

        /// <summary>
        /// 单词
        /// </summary>
        Word = 0,

        /// <summary>
        /// Foreach
        /// </summary>
        Foreach = 1,

        /// <summary>
        /// While 
        /// </summary>
        While = 2,

        /// <summary>
        /// 分支
        /// </summary>
        IfElse = 3,

        /// <summary>
        /// ForeachRow
        /// </summary>
        ForeachRow = 4
    }
}
