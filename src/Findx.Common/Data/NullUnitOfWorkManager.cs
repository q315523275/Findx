namespace Findx.Data
{
    internal class NullUnitOfWorkManager : IUnitOfWorkManager
    {
        public IUnitOfWork GetConnUnitOfWork(bool enableTransaction = false, string dbPrimary = null)
        {
            throw new NotImplementedException();
        }

        public IUnitOfWork GetEntityUnitOfWork<TEntity>(bool enableTransaction = false)
        {
            throw new NotImplementedException();
        }
    }
}
