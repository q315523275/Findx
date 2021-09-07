using Findx.Data;
using Findx.Extensions;
using FreeSql.Internal.ObjectPool;
using System;
using System.Data.Common;

namespace Findx.FreeSql
{
    public class FreeSqlUnitOfWork : IUnitOfWork<IFreeSql>
    {
        private readonly IFreeSql _fsql;
        protected Object<DbConnection> _conn;

        public FreeSqlUnitOfWork(IFreeSql freeSql)
        {
            _fsql = freeSql;
        }

        public DbConnection Connection { get { return _conn.Value; } }

        public DbTransaction Transaction { set; get; }

        void ReturnObject()
        {
            _fsql.Ado.MasterPool.Return(_conn);
            _conn = null;
            Transaction = null;
        }

        public void BeginOrUseTransaction()
        {
            if (Transaction != null) return;
            if (_conn != null) _fsql.Ado.MasterPool.Return(_conn);

            _conn = _fsql.Ado.MasterPool.Get();
            try
            {
                Transaction = _conn.Value.BeginTransaction();
            }
            catch
            {
                ReturnObject();
                throw;
            }
        }

        public void Commit()
        {
            try
            {
                if (Transaction != null)
                {
                    if (Transaction.Connection != null)
                        Transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                ex.ReThrow();
            }
            finally
            {
                ReturnObject();
            }
        }

        public void Rollback()
        {
            try
            {
                if (Transaction != null)
                {
                    if (Transaction.Connection != null)
                        Transaction.Rollback();
                }
            }
            catch (Exception ex)
            {
                ex.ReThrow();
            }
            finally
            {
                ReturnObject();
            }
        }

        public IFreeSql GetInstance()
        {
            return _fsql;
        }
    }
}
