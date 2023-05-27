namespace Findx.DependencyInjection;

/// <summary>
///     默认作用域工厂
/// </summary>
public class DefaultServiceScopeFactory : IHybridServiceScopeFactory
{
    /// <summary>
    /// </summary>
    /// <param name="serviceScopeFactory"></param>
    public DefaultServiceScopeFactory(IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    ///     作用域工厂
    /// </summary>
    private IServiceScopeFactory ServiceScopeFactory { get; }

    /// <summary>
    ///     创建新的容器作用域
    /// </summary>
    /// <returns></returns>
    public IServiceScope CreateScope()
    {
        return ServiceScopeFactory.CreateScope();
    }
}