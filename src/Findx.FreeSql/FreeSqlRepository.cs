using Findx.Data;
using Findx.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        private readonly IUnitOfWork<IFreeSqlClient> _unitOfWork;
        public FreeSqlRepository(IFreeSql freeSql, IFreeSqlClient freeSqlClient, IUnitOfWork<IFreeSqlClient> unitOfWork, IOptionsMonitor<FreeSqlOptions> options)
        {
            _freeSql = freeSql;
            _freeSqlClient = freeSqlClient;
            _unitOfWork = unitOfWork;
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


        public IUnitOfWork GetUnitOfWork()
        {
            return _unitOfWork;
        }

        public int Insert(TEntity entity)
        {
            return _freeSql.Insert<TEntity>(entity).ExecuteAffrows();
        }

        public int Insert(IEnumerable<TEntity> entities)
        {
            return _freeSql.Insert<TEntity>(entities).ExecuteAffrows();
        }

        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return _freeSql.Insert<TEntity>(entity).ExecuteAffrowsAsync(cancellationToken);
        }

        public Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return _freeSql.Insert<TEntity>(entities).ExecuteAffrowsAsync(cancellationToken);
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


        public int Update(TEntity entity, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null)
        {
            var update = _freeSql.Update<TEntity>().SetSource(entity);

            if (updateColumns != null)
                update.UpdateColumns(updateColumns);

            if (ignoreColumns != null)
                update.IgnoreColumns(ignoreColumns);

            if (whereExpression != null)
                update.Where(whereExpression);

            return update.ExecuteAffrows();
        }

        public Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, CancellationToken cancellationToken = default)
        {
            var update = _freeSql.Update<TEntity>().SetSource(entity);

            if (updateColumns != null)
                update.UpdateColumns(updateColumns);

            if (ignoreColumns != null)
                update.IgnoreColumns(ignoreColumns);

            if (whereExpression != null)
                update.Where(whereExpression);

            return update.ExecuteAffrowsAsync();
        }


        public TEntity Get(dynamic key)
        {
            return _freeSql.Select<TEntity>(key).First();
        }

        public Task<TEntity> GetAsync(dynamic key, CancellationToken cancellationToken = default)
        {
            return _freeSql.Select<TEntity>(key).FirstAsync();
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
            if (whereExpression == null)
                return _freeSql.Select<TEntity>().FirstAsync(cancellationToken);
            else
                return _freeSql.Select<TEntity>().Where(whereExpression).FirstAsync(cancellationToken);
        }

        public List<TEntity> Top(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null)
        {
            var queryable = _freeSql.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            return queryable.Take(topSize).ToList();
        }

        public Task<List<TEntity>> TopAsync(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _freeSql.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            return queryable.Take(topSize).ToListAsync();
        }

        public List<TObject> Top<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            var queryable = _freeSql.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            queryable.Take(topSize);

            if (selectByExpression == null)
                return queryable.ToList<TObject>();
            else
                return queryable.ToList(selectByExpression);
        }

        public Task<List<TObject>> TopAsync<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _freeSql.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            queryable.Take(topSize);

            if (selectByExpression == null)
                return queryable.ToListAsync<TObject>();
            else
                return queryable.ToListAsync(selectByExpression);
        }

        public List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression != null)
                return _freeSql.Select<TEntity>().Where(whereExpression).ToList();
            return _freeSql.Select<TEntity>().ToList();
        }

        public Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression != null)
                return _freeSql.Select<TEntity>().Where(whereExpression).ToListAsync();
            return _freeSql.Select<TEntity>().ToListAsync();
        }

        public List<TObject> Select<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            var select = _freeSql.Select<TEntity>();

            if (whereExpression != null)
                select.Where(whereExpression);

            if (selectByExpression == null)
                return select.ToList<TObject>();
            else
                return select.ToList(selectByExpression);
        }

        public Task<List<TObject>> SelectAsync<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
        {
            var select = _freeSql.Select<TEntity>();

            if (whereExpression != null)
                select.Where(whereExpression);

            if (selectByExpression == null)
                return select.ToListAsync<TObject>();
            else
                return select.ToListAsync(selectByExpression);
        }

        public PagedResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null)
        {
            var queryable = _freeSql.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            var result = queryable.Count(out var totalRows).Page(pageNumber, pageSize).ToList();

            return new PagedResult<List<TEntity>>(pageNumber, pageSize, (int)totalRows, result);
        }

        public async Task<PagedResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _freeSql.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            var result = await queryable.Count(out var totalRows).Page(pageNumber, pageSize).ToListAsync();

            return new PagedResult<List<TEntity>>(pageNumber, pageSize, (int)totalRows, result);
        }

        public PagedResult<List<TObject>> Paged<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            var queryable = _freeSql.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            queryable.Count(out var totalRows).Page(pageNumber, pageSize);

            List<TObject> result;

            if (selectByExpression == null)
                result = queryable.ToList<TObject>();
            else
                result = queryable.ToList(selectByExpression);

            return new PagedResult<List<TObject>>(pageNumber, pageSize, (int)totalRows, result);
        }

        public async Task<PagedResult<List<TObject>>> PagedAsync<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _freeSql.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            queryable.Count(out var totalRows).Page(pageNumber, pageSize);

            List<TObject> result;

            if (selectByExpression == null)
                result = await queryable.ToListAsync<TObject>();
            else
                result = await queryable.ToListAsync(selectByExpression);

            return new PagedResult<List<TObject>>(pageNumber, pageSize, (int)totalRows, result);
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

    }
}
