using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Findx.Discovery.Abstractions;

namespace Findx.Discovery.LoadBalancer.Selectors;

/// <summary>
///     最小连接选择器
/// </summary>
public class LeastConnectionSelector : ILoadBalancer
{
    private static readonly object SyncLock = new();
    private readonly List<Lease> _leases;
    private readonly string _serviceName;
    private readonly Func<Task<IReadOnlyList<IServiceEndPoint>>> _services;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceName"></param>
    public LeastConnectionSelector(Func<Task<IReadOnlyList<IServiceEndPoint>>> services, string serviceName)
    {
        _services = services;
        _serviceName = serviceName;
        _leases = [];
    }

    /// <summary>
    ///     选择器名称
    /// </summary>
    public LoadBalancerType Name => LoadBalancerType.LeastConnection;

    /// <summary>
    ///     获取服务
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<IServiceEndPoint> ResolveServiceEndPointAsync(CancellationToken cancellationToken = default)
    {
        var services = await _services.Invoke();

        if (services == null)
            throw new ArgumentNullException($"{_serviceName}");

        if (!services.Any())
            throw new ArgumentNullException($"{_serviceName}");

        lock (SyncLock)
        {
            UpdateServices(services);

            var leaseWithLeastConnections = GetLeaseWithLeastConnections();

            _leases.Remove(leaseWithLeastConnections);

            leaseWithLeastConnections = AddConnection(leaseWithLeastConnections);

            _leases.Add(leaseWithLeastConnections);

            return leaseWithLeastConnections.ServiceEndPoint;
        }
    }

    /// <summary>
    ///     更新统计
    /// </summary>
    /// <param name="serviceEndPoint"></param>
    /// <param name="responseTime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task UpdateStatsAsync(IServiceEndPoint serviceEndPoint, TimeSpan responseTime, CancellationToken cancellationToken = default)
    {
        lock (SyncLock)
        {
            var matchingLease = _leases.FirstOrDefault(l => l.ServiceEndPoint.Host == serviceEndPoint.Host
                                                            && l.ServiceEndPoint.Port == serviceEndPoint.Port);

            if (matchingLease != null)
            {
                var replacementLease = new Lease(serviceEndPoint, matchingLease.Connections - 1);

                _leases.Remove(matchingLease);

                _leases.Add(replacementLease);
            }
        }

        return Task.CompletedTask;
    }

    private Lease AddConnection(Lease lease)
    {
        return new Lease(lease.ServiceEndPoint, lease.Connections + 1);
    }

    private Lease GetLeaseWithLeastConnections()
    {
        // now get the service with the least connections?
        Lease leaseWithLeastConnections = null;

        for (var i = 0; i < _leases.Count; i++)
            if (i == 0)
            {
                leaseWithLeastConnections = _leases[i];
            }
            else
            {
                // ReSharper disable once PossibleNullReferenceException
                if (_leases[i].Connections < leaseWithLeastConnections.Connections)
                    leaseWithLeastConnections = _leases[i];
            }

        return leaseWithLeastConnections;
    }

    private bool UpdateServices(IReadOnlyList<IServiceEndPoint> services)
    {
        if (_leases.Count > 0)
        {
            var leasesToRemove = new List<Lease>();

            foreach (var lease in _leases)
            {
                var match = services.FirstOrDefault(s => s.Host == lease.ServiceEndPoint.Host && s.Port == lease.ServiceEndPoint.Port);

                if (match == null) leasesToRemove.Add(lease);
            }

            foreach (var lease in leasesToRemove) _leases.Remove(lease);

            foreach (var service in services)
            {
                var exists = _leases.FirstOrDefault(l => l.ServiceEndPoint.Host == service.Host && l.ServiceEndPoint.Port == service.Port);

                if (exists == null) _leases.Add(new Lease(service, 0));
            }
        }
        else
        {
            foreach (var service in services) _leases.Add(new Lease(service, 0));
        }

        return true;
    }
}