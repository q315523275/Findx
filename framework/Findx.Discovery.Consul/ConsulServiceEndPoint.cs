using System.Collections.Generic;
using Consul;
using Findx.Discovery.Abstractions;

namespace Findx.Discovery.Consul;

public class ConsulServiceEndPoint : IServiceEndPoint
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceEntry"></param>
    public ConsulServiceEndPoint(ServiceEntry serviceEntry)
    {
        Host = serviceEntry.Service.Address;
        Metadata = ConsulRegistration.GetMetadata(serviceEntry.Service.Tags);
        ServiceName = serviceEntry.Service.Service;
        Port = serviceEntry.Service.Port;
    }

    public string ServiceName { get; }

    public string Host { get; }

    public int Port { get; }

    public IDictionary<string, string> Metadata { get; }
}