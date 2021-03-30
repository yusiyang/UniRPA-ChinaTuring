using SqlSugar;

namespace Uni.Entity
{
    /// <summary>
    /// 关键字活动属性映射表
    /// </summary>
    [SugarTable(nameof(ParameterKeywordActivityPropertyMapping))]
    public class ParameterKeywordActivityPropertyMapping : EntityBase
    {
        /// <summary>
        /// 关键字Id
        /// </summary>
        [SugarColumn(Length = 36)]
        public string ParameterKeywordId { get; set; }

        /// <summary>
        /// 活动属性Id
        /// </summary>
        [SugarColumn(Length = 36)]
        public string ActivityPropertyId { get; set; }

    }
}
