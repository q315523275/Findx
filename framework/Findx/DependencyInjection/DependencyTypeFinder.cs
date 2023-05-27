using Findx.Extensions;
using Findx.Finders;
using Findx.Reflection;

namespace Findx.DependencyInjection;

/// <summary>
///     类型查找器
/// </summary>
public class DependencyTypeFinder : FinderBase<Type>, IDependencyTypeFinder
{
    private readonly IAppDomainAssemblyFinder _appDomainAssemblyFinder;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="appDomainAssemblyFinder">Assembly查找器</param>
    public DependencyTypeFinder(IAppDomainAssemblyFinder appDomainAssemblyFinder)
    {
        _appDomainAssemblyFinder = appDomainAssemblyFinder;
    }

    /// <summary>
    ///     重写以实现所有项的查找
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Type> FindAllItems()
    {
        Type[] baseTypes = { typeof(ISingletonDependency), typeof(IScopeDependency), typeof(ITransientDependency) };
        var assemblies = _appDomainAssemblyFinder.FindAll(true);
        return assemblies.SelectMany(x => x.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract && !type.IsInterface &&
                           !type.HasAttribute<IgnoreDependencyAttribute>()
                           && (baseTypes.Any(b => b.IsAssignableFrom(type)) ||
                               type.HasAttribute<DependencyAttribute>())).Distinct();
    }
}