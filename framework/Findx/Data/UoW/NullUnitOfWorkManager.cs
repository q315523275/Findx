using System.Threading.Tasks;

namespace Findx.Data;

internal class NullUnitOfWorkManager : IUnitOfWorkManager
{
    public Task<IUnitOfWork> GetConnUnitOfWorkAsync(bool enableTransaction = false, bool beginTransaction = false, string dbPrimary = default, CancellationToken cancellationToken = default)
    {
        return default;
    }

    public Task<IUnitOfWork> GetEntityUnitOfWorkAsync<TEntity>(bool enableTransaction = false, bool beginTransaction = false, CancellationToken cancellationToken = default)
    {
        return default;
    }

    public Task<IEnumerable<IUnitOfWork>> GetAllConnUnitOfWorkAsync(CancellationToken cancellationToken = default)
    {
        return default;
    }

    public Task<IEnumerable<IUnitOfWork>> GetAllEntityUnitOfWorkAsync(CancellationToken cancellationToken = default)
    {
        return default;
    }
}