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
        private readonly IOptionsMonitor<SqlSugarOptions> _options;

        public SqlSugarRepository(SqlSugarClient sqlSugarClient, IUnitOfWork<SqlSugarClient> unitOfWork, IOptionsMonitor<SqlSugarOptions> options)
        {
            Check.NotNull(sqlSugarClient, nameof(sqlSugarClient));

            _sqlSugarClient = sqlSugarClient;
            _unitOfWork = unitOfWork;
            _options = options;

            // 判断数据源数量
            if (Options?.DataSource.Keys.Count <= 1)
                return;

            var primary = DataSourceMap.GetOrAdd(_entityType, () =>
            {
                var dataSourceAttribute = _entityType.GetAttribute<DataSourceAttribute>();
                return dataSourceAttribute?.Primary ?? Options?.Primary;
            });

            _sqlSugarClient.ChangeDatabase(primary);
        }

        private SqlSugarOptions Options
        {
            get
            {
                return _options?.CurrentValue;
            }
        }

        protected Type _entityType = typeof(TEntity);

        public IUnitOfWork GetUnitOfWork()
        {
            return _unitOfWork;
        }


        public int Insert(TEntity entity)
        {
            Check.NotNull(entity, nameof(entity));

            return _sqlSugarClient.Insertable<TEntity>(entity).ExecuteCommand();
        }

        public int Insert(IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, nameof(entities));

            return _sqlSugarClient.Insertable<TEntity>(entities).ExecuteCommand();
        }

        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Check.NotNull(entity, nameof(entity));

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            return _sqlSugarClient.Insertable<TEntity>(entity).ExecuteCommandAsync();
        }

        public Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            Check.NotNull(entities, nameof(entities));

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            return _sqlSugarClient.Insertable<TEntity>(entities).ExecuteCommandAsync();
        }


        public int Delete(dynamic key)
        {
            Check.NotNull(key, nameof(key));

            return _sqlSugarClient.Deleteable<TEntity>(key).ExecuteCommand();
        }

        public int Delete(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            var deleteable = _sqlSugarClient.Deleteable<TEntity>();

            if (whereExpression != null)
                deleteable.Where(whereExpression);

            return deleteable.ExecuteCommand();
        }

        public Task<int> DeleteAsync(dynamic key, CancellationToken cancellationToken = default)
        {
            Check.NotNull(key, nameof(key));

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            return _sqlSugarClient.Deleteable<TEntity>(key).ExecuteCommandAsync();
        }

        public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            var deleteable = _sqlSugarClient.Deleteable<TEntity>();

            if (whereExpression != null)
                deleteable.Where(whereExpression);

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            return deleteable.ExecuteCommandAsync();
        }


        public int Update(TEntity entity, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null)
        {
            var updateable = _sqlSugarClient.Updateable(entity);

            if (updateColumns != null)
                updateable.UpdateColumns(updateColumns);

            if (ignoreColumns != null)
                updateable.IgnoreColumns(ignoreColumns);

            if (whereExpression != null)
                updateable.Where(whereExpression);

            return updateable.ExecuteCommand();
        }

        public Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, CancellationToken cancellationToken = default)
        {
            var updateable = _sqlSugarClient.Updateable(entity);

            if (updateColumns != null)
                updateable.UpdateColumns(updateColumns);

            if (ignoreColumns != null)
                updateable.IgnoreColumns(ignoreColumns);

            if (whereExpression != null)
                updateable.Where(whereExpression);

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            return updateable.ExecuteCommandAsync();
        }


        public TEntity First(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression != null)
                return _sqlSugarClient.Queryable<TEntity>().First(whereExpression);

            return _sqlSugarClient.Queryable<TEntity>().First();
        }

        public Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            if (whereExpression != null)
                return _sqlSugarClient.Queryable<TEntity>().FirstAsync(whereExpression);

            return _sqlSugarClient.Queryable<TEntity>().FirstAsync();
        }

        public TEntity Get(dynamic key)
        {
            Check.NotNull(key, nameof(key));

            return _sqlSugarClient.Queryable<TEntity>().InSingle(key);
        }

        public Task<TEntity> GetAsync(dynamic key, CancellationToken cancellationToken = default)
        {
            Check.NotNull(key, nameof(key));

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            return _sqlSugarClient.Queryable<TEntity>().InSingleAsync(key);
        }

        public List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            return queryable.ToList();
        }

        public Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            return queryable.ToListAsync();
        }

        public List<TEntity> Top(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null)
            {
                if (ascending)
                    queryable.OrderBy(orderByExpression, OrderByType.Asc);
                else
                    queryable.OrderBy(orderByExpression, OrderByType.Desc);
            }

            return queryable.Take(topSize).ToList();
        }

        public Task<List<TEntity>> TopAsync(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false, CancellationToken cancellationToken = default)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null)
            {
                if (ascending)
                    queryable.OrderBy(orderByExpression, OrderByType.Asc);
                else
                    queryable.OrderBy(orderByExpression, OrderByType.Desc);
            }

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            return queryable.Take(topSize).ToListAsync();
        }

        public PagedResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null)
            {
                if (ascending)
                    queryable.OrderBy(orderByExpression, OrderByType.Asc);
                else
                    queryable.OrderBy(orderByExpression, OrderByType.Desc);
            }

            int totalRows = 0;

            var result = queryable.ToPageList(pageNumber, pageSize, ref totalRows);

            return new PagedResult<List<TEntity>>(pageSize, pageNumber, totalRows, result);
        }

        public async Task<PagedResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false, CancellationToken cancellationToken = default)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null)
            {
                if (ascending)
                    queryable.OrderBy(orderByExpression, OrderByType.Asc);
                else
                    queryable.OrderBy(orderByExpression, OrderByType.Desc);
            }

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            RefAsync<int> totalRows = 0;

            var result = await queryable.ToPageListAsync(pageNumber, pageSize, totalRows);

            return new PagedResult<List<TEntity>>(pageSize, pageNumber, totalRows.Value, result);
        }

        public int Count(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression != null)
                return _sqlSugarClient.Queryable<TEntity>().Count(whereExpression);

            return _sqlSugarClient.Queryable<TEntity>().Count();
        }

        public Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            if (whereExpression != null)
                return _sqlSugarClient.Queryable<TEntity>().CountAsync(whereExpression);

            return _sqlSugarClient.Queryable<TEntity>().CountAsync();
        }

        public bool Exist(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression != null)
                return _sqlSugarClient.Queryable<TEntity>().Any(whereExpression);

            return _sqlSugarClient.Queryable<TEntity>().Any();
        }

        public Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            if (whereExpression != null)
                return _sqlSugarClient.Queryable<TEntity>().AnyAsync(whereExpression);

            return _sqlSugarClient.Queryable<TEntity>().AnyAsync();
        }
    }
}
