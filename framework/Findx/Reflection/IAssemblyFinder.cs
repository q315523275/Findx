using Findx.DependencyInjection;

namespace Findx.Reflection;

/// <summary>
///     Assembly类型查找器
/// </summary>
[IgnoreDependency]
public interface IAssemblyFinder : IFinder<Assembly>;