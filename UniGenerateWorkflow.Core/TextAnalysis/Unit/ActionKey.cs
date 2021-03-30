namespace Uni.Core
{
    /// <summary>
    /// 行为关键字
    /// </summary>
    public class ActionKey : Unit, IUnit
    {
        /// <summary>
        /// 行为单元类型
        /// </summary>
        public override UnitType Type
        {
            get
            {
                return UnitType.ActionKey;
            }
        }
    }
}