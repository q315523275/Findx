using Findx.Data;
using Findx.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.FreeSql
{
    public class FreeSqlRepository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        private readonly static IDictionary<Type, DataEntityAttribute> DataEntityMap = new ConcurrentDictionary<Type, DataEntityAttribute>();
        private readonly static IDictionary<Type, (bool softDeletable, bool customSharding)> BaseOnMap = new ConcurrentDictionary<Type, (bool softDeletable, bool customSharding)>();

        private readonly IFreeSql _fsql;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptionsMonitor<FreeSqlOptions> _options;
        private readonly DataEntityAttribute _attribute;
        private readonly bool _softDeletable;
        private readonly bool _customSharding;

        private readonly Type EntityType = typeof(TEntity);
        internal Func<string, string> AsTableValueInternal { get; private set; }
        internal Func<Type, string, string> AsTableSelectValueInternal { get; private set; }

        public FreeSqlRepository(FreeSqlClient clients, IUnitOfWorkManager uowManager, IOptionsMonitor<FreeSqlOptions> options)
        {
            var js = DateTime.Now;

            Check.NotNull(options.CurrentValue, "FreeSqlOptions");

            _options = options;

            _attribute = DataEntityMap.GetOrAdd(EntityType , () => { return EntityType .GetAttribute<DataEntityAttribute>(); });

            var primary = _attribute?.DataSource ?? Options.Primary ?? "";

            clients.TryGetValue(primary, out _fsql);

            // Check异常
            if (Options.Strict) Check.NotNull(_fsql, nameof(_fsql));

            // 使用默认库
            if (_fsql == null)
            {
                primary = Options.Primary;
                clients.TryGetValue(Options.Primary, out _fsql);
                Check.NotNull(_fsql, nameof(_fsql));
            }

            // 获取工作单元
            _unitOfWork = uowManager.GetConnUnitOfWork(primary);

            // 基类标记
            var baseOns = BaseOnMap.GetOrAdd(EntityType , () =>
            {
                // 是否标记实体逻辑删除
                var softDeletable = EntityType .IsBaseOn(typeof(ISoftDeletable));
                // 是否标记自定义分表函数
                var customSharding = EntityType .IsBaseOn(typeof(ITableSharding));
                return (softDeletable, customSharding);
            });
            _softDeletable = baseOns.softDeletable;
            _customSharding = baseOns.customSharding;

            // 初始化分表计算
            if (_attribute?.TableShardingType == ShardingType.Time)
            {
                AsTableValueInternal = (oldName) => $"{oldName}_{DateTime.Now.ToString(_attribute.TableShardingExt)}";
                AsTableSelectValueInternal = (type, oldName) => $"{oldName}_{DateTime.Now.ToString(_attribute.TableShardingExt)}";
            }

            Debug.WriteLine($"仓储构造函数耗时:{(DateTime.Now - js).TotalMilliseconds:0.000}毫秒");
        }

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

        #region 插入
        public int Insert(TEntity entity)
        {
            return _fsql.Insert(entity).AsTable(AsTableValueInternal).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public int Insert(List<TEntity> entities)
        {
            return _fsql.Insert(entities).AsTable(AsTableValueInternal).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return _fsql.Insert(entity).AsTable(AsTableValueInternal).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }

        public Task<int> InsertAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return _fsql.Insert(entities).AsTable(AsTableValueInternal).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }
        #endregion

        #region 删除
        public int Delete(object key)
        {
            if (_softDeletable)
            {
                return _fsql.Update<TEntity>(key).AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).Deleted == true).Set(it => (it as ISoftDeletable).DeletedTime == DateTime.Now).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
            }

            return _fsql.Delete<TEntity>(key).AsTable(AsTableValueInternal).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> DeleteAsync(object key, CancellationToken cancellationToken = default)
        {
            if (_softDeletable)
            {
                return _fsql.Update<TEntity>(key).AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).Deleted == true).Set(it => (it as ISoftDeletable).DeletedTime == DateTime.Now).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            }

            return _fsql.Delete<TEntity>(key).AsTable(AsTableValueInternal).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync();
        }

        public int Delete(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (_softDeletable && whereExpression == null)
            {
                return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).Deleted == true).Set(it => (it as ISoftDeletable).DeletedTime == DateTime.Now).Where(it => true).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
            }

            if (_softDeletable && whereExpression != null)
            {
                return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).Deleted == true).Set(it => (it as ISoftDeletable).DeletedTime == DateTime.Now).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
            }

            if (whereExpression == null)
                return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(it => true).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
            else
                return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (_softDeletable && whereExpression == null)
            {
                return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).Deleted == true).Set(it => (it as ISoftDeletable).DeletedTime == DateTime.Now).Where(it => true).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            }

            if (_softDeletable && whereExpression != null)
            {
                return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).Deleted == true).Set(it => (it as ISoftDeletable).DeletedTime == DateTime.Now).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            }

            if (whereExpression == null)
                return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(it => true).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            else
                return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }
        #endregion

        #region 更新
        public int Update(TEntity entity, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null)
        {
            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal).SetSource(entity);

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
            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal).SetSource(entity);

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
            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal);

            if (ignoreNullColumns)
                update.SetSourceIgnore(entity, col => col == null);
            else
                update.SetSource(entity);

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> UpdateAsync(TEntity entity, bool ignoreNullColumns = false, CancellationToken cancellationToken = default)
        {
            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal);

            if (ignoreNullColumns)
                update.SetSourceIgnore(entity, col => col == null);
            else
                update.SetSource(entity);

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync();
        }

        public int Update(List<TEntity> entitys)
        {
            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal).SetSource(entitys);

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> UpdateAsync(List<TEntity> entitys, CancellationToken cancellationToken = default)
        {
            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal).SetSource(entitys);

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync();
        }

        public int UpdateColumns(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression)
        {
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(columns).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> UpdateColumnsAsync(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(columns).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync();
        }

        public int UpdateColumns(List<Expression<Func<TEntity, bool>>> columns, Expression<Func<TEntity, bool>> whereExpression)
        {
            Check.NotNull(columns, nameof(columns));
            Check.NotNull(whereExpression, nameof(whereExpression));

            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal);

            foreach (var item in columns)
            {
                update.Set(item);
            }

            return update.Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> UpdateColumnsAsync(List<Expression<Func<TEntity, bool>>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            Check.NotNull(columns, nameof(columns));
            Check.NotNull(whereExpression, nameof(whereExpression));

            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal);

            foreach (var item in columns)
            {
                update.Set(item);
            }

            return update.Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync();
        }

        #endregion

        #region 查询
        public TEntity Get(object key)
        {
            return _fsql.Select<TEntity>(key).AsTable(AsTableSelectValueInternal).WithTransaction(_unitOfWork?.Transaction).First();
        }

        public Task<TEntity> GetAsync(object key, CancellationToken cancellationToken = default)
        {
            return _fsql.Select<TEntity>(key).AsTable(AsTableSelectValueInternal).WithTransaction(_unitOfWork?.Transaction).FirstAsync(cancellationToken);
        }

        public TEntity First(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(_unitOfWork?.Transaction).First();
            else
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).First();
        }

        public Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression == null)
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(_unitOfWork?.Transaction).FirstAsync(cancellationToken);
            else
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).FirstAsync(cancellationToken);
        }

        public List<TEntity> Top(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null)
        {
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            return queryable.WithTransaction(_unitOfWork?.Transaction).Take(topSize).ToList();
        }

        public Task<List<TEntity>> TopAsync(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, CancellationToken cancellationToken = default)
        {
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            return queryable.WithTransaction(_unitOfWork?.Transaction).Take(topSize).ToListAsync();
        }

        public List<TObject> Top<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
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
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
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
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ToList();
            return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(_unitOfWork?.Transaction).ToList();
        }

        public Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression != null)
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ToListAsync();
            return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(_unitOfWork?.Transaction).ToListAsync();
        }

        public List<TObject> Select<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null)
        {
            var select = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                select.Where(whereExpression);

            if (selectByExpression == null)
                return select.WithTransaction(_unitOfWork?.Transaction).ToList<TObject>();
            else
                return select.WithTransaction(_unitOfWork?.Transaction).ToList(selectByExpression);
        }

        public Task<List<TObject>> SelectAsync<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default)
        {
            var select = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                select.Where(whereExpression);

            if (selectByExpression == null)
                return select.WithTransaction(_unitOfWork?.Transaction).ToListAsync<TObject>();
            else
                return select.WithTransaction(_unitOfWork?.Transaction).ToListAsync(selectByExpression);
        }

        public PageResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null)
        {
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
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
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
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
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
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
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderByExpression != null && orderByExpression.OrderBy.Count > 0)
            {
                foreach (var item in orderByExpression.OrderBy)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
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
        #endregion

        #region 函数查询
        public int Count(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(_unitOfWork?.Transaction).Count().To<int>();
            else
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).Count().To<int>();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression == null)
                return (await _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(_unitOfWork?.Transaction).CountAsync(cancellationToken)).To<int>();
            else
                return (await _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).CountAsync(cancellationToken)).To<int>();
        }

        public bool Exist(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(_unitOfWork?.Transaction).Any();
            else
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).Any();
        }

        public Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression == null)
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(_unitOfWork?.Transaction).AnyAsync(cancellationToken);
            else
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).AnyAsync(cancellationToken);
        }
        #endregion

        #region 库表
        public string GetDbTableName()
        {
            var dbName = _fsql.CodeFirst.GetTableByEntity(EntityType ).DbName;
            return AsTableValueInternal?.Invoke(dbName) ?? dbName;
        }

        public List<string> GetDbColumnName()
        {
            var columns = _fsql.CodeFirst.GetTableByEntity(EntityType ).Columns;
            return columns.Keys.ToList();
        }

        public IRepository<TEntity> AsTable(Func<string, string> rule)
        {
            AsTableValueInternal = rule;
            AsTableSelectValueInternal = rule == null ? null : new Func<Type, string, string>((a, b) => a == EntityType ? rule(b) : null);

            return this;
        }

        #endregion
    }
}
