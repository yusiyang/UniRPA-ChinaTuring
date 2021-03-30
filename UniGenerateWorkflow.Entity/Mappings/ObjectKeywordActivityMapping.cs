using SqlSugar;

namespace Uni.Entity
{
    /// <summary>
    /// 关键字活动映射表
    /// </summary>
    [SugarTable(nameof(ObjectKeywordActivityMapping))]
    public class ObjectKeywordActivityMapping : EntityBase
    {
        /// <summary>
        /// 关键字Id
        /// </summary>
        [SugarColumn(Length = 36)]
        public string ObjectKeywordId { get; set; }

        /// <summary>
        /// 活动Id
        /// </summary>
        [SugarColumn(Length = 36)]
        public string ActivityId { get; set; }

    }
}
