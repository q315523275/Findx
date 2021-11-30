using System;
using System.Collections.Generic;
using System.Text;

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
