using SqlSugar;

namespace Uni.Entity
{
    /// <summary>
    /// 操作关键字表
    /// </summary>
    [SugarTable(nameof(ActionKeyword))]
    public class ActionKeyword : EntityBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        [SugarColumn(Length = 128)]
        public string Name { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        [SugarColumn(ColumnDataType = "int")]
        public KeywordTypeEnum KeywordType { get; set; }

    }
}
