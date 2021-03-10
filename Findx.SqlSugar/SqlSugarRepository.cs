using Findx.Data;
using Findx.Extensions;
using Microsoft.Extensions.Options;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.SqlSugar
{
    public class SqlSugarRepository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        private readonly static IDictionary<Type, string> DataSourceMap = new Dictionary<Type, string>();
        private readonly SqlSugarClient _sqlSugarClient;
        private readonly IUnitOfWork<SqlSugarClient> _unitOfWork;
        public SqlSugarRepository(SqlSugarClient sqlSugarClient, IUnitOfWork<SqlSugarClient> unitOfWork, IOptionsMonitor<SqlSugarOptions> options)
        {
            Check.NotNull(sqlSugarClient, nameof(sqlSugarClient));

            _sqlSugarClient = sqlSugarClient;
            _unitOfWork = unitOfWork;

            Type entityType = typeof(TEntity);
            string primary = DataSourceMap.GetOrAdd(entityType, () =>
            {
                DataSourceAttribute dataSourceAttribute = entityType.GetAttribute<DataSourceAttribute>();
                return dataSourceAttribute?.Primary ?? options?.CurrentValue?.Primary;
            });

            _sqlSugarClient.ChangeDatabase(primary);
        }

        public int Count(Expression<Func<TEntity, bool>> whereExpression)
        {
            return _sqlSugarClient.Queryable<TEntity>().Count(whereExpression);
        }

        public Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            return _sqlSugarClient.Queryable<TEntity>().CountAsync(whereExpression);
        }




        public int Delete(dynamic key)
        {
            return _sqlSugarClient.Deleteable<TEntity>(key).ExecuteCommand();
        }

        public int Delete(Expression<Func<TEntity, bool>> whereExpression)
        {
            return _sqlSugarClient.Deleteable<TEntity>().Where(whereExpression).ExecuteCommand();
        }

        public Task<int> DeleteAsync(dynamic key, CancellationToken cancellationToken = default)
        {
            _sqlSugarClient.Ado.CancellationToken = cancellationToken;
            return _sqlSugarClient.Deleteable<TEntity>(key).ExecuteCommandAsync();
        }

        public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            _sqlSugarClient.Ado.CancellationToken = cancellationToken;
            return _sqlSugarClient.Deleteable<TEntity>().Where(whereExpression).ExecuteCommandAsync();
        }




        public bool Exist(Expression<Func<TEntity, bool>> whereExpression)
        {
            return _sqlSugarClient.Queryable<TEntity>().Any(whereExpression);
        }

        public Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            _sqlSugarClient.Ado.CancellationToken = cancellationToken;
            return _sqlSugarClient.Queryable<TEntity>().AnyAsync(whereExpression);
        }

        public TEntity First(Expression<Func<TEntity, bool>> whereExpression)
        {
            return _sqlSugarClient.Queryable<TEntity>().First(whereExpression);
        }

        public Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            _sqlSugarClient.Ado.CancellationToken = cancellationToken;
            return _sqlSugarClient.Queryable<TEntity>().FirstAsync(whereExpression);
        }

        public TEntity Get(dynamic key)
        {
            return _sqlSugarClient.Queryable<TEntity>().InSingle(key);
        }

        public Task<TEntity> GetAsync(dynamic key, CancellationToken cancellationToken = default)
        {
            return _sqlSugarClient.Queryable<TEntity>().InSingleAsync(key);
        }




        public IUnitOfWork GetUnitOfWork()
        {
            return _unitOfWork;
        }



        public int Insert(TEntity entity)
        {
            return _sqlSugarClient.Insertable(entity).ExecuteCommand();
        }

        public int Insert(IEnumerable<TEntity> entities)
        {
            return _sqlSugarClient.Insertable<TEntity>(entities).ExecuteCommand();
        }

        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return _sqlSugarClient.Insertable(entity).ExecuteCommandAsync();
        }

        public Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return _sqlSugarClient.Insertable<TEntity>(entities).ExecuteCommandAsync();
        }




        public PagedResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>().Where(whereExpression);
            if (orderByExpression != null)
            {
                if (ascending)
                    queryable.OrderBy(orderByExpression, OrderByType.Asc);
                else
                    queryable.OrderBy(orderByExpression, OrderByType.Desc);
            }

            var result = queryable.ToPageList(pageNumber, pageSize);

            return new PagedResult<List<TEntity>>(pageSize, pageNumber, result);
        }

        public async Task<PagedResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false, CancellationToken cancellationToken = default)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>().Where(whereExpression);
            if (orderByExpression != null)
            {
                if (ascending)
                    queryable.OrderBy(orderByExpression, OrderByType.Asc);
                else
                    queryable.OrderBy(orderByExpression, OrderByType.Desc);
            }

            var result = await queryable.ToPageListAsync(pageNumber, pageSize);

            return new PagedResult<List<TEntity>>(pageSize, pageNumber, result);
        }

        public List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression)
        {
            return _sqlSugarClient.Queryable<TEntity>().Where(whereExpression).ToList();
        }

        public Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            return _sqlSugarClient.Queryable<TEntity>().Where(whereExpression).ToListAsync();
        }



        public int Update(TEntity entity, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null)
        {
            var updateable = _sqlSugarClient.Updateable(entity);
            if (updateColumns != null)
                updateable.UpdateColumns(updateColumns);
            if (ignoreColumns != null)
                updateable.IgnoreColumns(ignoreColumns);
            return updateable.ExecuteCommand();
        }

        public int Update(TEntity entity, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null)
        {
            var updateable = _sqlSugarClient.Updateable(entity);
            if (updateColumns != null)
                updateable.UpdateColumns(updateColumns);
            if (ignoreColumns != null)
                updateable.IgnoreColumns(ignoreColumns);
            return updateable.Where(whereExpression).ExecuteCommand();
        }

        public Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, CancellationToken cancellationToken = default)
        {
            var updateable = _sqlSugarClient.Updateable(entity);
            if (updateColumns != null)
                updateable.UpdateColumns(updateColumns);
            if (ignoreColumns != null)
                updateable.IgnoreColumns(ignoreColumns);
            return updateable.ExecuteCommandAsync();
        }

        public Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, CancellationToken cancellationToken = default)
        {
            var updateable = _sqlSugarClient.Updateable(entity);
            if (updateColumns != null)
                updateable.UpdateColumns(updateColumns);
            if (ignoreColumns != null)
                updateable.IgnoreColumns(ignoreColumns);
            return updateable.Where(whereExpression).ExecuteCommandAsync();
        }
    }
}
