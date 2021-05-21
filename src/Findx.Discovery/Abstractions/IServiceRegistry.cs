using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Discovery.Abstractions
{
    public interface IServiceRegistry<in T> : IDisposable
        where T : IServiceInstance
    {
        Task<bool> Register(T registration, CancellationToken cancellationToken = default);
        Task<bool> Deregister(T registration, CancellationToken cancellationToken = default);
        Task SetStatus(T registration, string status, CancellationToken cancellationToken = default);
        Task<string> GetStatus(T registration, CancellationToken cancellationToken = default);
    }
}
