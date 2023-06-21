namespace Findx.DependencyInjection;

/// <summary>
/// 服务工厂
/// </summary>
/// <typeparam name="TService"></typeparam>
public abstract class ServiceFactoryBase<TService>: IServiceFactory<TService> where TService : class
{
    /// <summary>
    /// 创建服务
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual TService Create(string name)
    {
        name.ThrowIfNull();
        return ServiceLocator.GetService<TService>(name);
    }
}