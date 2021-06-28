using Findx.Data;
using FreeSql.Internal.ObjectPool;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Data.Common;

namespace Findx.FreeSql
{
    public class FreeSqlUnitOfWork : IUnitOfWork<IFreeSqlClient>
    {
        private readonly IFreeSql _freeSql;
        private readonly IFreeSqlClient _freeSqlClient;
        private readonly IOptionsMonitor<FreeSqlOptions> _options;

        protected Object<DbConnection> _conn;
        protected DbTransaction _tran;

        public FreeSqlUnitOfWork(IFreeSql freeSql, IFreeSqlClient freeSqlClient, IOptionsMonitor<FreeSqlOptions> options, DbTransaction tran)
        {
            _freeSql = freeSql;
            _freeSqlClient = freeSqlClient;
            _options = options;
            _tran = tran;
        }
        private FreeSqlOptions Options
        {
            get
            {
                return _options?.CurrentValue;
            }
        }


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
