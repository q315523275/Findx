using Findx.Reflection;

namespace Findx.RabbitMQ;

public class RabbitConsumerFinder : AttributeTypeFinderBase<RabbitListenerAttribute>, IRabbitConsumerFinder
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="appDomainAssemblyFinder"></param>
    public RabbitConsumerFinder(IAppDomainAssemblyFinder appDomainAssemblyFinder) : base(appDomainAssemblyFinder) { }
}