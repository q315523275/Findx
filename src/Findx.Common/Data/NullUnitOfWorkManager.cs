namespace Findx.Data
{
    internal class NullUnitOfWorkManager : IUnitOfWorkManager
    {
        public IUnitOfWork GetConnUnitOfWork(string dbPrimary = default, bool enableTransaction = false)
        {
            throw new NotImplementedException();
        }
    }
}
