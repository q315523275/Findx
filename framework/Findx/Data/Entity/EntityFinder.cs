using Findx.Reflection;

namespace Findx.Data;

/// <summary>
///     实体查找器
/// </summary>
public class EntityFinder : FinderBase<Type>, IEntityFinder
{
    /// <summary>
    ///     重写以实现所有项的查找
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Type> FindAllItems()
    {
        // 排除被继承的Handler实类
        var types = AssemblyManager.FindTypesByBase<IEntity>();
        // ReSharper disable once PossibleMultipleEnumeration
        var baseHandlerTypes = types.Select(m => m.BaseType).Where(m => m is { IsClass: true, IsAbstract: false });
        // ReSharper disable once PossibleMultipleEnumeration
        return types.Except(baseHandlerTypes);
    }
}