using System.Threading.Tasks;

namespace Findx.Data
{
    internal class NullUnitOfWorkManager : IUnitOfWorkManager
    {
        public IUnitOfWork GetConnUnitOfWork(bool enableTransaction = false, bool beginTransaction = false, string dbPrimary = null)
        {
            throw new NotImplementedException();
        }

        public Task<IUnitOfWork> GetConnUnitOfWorkAsync(bool enableTransaction = false, bool beginTransaction = false, string dbPrimary = default,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IUnitOfWork GetEntityUnitOfWork<TEntity>(bool enableTransaction = false, bool beginTransaction = false)
        {
            throw new NotImplementedException();
        }

        public Task<IUnitOfWork> GetEntityUnitOfWorkAsync<TEntity>(bool enableTransaction = false, bool beginTransaction = false,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
