using Findx.Reflection;

namespace Findx.Modularity;

/// <summary>
///     Findx框架模块查找器
/// </summary>
public class StartupModuleTypeFinder : FinderBase<Type>, IStartupModuleTypeFinder
{
    /// <summary>
    ///     重写以实现所有项的查找
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Type> FindAllItems()
    {
        // 排除被继承的Module实类
        var types = AssemblyManager.FindTypesByBase<StartupModule>();
        var baseModuleTypes = types.Select(m => m.BaseType).Where(m => m is { IsClass: true, IsAbstract: false });
        return types.Except(baseModuleTypes);
    }
}