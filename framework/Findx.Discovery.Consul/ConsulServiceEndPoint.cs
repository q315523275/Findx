using System;
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
        //  Consul 约定：Service.Address 为空时回退 Node.Address，否则 Host 为 null 会导致后续 Uri 构造失败
        Host = string.IsNullOrEmpty(serviceEntry.Service.Address) ? serviceEntry.Node.Address : serviceEntry.Service.Address;
        Metadata = ConsulRegistration.GetMetadata(serviceEntry.Service.Tags);
        ServiceName = serviceEntry.Service.Service;
        Port = serviceEntry.Service.Port;
    }

    public string ServiceName { get; }

    public string Host { get; }

    public int Port { get; }

    public IDictionary<string, string> Metadata { get; }

    public override bool Equals(object obj)
    {
        if (obj is ConsulServiceEndPoint other)
            return Host == other.Host && Port == other.Port;

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Host, Port);
    }
}