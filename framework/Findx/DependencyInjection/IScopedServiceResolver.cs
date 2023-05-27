namespace Findx.DependencyInjection;

/// <summary>
///     范围域服务获取器
/// </summary>
public interface IScopedServiceResolver
{
    /// <summary>
    ///     是否开启获取
    /// </summary>
    bool ResolveEnabled { get; }

    /// <summary>
    ///     获取单个泛型服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T GetService<T>();

    /// <summary>
    ///     获取单个服务对象
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    object GetService(Type serviceType);

    /// <summary>
    ///     获取泛型服务集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IEnumerable<T> GetServices<T>();

    /// <summary>
    ///     获取服务对象集合
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    IEnumerable<object> GetServices(Type serviceType);
}