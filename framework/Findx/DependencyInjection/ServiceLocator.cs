using System.Security.Claims;
using System.Security.Principal;

namespace Findx.DependencyInjection;

/// <summary>
///     服务提供者定位器
/// </summary>
public static class ServiceLocator
{
    /// <summary>
    ///     服务提供器
    /// </summary>
    public static IServiceProvider Instance { get; set; }

    /// <summary>
    ///     获取单个泛型实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetService<T>()
    {
        Check.NotNull(Instance, nameof(Instance));

        var scopedResolver = Instance.GetService<IScopedServiceResolver>();
        if (scopedResolver is { ResolveEnabled: true }) return scopedResolver.GetService<T>() ?? default;
        return Instance.GetService<T>() ?? default;
    }

    /// <summary>
    ///     获取单个实例对象
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public static object GetService(Type serviceType)
    {
        Check.NotNull(Instance, nameof(Instance));
        Check.NotNull(serviceType, nameof(serviceType));

        var scopedResolver = Instance.GetService<IScopedServiceResolver>();
        if (scopedResolver is { ResolveEnabled: true }) return scopedResolver.GetService(serviceType) ?? default;
        return Instance.GetService(serviceType) ?? default;
    }

    /// <summary>
    ///     获取泛型实例集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetServices<T>()
    {
        Check.NotNull(Instance, nameof(Instance));

        var scopedResolver = Instance.GetService<IScopedServiceResolver>();
        if (scopedResolver is { ResolveEnabled: true }) return scopedResolver.GetServices<T>() ?? default;
        return Instance.GetServices<T>() ?? default;
    }

    /// <summary>
    ///     获取泛型实例对象集合
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public static IEnumerable<object> GetServices(Type serviceType)
    {
        Check.NotNull(Instance, nameof(Instance));
        Check.NotNull(serviceType, nameof(serviceType));

        var scopedResolver = Instance.GetService<IScopedServiceResolver>();
        if (scopedResolver is { ResolveEnabled: true }) return scopedResolver.GetServices(serviceType) ?? default;
        return Instance.GetServices(serviceType) ?? default;
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
}