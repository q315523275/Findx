using Findx.Data;
using System;
using System.Data;
using System.Data.Common;

namespace Findx.FreeSql
{
    public class FreeSqlUnitOfWork : IUnitOfWork<IFreeSqlClient>
    {

        protected DbTransaction _tran;
        public void BeginTran()
        {
            throw new NotImplementedException();
        }

        public void BeginTran(IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        public void CommitTran()
        {
            throw new NotImplementedException();
        }

        public IFreeSqlClient GetInstance()
        {
            throw new NotImplementedException();
        }

        public void RollbackTran()
        {
            throw new NotImplementedException();
        }
    }
}
