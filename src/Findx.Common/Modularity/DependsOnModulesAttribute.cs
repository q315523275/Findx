namespace Findx.Modularity;

/// <summary>
///     定义模块依赖
/// </summary>
public class DependsOnModulesAttribute : Attribute
{
    /// <summary>
    ///     初始化一个 OSharp模块依赖<see cref="DependsOnModulesAttribute" />类型的新实例
    /// </summary>
    public DependsOnModulesAttribute(params Type[] dependedModuleTypes)
    {
        DependedModuleTypes = dependedModuleTypes;
    }

    /// <summary>
    ///     获取 当前模块的依赖模块类型集合
    /// </summary>
    public Type[] DependedModuleTypes { get; }
}