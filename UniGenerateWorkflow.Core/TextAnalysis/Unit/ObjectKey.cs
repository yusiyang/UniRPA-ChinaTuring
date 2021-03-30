namespace Uni.Core
{
    /// <summary>
    /// 对象关键字
    /// </summary>
    public class ObjectKey : Unit, IUnit
    {
        /// <summary>
        /// 对象单元类型
        /// </summary>
        public override UnitType Type
        {
            get
            {
                return UnitType.ObjectKey;
            }
        }
    }
}