using Findx.Extensions;
using Findx.Modularity;

namespace Findx.Builders;

/// <summary>
///     Findx框架构造者
/// </summary>
public class FindxBuilder : IFindxBuilder
{
    private readonly List<FindxModule> _sourceModules;
    private List<FindxModule> _modules;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="services"></param>
    public FindxBuilder(IServiceCollection services)
    {
        Services = services;
        Configuration = services.GetConfiguration();
        Check.NotNull(Configuration, nameof(Configuration));
        _sourceModules = GetAllModules(services);
        _modules = new List<FindxModule>();
    }

    /// <summary>
    ///     服务
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    ///     配置
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    ///     模块集合
    /// </summary>
    public IEnumerable<FindxModule> Modules => _modules;

    /// <summary>
    ///     添加泛型模块
    /// </summary>
    /// <typeparam name="TModule"></typeparam>
    /// <returns></returns>
    public IFindxBuilder AddModule<TModule>() where TModule : FindxModule
    {
        var type = typeof(TModule);
        return AddModule(type);
    }

    /// <summary>
    ///     添加全部模块
    /// </summary>
    /// <param name="exceptModuleTypes">不加载模块</param>
    /// <returns></returns>
    public IFindxBuilder AddModules(params Type[] exceptModuleTypes)
    {
        var source = _sourceModules;
        var exceptModules = source.Where(m => exceptModuleTypes.Contains(m.GetType()));
        source = source.Except(exceptModules).ToList();
        foreach (var module in source) AddModule(module.GetType());
        return this;
    }

    /// <summary>
    ///     添加单个模块
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private IFindxBuilder AddModule(Type type)
    {
        if (!type.IsBaseOn(typeof(FindxModule))) throw new Exception($"要加载的FindxModule型“{type}”不派生于基类 FindxModule");

        if (_modules.Any(m => m.GetType() == type)) return this;

        var tmpModules = new FindxModule[_modules.Count()];
        _modules.CopyTo(tmpModules);
        var module = _sourceModules.FirstOrDefault(m => m.GetType() == type);
        if (module == null) throw new Exception($"类型为“{type.FullName}”的模块实例无法找到");
        _modules.TryAdd(module);

        // 添加依赖模块
        var dependTypes = module.GetDependModuleTypes();
        foreach (var dependType in dependTypes)
        {
            var dependPack = _sourceModules.FirstOrDefault(m => m.GetType() == dependType);
            if (dependPack == null)
                throw new Exception($"加载模块{module.GetType().FullName}时无法找到依赖模块{dependType.FullName}");
            _modules.TryAdd(dependPack);
        }

        // 按先层级后顺序的规则进行排序
        _modules = _modules.OrderBy(m => m.Level).ThenBy(m => m.Order).ToList();

        var logName = typeof(FindxBuilder).FullName;
        foreach (var tmpModule in _modules.Except(tmpModules))
        {
            var moduleType = tmpModule.GetType();
            var moduleName = moduleType.GetDescription();
            var tmpCount = Services.Count;
            AddModule(Services, tmpModule);
            Services.LogInformation($"模块《{moduleName}》的服务添加完毕,添加了 {Services.Count - tmpCount} 个服务", logName);
        }

        return this;
    }

    /// <summary>
    ///     添加模块配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="module"></param>
    /// <returns></returns>
    private static IServiceCollection AddModule(IServiceCollection services, FindxModule module)
    {
        var type = module.GetType();
        var serviceType = typeof(FindxModule);

        if (type.BaseType?.IsAbstract == false)
        {
            // 移除多重继承的模块
            var descriptors = services.Where(m => m.Lifetime == ServiceLifetime.Singleton &&
                                                  m.ServiceType == serviceType
                                                  && m.ImplementationInstance?.GetType() == type.BaseType);
            foreach (var descriptor in descriptors) services.Remove(descriptor);
        }

        if (services.Any(m =>
                m.Lifetime == ServiceLifetime.Singleton && m.ServiceType == serviceType &&
                m.ImplementationInstance?.GetType() == type)) return services;

        services.AddSingleton(typeof(FindxModule), module);
        module.ConfigureServices(services);

        return services;
    }

    /// <summary>
    ///     获取应用程序所有模块
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static List<FindxModule> GetAllModules(IServiceCollection services)
    {
        var moduleTypeFinder =
            services.GetOrAddTypeFinder<IFindxModuleTypeFinder>(assemblyFinder =>
                new FindxModuleTypeFinder(assemblyFinder));
        var moduleTypes = moduleTypeFinder.FindAll();
        return moduleTypes.Select(m => (FindxModule)Activator.CreateInstance(m))
            .OrderBy(m => m.Level).ThenBy(m => m.Order).ThenBy(m => m.GetType().FullName)
            .ToList();
    }
}