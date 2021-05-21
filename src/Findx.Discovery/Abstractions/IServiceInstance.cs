using System;
using System.Collections.Generic;
using System.Net;

namespace Findx.Discovery.Abstractions
{
    public interface IServiceInstance
    {
        string ServiceName { get; }
        string Host { get; }
        int Port { get; }
        IDictionary<string, string> Metadata { get; }
    }
    public static class ServiceInstanceExtension
    {
        public static Uri ToUri(this IServiceInstance serviceInstance, string scheme = "http", string path = "/")
        {
            return new UriBuilder(scheme, serviceInstance.Host, serviceInstance.Port, path).Uri;
        }
        public static EndPoint ToIPEndPoint(this IServiceInstance serviceInstance)
        {
            return new IPEndPoint(IPAddress.Parse(serviceInstance.Host), serviceInstance.Port);
        }
    }
}
