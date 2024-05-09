namespace Findx.DependencyInjection;

/// <summary>
///     泛型服务工厂
/// </summary>
public interface IServiceFactory<out TService> where TService : class
{
    /// <summary>
    ///     创建实例
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    TService Create(string name);
}