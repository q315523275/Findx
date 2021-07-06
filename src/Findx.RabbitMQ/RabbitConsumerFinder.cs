using Findx.Reflection;

namespace Findx.RabbitMQ
{
    public class RabbitConsumerFinder : AttributeTypeFinderBase<RabbitListenerAttribute>, IRabbitConsumerFinder
    {
        public RabbitConsumerFinder(IAppDomainAssemblyFinder appDomainAssemblyFinder) : base(appDomainAssemblyFinder)
        {
        }
    }
}
