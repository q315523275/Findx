## 模块化

框架采用模块化的方式进行开发，各个模块独立管理，系统启动时自动扫描所有模块、然后执行各个模块对应初始化方法等。模块需继承StartupModule类，模块包含ConfigureServices、UseModule、OnShutdown等主要方法。

模块加载顺序：ModuleLevel Framework>Application>Business,同级别使用Order排序号排序

> 模块依赖

系统启动加载模块时，会自动识别当前模块所依赖模块，然后自动载入依赖模块。
```js
/// <summary>
///     Findx-Consul服务发现模块
/// </summary>
[Description("Findx-Consul服务发现模块")]
[DependsOnModules([typeof(DiscoveryCoreModule)])]
public class ConsulDiscoveryModule : StartupModule
```

> 模块加载控制

通过IFindxBuilder系统构建器，可以控制模块的加载、排除、使用。

```js
builder.Services.AddFindx().AddModules();
app.UseFindx();
```
如果需要排除某些模块时，可以在AddModules()方法传入需要剔除的模块类型。

> 模块需要使用IApplicationBuilder

可以引用Findx.AspNetCore包，然后继承包里的AspNetCoreModuleBase类

```js
/// <summary>
///     AspNetCore模块基类
/// </summary>
public abstract class AspNetCoreModuleBase : StartupModule
{
    /// <summary>
    ///     应用AspNetCore的服务业务
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    public virtual void UseModule(IApplicationBuilder app)
    {
        base.UseModule(app.ApplicationServices);
    }
}
```


> Consul服务发现模块示例

```js
/// <summary>
///     Findx-Consul服务发现模块
/// </summary>
[Description("Findx-Consul服务发现模块")]
[DependsOnModules([typeof(DiscoveryCoreModule)])]
public class ConsulDiscoveryModule : StartupModule
{
    /// <summary>
    ///     模块等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Framework;

    /// <summary>
    ///     模块排序
    /// </summary>
    public override int Order => 100;

    /// <summary>
    ///     模块配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var configuration = services.GetConfiguration();
        if (!configuration.GetValue<bool>("Findx:Discovery:Enabled")) 
            return services;

        var section = configuration.GetSection("Findx:Consul");
        services.Configure<ConsulOptions>(section);

        services.TryAddSingleton<IConsulServiceRegistry, ConsulServiceRegistry>();
        services.TryAddSingleton<IConsulRegistration, ConsulRegistration>();
        services.TryAddSingleton<IServiceEndPointProvider, ConsulServiceEndPointProvider>();
        services.TryAddSingleton(sp => ConsulClientFactory.CreateClient(sp.GetRequiredService<IOptionsMonitor<ConsulOptions>>().CurrentValue));

        // 自动注册服务发现
        services.AddHostedService<ConsulDiscoveryAutoRegistryWorker>();

        return services;
    }
        
    /// <summary>
    /// 启用模块
    /// </summary>
    /// <param name="app"></param>
    public override void UseModule(IServiceProvider app)
    {
        var optionsMonitor = app.GetRequiredService<IOptionsMonitor<DiscoveryOptions>>();
        if (optionsMonitor.CurrentValue.Enabled) base.UseModule(app);
    }

    /// <summary>
    ///     模块销毁
    /// </summary>
    /// <param name="provider"></param>
    public override void OnShutdown(IServiceProvider provider)
    {
        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<DiscoveryOptions>>();
        if (optionsMonitor.CurrentValue.Enabled && optionsMonitor.CurrentValue.Deregister)
        {
            var consulServiceRegistry = provider.GetRequiredService<IConsulServiceRegistry>();
            var consulRegistration = provider.GetRequiredService<IConsulRegistration>();
            consulServiceRegistry.DeregisterAsync(consulRegistration).ConfigureAwait(false).GetAwaiter();
        }
        base.OnShutdown(provider);
    }
}
```