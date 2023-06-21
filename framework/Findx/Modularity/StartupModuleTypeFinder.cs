using Findx.Reflection;

namespace Findx.Modularity;

/// <summary>
///     Findx框架模块查找器
/// </summary>
public class StartupModuleTypeFinder : BaseTypeFinderBase<StartupModule>, IStartupModuleTypeFinder
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="appDomainAssemblyFinder"></param>
    public StartupModuleTypeFinder(IAppDomainAssemblyFinder appDomainAssemblyFinder) : base(appDomainAssemblyFinder)
    {
    }

    /// <summary>
    ///     重写以实现所有项的查找
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Type> FindAllItems()
    {
        // 排除被继承的Module实类
        var types = base.FindAllItems();
        var baseModuleTypes = types.Select(m => m.BaseType).Where(m => m is { IsClass: true, IsAbstract: false });
        return types.Except(baseModuleTypes);
    }
}