using Findx.Data;
using Findx.Extensions;
using Microsoft.Extensions.Options;
using SqlSugar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.SqlSugar
{
    public class SqlSugarRepository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        private readonly static IDictionary<Type, DataEntityAttribute> DataEntityMap = new ConcurrentDictionary<Type, DataEntityAttribute>();
        private readonly static IDictionary<Type, (bool softDeletable, bool customSharding)> BaseOnMap = new ConcurrentDictionary<Type, (bool softDeletable, bool customSharding)>();

        private readonly Type _entityType = typeof(TEntity);
        private readonly SqlSugarProvider _sugar;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptionsMonitor<SqlSugarOptions> _options;
        private readonly DataEntityAttribute _attribute;
        private readonly bool _softDeletable;
        private readonly bool _customSharding;
        private readonly string _softDeletableFilterName = "SoftDeletable";

        private string _oldTableName;
        private string _tableName;
        public SqlSugarRepository(SqlSugarClient sqlSugarClient, IUnitOfWorkManager uowManager, IOptionsMonitor<SqlSugarOptions> options)
        {
            Check.NotNull(sqlSugarClient, nameof(sqlSugarClient));
            var js = DateTime.Now;

            Check.NotNull(options.CurrentValue, "SqlSugarOptions");

            _options = options;

            _attribute = DataEntityMap.GetOrAdd(_entityType, () => { return _entityType.GetAttribute<DataEntityAttribute>(); });

            var primary = _attribute?.DataSource ?? Options.Primary ?? "";

            _sugar = sqlSugarClient.GetConnection(primary);

            // Check异常
            if (Options.Strict) Check.NotNull(_sugar, nameof(_sugar));

            // 使用默认库
            if (_sugar == null)
            {
                primary = Options.Primary;
                _sugar = sqlSugarClient.GetConnection(primary);
                Check.NotNull(_sugar, nameof(_sugar));
            }

            // 获取工作单元
            _unitOfWork = uowManager.GetConnUnitOfWork(primary);

            // 基类标记
            var baseOns = BaseOnMap.GetOrAdd(_entityType, () =>
            {
                // 是否标记实体逻辑删除
                var softDeletable = _entityType.IsBaseOn(typeof(ISoftDeletable));
                // 是否标记自定义分表函数
                var customSharding = _entityType.IsBaseOn(typeof(ITableSharding));
                return (softDeletable, customSharding);
            });
            _softDeletable = baseOns.softDeletable && Options.SoftDeletable; // 全局和
            _customSharding = baseOns.customSharding;

            // 初始化分表计算
            _tableName = _oldTableName = _sugar.EntityMaintenance.GetEntityInfo<TEntity>().DbTableName;
            if (_attribute?.TableShardingType == ShardingType.Time)
            {
                _tableName = $"{_oldTableName}_{DateTime.Now.ToString(_attribute.TableShardingExt)}";
            }

            Debug.WriteLine($"仓储构造函数耗时:{(DateTime.Now - js).TotalMilliseconds:0.000}毫秒");
        }

        private SqlSugarOptions Options
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

        #region 插入
        public int Insert(TEntity entity)
        {
            Check.NotNull(entity, nameof(entity));

            return _sugar.Insertable<TEntity>(entity).AS(_tableName).ExecuteCommand();
        }

        public int Insert(List<TEntity> entities)
        {
            Check.NotNull(entities, nameof(entities));

            return _sugar.Insertable<TEntity>(entities).AS(_tableName).ExecuteCommand();
        }

        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Check.NotNull(entity, nameof(entity));

            _sugar.Ado.CancellationToken = cancellationToken;

            return _sugar.Insertable<TEntity>(entity).AS(_tableName).ExecuteCommandAsync();
        }

        public Task<int> InsertAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
        {
            Check.NotNull(entities, nameof(entities));

            _sugar.Ado.CancellationToken = cancellationToken;

            return _sugar.Insertable<TEntity>(entities).AS(_tableName).ExecuteCommandAsync();
        }
        #endregion

        #region 删除
        public int Delete(object key)
        {
            Check.NotNull(key, nameof(key));

            if (_softDeletable)
            {
                var model = _sugar.Queryable<TEntity>().AS(_tableName).InSingle(key);
                if (model != null)
                {
                    var softDeletable = model as ISoftDeletable;
                    softDeletable.Deleted = true;
                    softDeletable.DeletedTime = DateTime.Now;

                    _sugar.Updateable(model).AS(_tableName).UpdateColumns(it => new { (it as ISoftDeletable).Deleted, (it as ISoftDeletable).DeletedTime }).ExecuteCommand();
                }
            }

            return _sugar.Deleteable<TEntity>(key).AS(_tableName).ExecuteCommand();
        }

        public int Delete(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (_softDeletable && whereExpression == null)
            {
                return _sugar.Updateable<TEntity>().AS(_tableName).SetColumns(it => (it as ISoftDeletable).Deleted == true).SetColumns(it => (it as ISoftDeletable).DeletedTime == DateTime.Now).ExecuteCommand();
            }

            if (_softDeletable && whereExpression != null)
            {
                return _sugar.Updateable<TEntity>().AS(_tableName).SetColumns(it => (it as ISoftDeletable).Deleted == true).SetColumns(it => (it as ISoftDeletable).DeletedTime == DateTime.Now).Where(whereExpression).ExecuteCommand();
            }

            var deleteable = _sugar.Deleteable<TEntity>().AS(_tableName);

            if (whereExpression != null)
                deleteable.Where(whereExpression);

            return deleteable.ExecuteCommand();
        }

        public Task<int> DeleteAsync(object key, CancellationToken cancellationToken = default)
        {
            Check.NotNull(key, nameof(key));

            _sugar.Ado.CancellationToken = cancellationToken;

            if (_softDeletable)
            {
                var model = _sugar.Queryable<TEntity>().AS(_tableName).InSingle(key);
                if (model != null)
                {
                    var softDeletable = model as ISoftDeletable;
                    softDeletable.Deleted = true;
                    softDeletable.DeletedTime = DateTime.Now;

                    _sugar.Updateable(model).AS(_tableName).UpdateColumns(it => new { (it as ISoftDeletable).Deleted, (it as ISoftDeletable).DeletedTime }).ExecuteCommand();
                }
            }

            return _sugar.Deleteable<TEntity>(key).AS(_tableName).ExecuteCommandAsync();
        }

        public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (_softDeletable && whereExpression == null)
            {
                return _sugar.Updateable<TEntity>().AS(_tableName).SetColumns(it => (it as ISoftDeletable).Deleted == true).SetColumns(it => (it as ISoftDeletable).DeletedTime == DateTime.Now).ExecuteCommandAsync();
            }

            if (_softDeletable && whereExpression != null)
            {
                return _sugar.Updateable<TEntity>().AS(_tableName).SetColumns(it => (it as ISoftDeletable).Deleted == true).SetColumns(it => (it as ISoftDeletable).DeletedTime == DateTime.Now).Where(whereExpression).ExecuteCommandAsync();
            }

            var deleteable = _sugar.Deleteable<TEntity>().AS(_tableName);

            if (whereExpression != null)
                deleteable.Where(whereExpression);

            _sugar.Ado.CancellationToken = cancellationToken;

            return deleteable.ExecuteCommandAsync();
        }
        #endregion

        #region 更新
        public int Update(TEntity entity, bool ignoreNullColumns = false)
        {
            var updateable = _sugar.Updateable(entity).AS(_tableName);

            if (ignoreNullColumns)
                updateable.IgnoreColumns(ignoreAllNullColumns: true, ignoreAllDefaultValue: true);

            return updateable.ExecuteCommand();
        }

        public Task<int> UpdateAsync(TEntity entity, bool ignoreNullColumns = false, CancellationToken cancellationToken = default)
        {
            var updateable = _sugar.Updateable(entity).AS(_tableName);

            if (ignoreNullColumns)
                updateable.IgnoreColumns(ignoreAllNullColumns: true, ignoreAllDefaultValue: true);

            _sugar.Ado.CancellationToken = cancellationToken;

            return updateable.ExecuteCommandAsync();
        }

        public int Update(List<TEntity> entitys)
        {
            var updateable = _sugar.Updateable(entitys).AS(_tableName);
            return updateable.ExecuteCommand();
        }

        public Task<int> UpdateAsync(List<TEntity> entitys, CancellationToken cancellationToken = default)
        {
            var updateable = _sugar.Updateable(entitys).AS(_tableName);
            _sugar.Ado.CancellationToken = cancellationToken;

            return updateable.ExecuteCommandAsync();
        }

        public int Update(TEntity entity, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null)
        {
            var updateable = _sugar.Updateable(entity).AS(_tableName);

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
            var updateable = _sugar.Updateable(entity).AS(_tableName);

            if (updateColumns != null)
                updateable.UpdateColumns(updateColumns);

            if (ignoreColumns != null)
                updateable.IgnoreColumns(ignoreColumns);

            if (whereExpression != null)
                updateable.Where(whereExpression);

            _sugar.Ado.CancellationToken = cancellationToken;

            return updateable.ExecuteCommandAsync();
        }

        public int UpdateColumns(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression)
        {
            return _sugar.Updateable<TEntity>().AS(_tableName).SetColumns(columns).Where(whereExpression).ExecuteCommand();
        }

        public Task<int> UpdateColumnsAsync(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            _sugar.Ado.CancellationToken = cancellationToken;

            return _sugar.Updateable<TEntity>().AS(_tableName).SetColumns(columns).Where(whereExpression).ExecuteCommandAsync();
        }

        public int UpdateColumns(List<Expression<Func<TEntity, TEntity>>> columns, Expression<Func<TEntity, bool>> whereExpression)
        {
            Check.NotNull(columns, nameof(columns));
            Check.NotNull(whereExpression, nameof(whereExpression));

            var update = _sugar.Updateable<TEntity>().AS(_tableName);

            foreach (var item in columns)
            {
                update.SetColumns(item);
            }

            return update.Where(whereExpression).ExecuteCommand();
        }

        public Task<int> UpdateColumnsAsync(List<Expression<Func<TEntity, TEntity>>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            Check.NotNull(columns, nameof(columns));
            Check.NotNull(whereExpression, nameof(whereExpression));

            var update = _sugar.Updateable<TEntity>().AS(_tableName);

            foreach (var item in columns)
            {
                update.SetColumns(item);
            }

            return update.Where(whereExpression).ExecuteCommandAsync();
        }
        #endregion

        #region 查询
        public TEntity First(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                return queryable.First(whereExpression);

            return queryable.First();
        }

        public Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            _sugar.Ado.CancellationToken = cancellationToken;

            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                return queryable.FirstAsync(whereExpression);

            return queryable.FirstAsync();
        }

        public TEntity Get(object key)
        {
            Check.NotNull(key, nameof(key));

            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            return queryable.InSingle(key);
        }

        public Task<TEntity> GetAsync(object key, CancellationToken cancellationToken = default)
        {
            Check.NotNull(key, nameof(key));

            _sugar.Ado.CancellationToken = cancellationToken;

            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            return queryable.InSingleAsync(key);
        }



        public List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            return queryable.ToList();
        }

        public Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            _sugar.Ado.CancellationToken = cancellationToken;

            return queryable.ToListAsync();
        }

        public List<TObject> Select<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (selectByExpression == null)
                return queryable.Select<TObject>().ToList();
            else
                return queryable.Select(selectByExpression).ToList();
        }

        public Task<List<TObject>> SelectAsync<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            _sugar.Ado.CancellationToken = cancellationToken;

            if (selectByExpression == null)
                return queryable.Select<TObject>().ToListAsync();
            else
                return queryable.Select(selectByExpression).ToListAsync();
        }



        public List<TEntity> Top(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            return queryable.Take(topSize).ToList();
        }

        public Task<List<TEntity>> TopAsync(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            _sugar.Ado.CancellationToken = cancellationToken;

            return queryable.Take(topSize).ToListAsync();
        }

        public List<TObject> Top<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
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
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
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



        public PageResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            int totalRows = 0;

            var result = queryable.ToPageList(pageNumber, pageSize, ref totalRows);

            return new PageResult<List<TEntity>>(pageNumber, pageSize, totalRows, result);
        }

        public async Task<PageResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            _sugar.Ado.CancellationToken = cancellationToken;

            RefAsync<int> totalRows = 0;

            var result = await queryable.ToPageListAsync(pageNumber, pageSize, totalRows);

            return new PageResult<List<TEntity>>(pageNumber, pageSize, totalRows.Value, result);
        }

        public PageResult<List<TObject>> Paged<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            int totalRows = 0;

            if (selectByExpression == null)
            {
                var result = queryable.Select<TObject>().ToPageList(pageNumber, pageSize, ref totalRows);
                return new PageResult<List<TObject>>(pageNumber, pageSize, totalRows, result);
            }
            else
            {
                var result = queryable.Select(selectByExpression).ToPageList(pageNumber, pageSize, ref totalRows);
                return new PageResult<List<TObject>>(pageNumber, pageSize, totalRows, result);
            }
        }

        public async Task<PageResult<List<TObject>>> PagedAsync<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression, OrderByType.Asc);
                    else
                        queryable.OrderBy(item.Expression, OrderByType.Desc);
                }
            }

            _sugar.Ado.CancellationToken = cancellationToken;

            RefAsync<int> totalRows = 0;

            if (selectByExpression == null)
            {
                var result = await queryable.Select<TObject>().ToPageListAsync(pageNumber, pageSize, totalRows);
                return new PageResult<List<TObject>>(pageNumber, pageSize, totalRows, result);
            }
            else
            {
                var result = await queryable.Select(selectByExpression).ToPageListAsync(pageNumber, pageSize, totalRows);
                return new PageResult<List<TObject>>(pageNumber, pageSize, totalRows, result);
            }
        }
        #endregion

        #region 函数查询
        public int Count(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                return queryable.Count(whereExpression);

            return queryable.Count();
        }

        public Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            _sugar.Ado.CancellationToken = cancellationToken;

            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                return queryable.CountAsync(whereExpression);

            return queryable.CountAsync();
        }



        public bool Exist(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                return queryable.Any(whereExpression);

            return queryable.Any();
        }

        public Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            _sugar.Ado.CancellationToken = cancellationToken;

            var queryable = _sugar.Queryable<TEntity>().AS(_tableName);

            if (_softDeletable)
                queryable.Filter(_softDeletableFilterName);

            if (whereExpression != null)
                return queryable.AnyAsync(whereExpression);

            return queryable.AnyAsync();
        }
        #endregion

        #region 库表
        public string GetDbTableName()
        {
            var entityInfo = _sugar.EntityMaintenance.GetEntityInfo<TEntity>();
            return entityInfo.DbTableName;
        }

        public List<string> GetDbColumnName()
        {
            var list = new List<string>();
            var entityInfo = _sugar.EntityMaintenance.GetEntityInfo<TEntity>();
            foreach (var item in entityInfo.Columns)
            {
                list.Add(item.DbColumnName);
            }
            return list;
        }

        public IRepository<TEntity> AsTable(Func<string, string> tableRule)
        {
            _tableName = tableRule.Invoke(_oldTableName);

            return this;
        }
        #endregion
    }
}
