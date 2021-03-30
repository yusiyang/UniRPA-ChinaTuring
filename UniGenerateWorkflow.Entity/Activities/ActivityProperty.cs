using SqlSugar;
using System.Collections.Generic;

namespace Uni.Entity
{
    /// <summary>
    /// 活动属性表
    /// </summary>
    [SugarTable(nameof(ActivityProperty))]
    public class ActivityProperty : EntityBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        [SugarColumn(Length = 128)]
        public string Name { get; set; }

        /// <summary>
        /// 是否必须
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public bool Required { get; set; }

        /// <summary>
        /// 活动Id
        /// </summary>
        [SugarColumn(Length = 36)]
        public string ActivityId { get; set; }

    }
}
