using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Findx.Discovery.LoadBalancer.Selectors
{
    public class LeastConnectionSelector : ILoadBalancer
    {
        private readonly Func<Task<IList<IServiceInstance>>> _services;
        private readonly List<Lease> _leases;
        private readonly string _serviceName;
        private static readonly object _syncLock = new object();

        public LoadBalancerType Name => LoadBalancerType.LeastConnection;

        public LeastConnectionSelector(Func<Task<IList<IServiceInstance>>> services, string serviceName)
        {
            _services = services;
            _serviceName = serviceName;
            _leases = new List<Lease>();
        }

        public async Task<IServiceInstance> ResolveServiceInstanceAsync()
        {
            var services = await _services.Invoke();

            if (services == null)
                throw new ArgumentNullException($"{_serviceName}");

            if (!services.Any())
                throw new ArgumentNullException($"{_serviceName}");

            lock (_syncLock)
            {
                UpdateServices(services);

                var leaseWithLeastConnections = GetLeaseWithLeastConnections();

                _leases.Remove(leaseWithLeastConnections);

                leaseWithLeastConnections = AddConnection(leaseWithLeastConnections);

                _leases.Add(leaseWithLeastConnections);

                return leaseWithLeastConnections.ServiceInstance;
            }
        }

        public Task UpdateStatsAsync(IServiceInstance serviceInstance, TimeSpan responseTime)
        {
            lock (_syncLock)
            {
                var matchingLease = _leases.FirstOrDefault(l => l.ServiceInstance.Host == serviceInstance.Host
                    && l.ServiceInstance.Port == serviceInstance.Port);

                if (matchingLease != null)
                {
                    var replacementLease = new Lease(serviceInstance, matchingLease.Connections - 1);

                    _leases.Remove(matchingLease);

                    _leases.Add(replacementLease);
                }
            }
            return Task.CompletedTask;
        }

        private Lease AddConnection(Lease lease)
        {
            return new Lease(lease.ServiceInstance, lease.Connections + 1);
        }

        private Lease GetLeaseWithLeastConnections()
        {
            //now get the service with the least connections?
            Lease leaseWithLeastConnections = null;

            for (var i = 0; i < _leases.Count; i++)
            {
                if (i == 0)
                {
                    leaseWithLeastConnections = _leases[i];
                }
                else
                {
                    if (_leases[i].Connections < leaseWithLeastConnections.Connections)
                    {
                        leaseWithLeastConnections = _leases[i];
                    }
                }
            }

            return leaseWithLeastConnections;
        }

        private bool UpdateServices(IList<IServiceInstance> services)
        {
            if (_leases.Count > 0)
            {
                var leasesToRemove = new List<Lease>();

                foreach (var lease in _leases)
                {
                    var match = services.FirstOrDefault(s => s.Host == lease.ServiceInstance.Host
                        && s.Port == lease.ServiceInstance.Port);

                    if (match == null)
                    {
                        leasesToRemove.Add(lease);
                    }
                }

                foreach (var lease in leasesToRemove)
                {
                    _leases.Remove(lease);
                }

                foreach (var service in services)
                {
                    var exists = _leases.FirstOrDefault(l => l.ServiceInstance.Host == service.Host && l.ServiceInstance.Port == service.Port);

                    if (exists == null)
                    {
                        _leases.Add(new Lease(service, 0));
                    }
                }
            }
            else
            {
                foreach (var service in services)
                {
                    _leases.Add(new Lease(service, 0));
                }
            }

            return true;
        }
    }
}
