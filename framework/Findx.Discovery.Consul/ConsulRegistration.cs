using System;
using System.Collections.Generic;
using System.Linq;
using Consul;
using Findx.Common;
using Findx.Utilities;
using Microsoft.Extensions.Options;

namespace Findx.Discovery.Consul;

public class ConsulRegistration : IConsulRegistration
{
    private readonly IApplicationContext _applicationInstanceInfo;


    private readonly IOptionsMonitor<DiscoveryOptions> _options;

    public ConsulRegistration(IOptionsMonitor<DiscoveryOptions> options,
        IApplicationContext applicationInstanceInfo)
    {
        _options = options;
        _applicationInstanceInfo = applicationInstanceInfo;

        Service = CreateRegistration();
        InstanceId = Service.ID;
        Metadata = GetMetadata(Service.Tags);
    }

    private DiscoveryOptions Options
    {
        get
        {
            if (_options != null) return _options.CurrentValue;
            return default;
        }
    }

    public string InstanceId { get; set; }

    public AgentServiceRegistration Service { get; set; }

    public string ServiceName { get; set; }

    public string Host { get; set; }

    public int Port { get; set; }

    public IDictionary<string, string> Metadata { get; set; }

    private string CreateInstanceId()
    {
        Check.NotNull(ServiceName, nameof(ServiceName));
        Check.NotNull(Host, nameof(Host));
        Check.NotNull(Port, nameof(Port));

        return $"{ServiceName}-{Host}:{Port}".Replace(".", "-").Replace(":", "-");
    }

    private IEnumerable<string> CreateTags()
    {
        var tags = new List<string>();
        if (Options.Tags != null) tags.AddRange(Options.Tags);

        if (!string.IsNullOrEmpty(Options.InstanceGroup)) tags.Add("group=" + Options.InstanceGroup);

        tags.Add("secure=" + (Options.Scheme == "https").ToString().ToLower());
        tags.Add("version=" + _applicationInstanceInfo.Version.ToLower());

        return tags;
    }

    private AgentServiceRegistration CreateRegistration()
    {
        ServiceName = Options?.ServiceName ?? _applicationInstanceInfo.ApplicationName;
        Host = Options.HostName;
        Port = Options.Port == 0 ? _applicationInstanceInfo.Port : Options.Port;

        var service = new AgentServiceRegistration
        {
            ID = CreateInstanceId(),
            Name = ServiceName,
            Address = Host,
            Port = Port,
            Tags = CreateTags().ToArray()
        };
        SetCheck(service);

        return service;
    }

    private AgentServiceCheck CreateCheck(int port)
    {
        if (port <= 0) throw new ArgumentException("CreateCheck port must be greater than 0");

        var check = new AgentServiceCheck();

        if (!string.IsNullOrEmpty(Options.HealthCheckUrl))
        {
            check.HTTP = Options.HealthCheckUrl;
        }
        else
        {
            var uri = new Uri($"{Options.Scheme}://{Options.HostName}:{port}{Options.HealthCheckPath}");
            check.HTTP = uri.ToString();
        }

        if (!string.IsNullOrEmpty(Options.HealthCheckInterval))
            check.Interval = TimeSpanUtility.ToTimeSpan(Options.HealthCheckInterval);

        if (!string.IsNullOrEmpty(Options.HealthCheckTimeout))
            check.Timeout = TimeSpanUtility.ToTimeSpan(Options.HealthCheckTimeout);

        if (!string.IsNullOrEmpty(Options.HealthCheckCriticalTimeout))
            check.DeregisterCriticalServiceAfter = TimeSpanUtility.ToTimeSpan(Options.HealthCheckCriticalTimeout);

        check.TLSSkipVerify = Options.HealthCheckTlsSkipVerify;

        return check;
    }

    private void SetCheck(AgentServiceRegistration service)
    {
        if (Options.RegisterHealthCheck && service != null && service.Check == null)
            service.Check = CreateCheck(service.Port);
    }

    internal static IDictionary<string, string> GetMetadata(IList<string> tags)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (tags != null)
            foreach (var tag in tags)
            {
                var index = tag.IndexOf('=');
                string key, value;
                if (index == -1 || Equals(index + 1, tag.Length))
                {
                    key = value = tag;
                }
                else
                {
                    key = tag.Substring(0, index);
                    value = tag.Substring(index + 1);
                }

                metadata[key] = value;
            }

        return metadata;
    }
}