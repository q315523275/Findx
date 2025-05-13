using Findx.Finders;

namespace Findx.Reflection;

/// <summary>
///     指定基类的实现类型查找器基类
/// </summary>
/// <typeparam name="TBaseType"></typeparam>
public abstract class BaseTypeFinderBase<TBaseType> : FinderBase<Type>, ITypeFinder
{
    /// <summary>
    ///     重写以实现所有项的查找
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Type> FindAllItems()
    {
        return AssemblyManager.FindTypesByAttribute<TBaseType>();
    }
}