using SqlSugar;

namespace Uni.Entity
{
    /// <summary>
    /// 参数关键字表
    /// </summary>
    [SugarTable(nameof(ParameterKeyword))]
    public class ParameterKeyword : EntityBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        [SugarColumn(Length = 128)]
        public string Name { get; set; }

    }
}