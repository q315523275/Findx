using System;

namespace Findx.Data
{
    internal class NullUnitOfWorkManager : IUnitOfWorkManager
    {
        public IUnitOfWork GetConnUnitOfWork(string dbPrimary)
        {
            throw new NotImplementedException();
        }
    }
}
