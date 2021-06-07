using Findx.Data;
using Findx.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.FreeSql
{
    public class FreeSqlRepository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        private readonly static IDictionary<Type, string> DataSourceMap = new Dictionary<Type, string>();
        private readonly IFreeSql _freeSql;
        private readonly IFreeSqlClient _freeSqlClient;
        private readonly IOptionsMonitor<FreeSqlOptions> _options;

        public FreeSqlRepository(IFreeSql freeSql, IFreeSqlClient freeSqlClient, IOptionsMonitor<FreeSqlOptions> options)
        {
            _freeSql = freeSql;
            _freeSqlClient = freeSqlClient;
            _options = options;

            if (Options?.DataSource.Keys.Count <= 1) return;

            var primary = DataSourceMap.GetOrAdd(_entityType, () =>
            {
                var dataSourceAttribute = _entityType.GetAttribute<DataSourceAttribute>();
                return dataSourceAttribute?.Primary ?? Options?.Primary;
            });
            _freeSql = _freeSqlClient.Get(primary);
        }

        protected Type _entityType = typeof(TEntity);

        private FreeSqlOptions Options
        {
            get
            {
                return _options?.CurrentValue;
            }
        }
        public int Count(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _freeSql.Select<TEntity>().Count().To<int>();
            else
                return _freeSql.Select<TEntity>().Where(whereExpression).Count().To<int>();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression == null)
                return (await _freeSql.Select<TEntity>().CountAsync(cancellationToken)).To<int>();
            else
                return (await _freeSql.Select<TEntity>().Where(whereExpression).CountAsync(cancellationToken)).To<int>();
        }

        public int Delete(dynamic key)
        {
            return _freeSql.Delete<TEntity>(key).ExecuteAffrows();
        }

        public int Delete(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _freeSql.Delete<TEntity>().ExecuteAffrows();
            else
                return _freeSql.Delete<TEntity>().Where(whereExpression).ExecuteAffrows();
        }

        public Task<int> DeleteAsync(dynamic key, CancellationToken cancellationToken = default)
        {
            return _freeSql.Delete<TEntity>(key).ExecuteAffrowsAsync(cancellationToken);
        }

        public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression == null)
                return _freeSql.Delete<TEntity>().ExecuteAffrowsAsync(cancellationToken);
            else
                return _freeSql.Delete<TEntity>().Where(whereExpression).ExecuteAffrowsAsync(cancellationToken);
        }

        public bool Exist(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _freeSql.Select<TEntity>().Any();
            else
                return _freeSql.Select<TEntity>().Where(whereExpression).Any();
        }

        public Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression == null)
                return _freeSql.Select<TEntity>().AnyAsync(cancellationToken);
            else
                return _freeSql.Select<TEntity>().Where(whereExpression).AnyAsync(cancellationToken);
        }

        public TEntity First(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _freeSql.Select<TEntity>().First();
            else
                return _freeSql.Select<TEntity>().Where(whereExpression).First();
        }

        public Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public TEntity Get(dynamic key)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> GetAsync(dynamic key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IUnitOfWork GetUnitOfWork()
        {
            throw new NotImplementedException();
        }

        public int Insert(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public int Insert(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public PagedResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false)
        {
            throw new NotImplementedException();
        }

        public PagedResult<List<TObject>> Paged<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<List<TObject>>> PagedAsync<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            throw new NotImplementedException();
        }

        public List<TObject> Select<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<TObject>> SelectAsync<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public List<TEntity> Top(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false)
        {
            throw new NotImplementedException();
        }

        public List<TObject> Top<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<TEntity>> TopAsync(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<TObject>> TopAsync<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public int Update(TEntity entity, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
