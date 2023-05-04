using System.Collections.Generic;
using Consul;

namespace Findx.Discovery.Consul
{
    public class ConsulServiceInstance : IServiceInstance
    {
        public ConsulServiceInstance(ServiceEntry serviceEntry)
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
}