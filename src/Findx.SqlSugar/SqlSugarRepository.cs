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

            if (Options?.DataSource.Keys.Count <= 1) return;

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


        public int Update(TEntity entity, bool ignoreNullColumns = false)
        {
            var updateable = _sqlSugarClient.Updateable(entity);

            if (ignoreNullColumns)
                updateable.IgnoreColumns(ignoreAllNullColumns: true, ignoreAllDefaultValue: true);

            return updateable.ExecuteCommand();
        }

        public Task<int> UpdateAsync(TEntity entity, bool ignoreNullColumns = false, CancellationToken cancellationToken = default)
        {
            var updateable = _sqlSugarClient.Updateable(entity);

            if (ignoreNullColumns)
                updateable.IgnoreColumns(ignoreAllNullColumns: true, ignoreAllDefaultValue: true);

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            return updateable.ExecuteCommandAsync();
        }

        public int Update(List<TEntity> entitys, bool ignoreNullColumns = false)
        {
            var updateable = _sqlSugarClient.Updateable(entitys);

            if (ignoreNullColumns)
                updateable.IgnoreColumns(ignoreAllNullColumns: true, ignoreAllDefaultValue: true);

            return updateable.ExecuteCommand();
        }

        public Task<int> UpdateAsync(List<TEntity> entitys, bool ignoreNullColumns = false, CancellationToken cancellationToken = default)
        {
            var updateable = _sqlSugarClient.Updateable(entitys);

            if (ignoreNullColumns)
                updateable.IgnoreColumns(ignoreAllNullColumns: true, ignoreAllDefaultValue: true);

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            return updateable.ExecuteCommandAsync();
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



        public int UpdateColumns(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression)
        {
            return _sqlSugarClient.Updateable<TEntity>().SetColumns(columns).Where(whereExpression).ExecuteCommand();
        }

        public Task<int> UpdateColumnsAsync(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            return _sqlSugarClient.Updateable<TEntity>().SetColumns(columns).Where(whereExpression).ExecuteCommandAsync();
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

        public List<TObject> Select<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (selectByExpression == null)
                return queryable.Select<TObject>().ToList();
            else
                return queryable.Select(selectByExpression).ToList();
        }

        public Task<List<TObject>> SelectAsync<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            if (selectByExpression == null)
                return queryable.Select<TObject>().ToListAsync();
            else
                return queryable.Select(selectByExpression).ToListAsync();
        }

        public List<TEntity> Top(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            return queryable.Take(topSize).ToList();
        }

        public Task<List<TEntity>> TopAsync(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            return queryable.Take(topSize).ToListAsync();
        }

        public List<TObject> Top<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            if (selectByExpression == null)
                return queryable.Take(topSize).Select<TObject>().ToList();
            else
                return queryable.Take(topSize).Select(selectByExpression).ToList();
        }

        public Task<List<TObject>> TopAsync<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            if (selectByExpression == null)
                return queryable.Take(topSize).Select<TObject>().ToListAsync();
            else
                return queryable.Take(topSize).Select(selectByExpression).ToListAsync();
        }

        public PagedResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            int totalRows = 0;

            var result = queryable.ToPageList(pageNumber, pageSize, ref totalRows);

            return new PagedResult<List<TEntity>>(pageNumber, pageSize, totalRows, result);
        }

        public async Task<PagedResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            RefAsync<int> totalRows = 0;

            var result = await queryable.ToPageListAsync(pageNumber, pageSize, totalRows);

            return new PagedResult<List<TEntity>>(pageNumber, pageSize, totalRows.Value, result);
        }

        public PagedResult<List<TObject>> Paged<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            int totalRows = 0;

            if (selectByExpression == null)
            {
                var result = queryable.Select<TObject>().ToPageList(pageNumber, pageSize, ref totalRows);
                return new PagedResult<List<TObject>>(pageNumber, pageSize, totalRows, result);
            }
            else
            {
                var result = queryable.Select(selectByExpression).ToPageList(pageNumber, pageSize, ref totalRows);
                return new PagedResult<List<TObject>>(pageNumber, pageSize, totalRows, result);
            }
        }

        public async Task<PagedResult<List<TObject>>> PagedAsync<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _sqlSugarClient.Queryable<TEntity>();

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            _sqlSugarClient.Ado.CancellationToken = cancellationToken;

            RefAsync<int> totalRows = 0;

            if (selectByExpression == null)
            {
                var result = await queryable.Select<TObject>().ToPageListAsync(pageNumber, pageSize, totalRows);
                return new PagedResult<List<TObject>>(pageNumber, pageSize, totalRows, result);
            }
            else
            {
                var result = await queryable.Select(selectByExpression).ToPageListAsync(pageNumber, pageSize, totalRows);
                return new PagedResult<List<TObject>>(pageNumber, pageSize, totalRows, result);
            }
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
