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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptionsMonitor<FreeSqlOptions> _options;

        public FreeSqlRepository(FreeSqlClient clients, IUnitOfWorkManager uowManager, IOptionsMonitor<FreeSqlOptions> options)
        {
            _options = options;

            var primary = Options?.Primary;
            if (Options?.DataSource.Keys.Count > 1)
            {
                DataSourceMap.GetOrAdd(_entityType, () =>
                {
                    var dataSourceAttribute = _entityType.GetAttribute<DataSourceAttribute>();
                    return dataSourceAttribute?.Primary ?? Options?.Primary;
                });
            };
            clients.TryGetValue(primary, out _freeSql);
            _unitOfWork = uowManager.GetConnUnitOfWork(primary);
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
            return _freeSql.Insert(entity).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public int Insert(IEnumerable<TEntity> entities)
        {
            return _freeSql.Insert(entities).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return _freeSql.Insert(entity).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }

        public Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return _freeSql.Insert(entities).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }


        public int Delete(object key)
        {
            return _freeSql.Delete<TEntity>(key).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public int Delete(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _freeSql.Delete<TEntity>().Where(it => true).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
            else
                return _freeSql.Delete<TEntity>().Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> DeleteAsync(object key, CancellationToken cancellationToken = default)
        {
            return _freeSql.Delete<TEntity>(key).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync();
        }

        public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression == null)
                return _freeSql.Delete<TEntity>().Where(it => true).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            else
                return _freeSql.Delete<TEntity>().Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
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

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
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

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync();
        }


        public int Update(TEntity entity, bool ignoreNullColumns = false)
        {
            var update = _freeSql.Update<TEntity>();

            if (ignoreNullColumns)
                update.SetSourceIgnore(entity, col => col == null);
            else
                update.SetSource(entity);

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> UpdateAsync(TEntity entity, bool ignoreNullColumns = false, CancellationToken cancellationToken = default)
        {
            var update = _freeSql.Update<TEntity>();

            if (ignoreNullColumns)
                update.SetSourceIgnore(entity, col => col == null);
            else
                update.SetSource(entity);

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync();
        }

        public int Update(List<TEntity> entitys, bool ignoreNullColumns = false)
        {
            var update = _freeSql.Update<TEntity>();

            if (ignoreNullColumns)
                update.SetSource(entitys);
            else
                update.SetSource(entitys);

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> UpdateAsync(List<TEntity> entitys, bool ignoreNullColumns = false, CancellationToken cancellationToken = default)
        {
            var update = _freeSql.Update<TEntity>();

            if (ignoreNullColumns)
                update.SetSource(entitys);
            else
                update.SetSource(entitys);

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync();
        }

        public int UpdateColumns(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression)
        {
            return _freeSql.Update<TEntity>().Set(columns).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> UpdateColumnsAsync(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            return _freeSql.Update<TEntity>().Set(columns).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync();
        }


        public TEntity Get(object key)
        {
            return _freeSql.Select<TEntity>(key).WithTransaction(_unitOfWork?.Transaction).First();
        }

        public Task<TEntity> GetAsync(object key, CancellationToken cancellationToken = default)
        {
            return _freeSql.Select<TEntity>(key).WithTransaction(_unitOfWork?.Transaction).FirstAsync(cancellationToken);
        }

        public TEntity First(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _freeSql.Select<TEntity>().WithTransaction(_unitOfWork?.Transaction).First();
            else
                return _freeSql.Select<TEntity>().Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).First();
        }

        public Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression == null)
                return _freeSql.Select<TEntity>().WithTransaction(_unitOfWork?.Transaction).FirstAsync(cancellationToken);
            else
                return _freeSql.Select<TEntity>().Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).FirstAsync(cancellationToken);
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

            return queryable.WithTransaction(_unitOfWork?.Transaction).Take(topSize).ToList();
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

            return queryable.WithTransaction(_unitOfWork?.Transaction).Take(topSize).ToListAsync();
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
                return queryable.WithTransaction(_unitOfWork?.Transaction).ToList<TObject>();
            else
                return queryable.WithTransaction(_unitOfWork?.Transaction).ToList(selectByExpression);
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
                return queryable.WithTransaction(_unitOfWork?.Transaction).ToListAsync<TObject>();
            else
                return queryable.WithTransaction(_unitOfWork?.Transaction).ToListAsync(selectByExpression);
        }

        public List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression != null)
                return _freeSql.Select<TEntity>().Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ToList();
            return _freeSql.Select<TEntity>().WithTransaction(_unitOfWork?.Transaction).ToList();
        }

        public Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression != null)
                return _freeSql.Select<TEntity>().Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ToListAsync();
            return _freeSql.Select<TEntity>().WithTransaction(_unitOfWork?.Transaction).ToListAsync();
        }

        public List<TObject> Select<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            var select = _freeSql.Select<TEntity>();

            if (whereExpression != null)
                select.Where(whereExpression);

            if (selectByExpression == null)
                return select.WithTransaction(_unitOfWork?.Transaction).ToList<TObject>();
            else
                return select.WithTransaction(_unitOfWork?.Transaction).ToList(selectByExpression);
        }

        public Task<List<TObject>> SelectAsync<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
        {
            var select = _freeSql.Select<TEntity>();

            if (whereExpression != null)
                select.Where(whereExpression);

            if (selectByExpression == null)
                return select.WithTransaction(_unitOfWork?.Transaction).ToListAsync<TObject>();
            else
                return select.WithTransaction(_unitOfWork?.Transaction).ToListAsync(selectByExpression);
        }

        public PageResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null)
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

            var result = queryable.WithTransaction(_unitOfWork?.Transaction).Count(out var totalRows).Page(pageNumber, pageSize).ToList();

            return new PageResult<List<TEntity>>(pageNumber, pageSize, (int)totalRows, result);
        }

        public async Task<PageResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, CancellationToken cancellationToken = default)
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

            var result = await queryable.WithTransaction(_unitOfWork?.Transaction).Count(out var totalRows).Page(pageNumber, pageSize).ToListAsync();

            return new PageResult<List<TEntity>>(pageNumber, pageSize, (int)totalRows, result);
        }

        public PageResult<List<TObject>> Paged<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
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
                result = queryable.WithTransaction(_unitOfWork?.Transaction).ToList<TObject>();
            else
                result = queryable.WithTransaction(_unitOfWork?.Transaction).ToList(selectByExpression);

            return new PageResult<List<TObject>>(pageNumber, pageSize, (int)totalRows, result);
        }

        public async Task<PageResult<List<TObject>>> PagedAsync<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
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
                result = await queryable.WithTransaction(_unitOfWork?.Transaction).ToListAsync<TObject>();
            else
                result = await queryable.WithTransaction(_unitOfWork?.Transaction).ToListAsync(selectByExpression);

            return new PageResult<List<TObject>>(pageNumber, pageSize, (int)totalRows, result);
        }


        public int Count(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _freeSql.Select<TEntity>().WithTransaction(_unitOfWork?.Transaction).Count().To<int>();
            else
                return _freeSql.Select<TEntity>().Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).Count().To<int>();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression == null)
                return (await _freeSql.Select<TEntity>().WithTransaction(_unitOfWork?.Transaction).CountAsync(cancellationToken)).To<int>();
            else
                return (await _freeSql.Select<TEntity>().Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).CountAsync(cancellationToken)).To<int>();
        }

        public bool Exist(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _freeSql.Select<TEntity>().WithTransaction(_unitOfWork?.Transaction).Any();
            else
                return _freeSql.Select<TEntity>().Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).Any();
        }

        public Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression == null)
                return _freeSql.Select<TEntity>().WithTransaction(_unitOfWork?.Transaction).AnyAsync(cancellationToken);
            else
                return _freeSql.Select<TEntity>().Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).AnyAsync(cancellationToken);
        }

        public string GetDbTableName()
        {
            throw new NotImplementedException();
        }

        public List<string> GetDbColumnName()
        {
            throw new NotImplementedException();
        }
    }
}
