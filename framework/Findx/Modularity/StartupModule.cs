using Findx.Extensions;

namespace Findx.Modularity;

/// <summary>
///     模块组件基类
/// </summary>
public abstract class StartupModule
{
    /// <summary>
    ///     获取或设置 模块等级
    /// </summary>
    public virtual ModuleLevel Level => ModuleLevel.Business;

    /// <summary>
    ///     获取 模块启动顺序，模块启动的顺序先按级别启动，同一级别内部再按此顺序启动，
    ///     级别默认为0，表示无依赖，需要在同级别有依赖顺序的时候，再重写为>0的顺序值
    /// </summary>
    public virtual int Order => 0;

    /// <summary>
    ///     获取 是否已可用
    /// </summary>
    public bool IsEnabled { get; protected set; }

    /// <summary>
    ///     将模块服务添加到依赖注入服务容器中
    /// </summary>
    /// <param name="services">依赖注入服务容器</param>
    /// <returns></returns>
    public virtual IServiceCollection ConfigureServices(IServiceCollection services)
    {
        return services;
    }

    /// <summary>
    ///     应用模块初始化
    /// </summary>
    /// <param name="provider">服务提供者</param>
    public virtual void UseModule(IServiceProvider provider)
    {
        IsEnabled = true;
    }

    /// <summary>
    ///     应用模块注销
    /// </summary>
    /// <param name="provider"></param>
    public virtual void OnShutdown(IServiceProvider provider)
    {
        IsEnabled = false;
    }

    /// <summary>
    ///     模块依赖链解析结果缓存（依赖关系由类型元数据决定，跨实例共享）
    /// </summary>
    private static readonly ConcurrentDictionary<Type, Type[]> DependModuleTypesCache = new();

    /// <summary>
    ///     获取当前模块的依赖模块类型
    /// </summary>
    /// <returns></returns>
    internal IEnumerable<Type> GetDependModuleTypes(Type moduleType = null)
    {
        moduleType ??= GetType();
        return DependModuleTypesCache.GetOrAdd(moduleType, static type => ResolveDependModuleTypes(type));
    }

    /// <summary>
    ///     递归解析模块依赖链，带访问路径环检测
    /// </summary>
    private static Type[] ResolveDependModuleTypes(Type moduleType)
    {
        var dependAttrs = moduleType.GetAttributes<DependsOnModulesAttribute>();
        // ReSharper disable once PossibleMultipleEnumeration
        if (!dependAttrs.Any()) return Type.EmptyTypes;

        var dependTypes = new List<Type>();
        //  当前依赖链路径，用于检测循环依赖（进入时记录、回溯时移除）
        var visited = new HashSet<Type> { moduleType };
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var dependAttr in dependAttrs)
        {
            var packTypes = dependAttr.DependedModuleTypes;
            if (packTypes.Length == 0) continue;
            dependTypes.AddRange(packTypes);
            foreach (var type in packTypes) CollectDependTypes(type, visited, dependTypes);
        }

        return dependTypes.Distinct().ToArray();
    }

    /// <summary>
    ///     收集单个依赖模块的传递依赖；重复出现在依赖路径上即判定为循环依赖
    /// </summary>
    private static void CollectDependTypes(Type moduleType, HashSet<Type> visited, List<Type> result)
    {
        if (!visited.Add(moduleType))
            throw new InvalidOperationException($"检测到模块依赖循环：模块 {moduleType} 重复出现在依赖链中");
        try
        {
            var dependAttrs = moduleType.GetAttributes<DependsOnModulesAttribute>();
            foreach (var dependAttr in dependAttrs)
            {
                var packTypes = dependAttr.DependedModuleTypes;
                if (packTypes.Length == 0) continue;
                result.AddRange(packTypes);
                foreach (var type in packTypes) CollectDependTypes(type, visited, result);
            }
        }
        finally
        {
            visited.Remove(moduleType);
        }
    }
}