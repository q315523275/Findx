namespace Findx.Data.Compare;

/// <summary>
///     通过对象比较获取变更属性集
/// </summary>
public interface ICompareChange<in T> where T : IEntity
{
    /// <summary>
    ///     获取变更属性
    /// </summary>
    /// <param name="other">其它领域对象</param>
    ChangeValueCollection GetChanges(T other);
}
