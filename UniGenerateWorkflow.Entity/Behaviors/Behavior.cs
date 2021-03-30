using SqlSugar;

namespace Uni.Entity
{
    /// <summary>
    /// 行为映射表:知识库
    /// </summary>
    [SugarTable(nameof(Behavior))]
    public class Behavior : EntityBase
    {
        /// <summary>
        /// Key
        /// </summary>
        [SugarColumn(Length = 128)]
        public string Key { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        [SugarColumn(Length = 2000)]
        public string Value { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        [SugarColumn(ColumnDataType = "int")]
        public BehaviorTypeEnum BehaviorType { get; set; }
    }
}
