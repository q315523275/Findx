using Consul;

namespace Findx.Discovery.Consul
{
    public interface IConsulRegistration : IServiceInstance
    {
        string InstanceId { get; }
        AgentServiceRegistration Service { get; }
    }
}