using Findx.Reflection;

namespace Findx.RabbitMQ;

/// <summary>
///     MQ消费者查找器
/// </summary>
public class RabbitConsumerFinder : AttributeTypeFinderBase<RabbitListenerAttribute>, IRabbitConsumerFinder
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="appDomainAssemblyFinder"></param>
    public RabbitConsumerFinder(IAppDomainAssemblyFinder appDomainAssemblyFinder) : base(appDomainAssemblyFinder) { }
}