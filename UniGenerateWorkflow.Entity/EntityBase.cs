using SqlSugar;
using System;

namespace Uni.Entity
{
    /// <summary>
    /// 数据实体基础类
    /// </summary>
    public abstract class EntityBase
    {
        /// <summary>
        /// Id
        /// </summary>
        [SugarColumn(IsNullable = false, IsPrimaryKey = true, Length = 36)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

    }
}
