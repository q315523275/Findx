using System.Security.Claims;
using System.Security.Principal;
using Findx.Common;
using Findx.Extensions;

namespace Findx.DependencyInjection;

/// <summary>
///     服务提供者定位器
/// </summary>
public static class ServiceLocator
{
    private static IServiceProvider _provider;

    private static IServiceCollection _services;
    
    /// <summary>
    ///     获取 ServiceProvider是否为可用
    /// </summary>
    public static bool IsProviderEnabled => _provider != null;
    
    /// <summary>
    /// 获取 <see cref="ServiceLifetime.Scoped"/>生命周期的服务提供者
    /// </summary>
    public static IServiceProvider ScopedProvider
    {
        get
        {
            var scopedResolver = _provider.GetService<IScopedServiceResolver>();
            return scopedResolver is { ResolveEnabled: true } ? scopedResolver.ScopedProvider : null;
        }
    }
    
    /// <summary>
    /// 获取当前是否处于<see cref="ServiceLifetime.Scoped"/>生命周期中
    /// </summary>
    /// <returns></returns>
    public static bool InScoped()
    {
        var scopedResolver = _provider.GetService<IScopedServiceResolver>();
        return scopedResolver is { ResolveEnabled: true };
    }
    
    /// <summary>
    /// 设置应用程序服务集合
    /// </summary>
    internal static void SetServiceCollection(IServiceCollection services)
    {
        Check.NotNull(services, nameof(services));
        _services = services;
    }

    /// <summary>
    /// 设置应用程序服务提供者
    /// </summary>
    internal static void SetApplicationServiceProvider(IServiceProvider provider)
    {
        Check.NotNull(provider, nameof(provider));
        _provider = provider;
    }
    
    /// <summary>
    ///     获取所有已注册的<see cref="ServiceDescriptor"/>对象
    /// </summary>
    public static IEnumerable<ServiceDescriptor> GetServiceDescriptors()
    {
        Check.NotNull(_services, nameof(_services));
        return _services;
    }

    /// <summary>
    ///     获取单个泛型实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetService<T>()
    {
        _provider.ThrowIfNull();

        var scopedResolver = _provider.GetService<IScopedServiceResolver>();
        if (scopedResolver is { ResolveEnabled: true }) return scopedResolver.GetService<T>() ?? default;
        return _provider.GetService<T>() ?? default;
    }
    
    /// <summary>
    ///     获取单个泛型实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetService<T>(string name)
    {
        _provider.ThrowIfNull();
        name.ThrowIfNull();

        var scopedResolver = _provider.GetService<IScopedServiceResolver>(name);
        if (scopedResolver is { ResolveEnabled: true }) return scopedResolver.GetService<T>(name) ?? default;
        return _provider.GetService<T>(name) ?? default;
    }

    /// <summary>
    ///     获取单个实例对象
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public static object GetService(Type serviceType)
    {
        _provider.ThrowIfNull();

        var scopedResolver = _provider.GetService<IScopedServiceResolver>();
        if (scopedResolver is { ResolveEnabled: true }) return scopedResolver.GetService(serviceType);
        return _provider.GetService(serviceType);
    }

    /// <summary>
    ///     获取单个实例对象
    /// </summary>
    /// <param name="name"></param>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public static object GetService(string name, Type serviceType)
    {
        _provider.ThrowIfNull();
        name.ThrowIfNull();

        var scopedResolver = _provider.GetService<IScopedServiceResolver>();
        if (scopedResolver is { ResolveEnabled: true }) return scopedResolver.GetService(name, serviceType);
        return _provider.GetService(name, serviceType);
    }

    /// <summary>
    ///     获取泛型实例集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetServices<T>()
    {
        Check.NotNull(_provider, nameof(_provider));

        var scopedResolver = _provider.GetService<IScopedServiceResolver>();
        if (scopedResolver is { ResolveEnabled: true }) return scopedResolver.GetServices<T>();
        return _provider.GetServices<T>();
    }

    /// <summary>
    ///     获取泛型实例对象集合
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public static IEnumerable<object> GetServices(Type serviceType)
    {
        Check.NotNull(_provider, nameof(_provider));
        Check.NotNull(serviceType, nameof(serviceType));

        var scopedResolver = _provider.GetService<IScopedServiceResolver>();
        if (scopedResolver is { ResolveEnabled: true }) return scopedResolver.GetServices(serviceType);
        return _provider.GetServices(serviceType);
    }

    /// <summary>
    ///     获取当前用户
    /// </summary>
    public static ClaimsPrincipal GetCurrentUser()
    {
        try
        {
            var user = GetService<IPrincipal>();
            return user as ClaimsPrincipal;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     创建<see cref="ServiceLifetime.Scoped"/>生命周期服务提供器
    /// </summary>
    /// <returns>AsyncServiceScope,需手动释放</returns>
    public static AsyncServiceScope CreateAsyncScope()
    {
        return _provider.CreateAsyncScope();
    }
}