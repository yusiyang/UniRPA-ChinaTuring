using SqlSugar;
using System.Collections.Generic;

namespace Uni.Entity
{
    /// <summary>
    /// 活动表
    /// </summary>
    [SugarTable(nameof(Activity))]
    public class Activity : EntityBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        [SugarColumn(Length = 128)]
        public string Name { get; set; }
    }
}
