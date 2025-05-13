using Findx.Extensions;
using Findx.Finders;
using Findx.Reflection;

namespace Findx.DependencyInjection;

/// <summary>
///     类型查找器
/// </summary>
public class DependencyTypeFinder : FinderBase<Type>, IDependencyTypeFinder
{
    /// <summary>
    ///     重写以实现所有项的查找
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Type> FindAllItems()
    {
        Type[] baseTypes = [typeof(ISingletonDependency), typeof(IScopeDependency), typeof(ITransientDependency)];
        return AssemblyManager.FindTypes(type => type.IsClass && !type.IsAbstract && !type.IsInterface
                                          && !type.HasAttribute<IgnoreDependencyAttribute>()
                                          && (baseTypes.Any(b => b.IsAssignableFrom(type)) || type.HasAttribute<DependencyAttribute>())).Distinct();
    }
}