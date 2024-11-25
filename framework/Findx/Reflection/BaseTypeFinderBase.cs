using Findx.Extensions;
using Findx.Finders;

namespace Findx.Reflection;

/// <summary>
///     指定基类的实现类型查找器基类
/// </summary>
/// <typeparam name="TBaseType"></typeparam>
public abstract class BaseTypeFinderBase<TBaseType> : FinderBase<Type>, ITypeFinder
{
    private readonly IAppDomainAssemblyFinder _appDomainAssemblyFinder;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="appDomainAssemblyFinder"></param>
    public BaseTypeFinderBase(IAppDomainAssemblyFinder appDomainAssemblyFinder)
    {
        _appDomainAssemblyFinder = appDomainAssemblyFinder;
    }

    /// <summary>
    ///     重写以实现所有项的查找
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Type> FindAllItems()
    {
        var assemblies = _appDomainAssemblyFinder.FindAll(true);
        return assemblies.SelectMany(assembly => assembly.GetTypes())
                         .Where(type => type.IsClass && !type.IsAbstract && !type.IsInterface && type.IsDeriveClassFrom<TBaseType>())
                         .Distinct();
    }
}