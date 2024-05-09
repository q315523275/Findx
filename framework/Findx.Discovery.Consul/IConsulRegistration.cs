using Consul;
using Findx.Discovery.Abstractions;

namespace Findx.Discovery.Consul;

public interface IConsulRegistration : IServiceEndPoint
{
    string InstanceId { get; }
    AgentServiceRegistration Service { get; }
}