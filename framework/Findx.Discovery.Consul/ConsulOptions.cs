using Microsoft.Extensions.Options;

namespace Findx.Discovery.Consul;

public class ConsulOptions : IOptions<ConsulOptions>
{
    public string Host { get; set; } = "localhost";

    public string Scheme { get; set; } = "http";

    public int Port { get; set; } = 8500;

    public string Datacenter { get; set; }

    public string Token { get; set; }

    public string WaitTime { get; set; }

    public ConsulOptions Value => this;
}