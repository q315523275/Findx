using System.Threading.Tasks;

namespace Findx.Data;

internal class NullUnitOfWorkManager : IUnitOfWorkManager
{
    public Task<IUnitOfWork> GetUnitOfWorkAsync(string primary, bool enableTransaction, bool beginTransaction, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IUnitOfWork> GetEntityUnitOfWorkAsync(Type entityType, bool enableTransaction, bool beginTransaction, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IUnitOfWork> GetEntityUnitOfWorkAsync<TEntity>(bool enableTransaction, bool beginTransaction, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IUnitOfWork>> GetAllUnitOfWorkAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}