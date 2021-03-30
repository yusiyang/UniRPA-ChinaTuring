using SqlSugar;

namespace Uni.Entity
{
    /// <summary>
    /// 对象表:浏览器/百度等
    /// </summary>
    [SugarTable(nameof(ObjectKeyword))]
    public class ObjectKeyword : EntityBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        [SugarColumn(Length = 128)]
        public string Name { get; set; }

    }
}
