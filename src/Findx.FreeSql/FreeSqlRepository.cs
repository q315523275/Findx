using Findx.Data;
using Findx.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Principal;
using Findx.Security;

namespace Findx.FreeSql
{
    public class FreeSqlRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity, new()
    {
        private readonly IPrincipal _principal;
        private readonly IFreeSql _fsql;
        private readonly IOptionsMonitor<FreeSqlOptions> _options;
        private readonly EntityExtensionAttribute _entityExtensionAttribute;

        private readonly Type _entityType = typeof(TEntity);
        private Func<string, string> AsTableValueInternal { get; set; }
        private Func<Type, string, string> AsTableSelectValueInternal { get; set; }

        /// <summary>
        /// 获取Orm配置
        /// </summary>
        private FreeSqlOptions Options => _options?.CurrentValue;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public FreeSqlRepository(IServiceProvider serviceProvider)
        {
            // 服务
            var clients = serviceProvider.GetRequiredService<FreeSqlClient>();
            _options = serviceProvider.GetRequiredService<IOptionsMonitor<FreeSqlOptions>>();
            _principal = serviceProvider.GetService<IPrincipal>();

            // 实体扩展属性
            _entityExtensionAttribute = _entityType.GetEntityExtensionAttribute();
            
            // 设置缺省值
            if (_entityExtensionAttribute.DataSource.IsNullOrWhiteSpace())
                _entityExtensionAttribute.DataSource = _options.CurrentValue.Primary;
            
            // Orm实例
            clients.TryGetValue(_entityExtensionAttribute.DataSource, out _fsql);
            
            // Orm实例检查
            if (Options.Strict)
                Check.NotNull(_fsql, nameof(_fsql));
            
            // 默认数据库连接标识
            if (_fsql == null)
            {
                clients.TryGetValue(Options.Primary, out _fsql);
                Check.NotNull(_fsql, nameof(_fsql));
            }
        }

        /// <summary>
        /// 获取或设置：工作单元
        /// </summary>
        public IUnitOfWork UnitOfWork { set; get; }

        #region 插入
        public int Insert(TEntity entity)
        {
            entity.ThrowIfNull();
            entity = CheckInsert(entity)[0];
            var fInsert = _fsql.Insert(entity);
            // 存在分表标签
            // 动态自定义分表为Null
            if (_entityExtensionAttribute.HasTableSharding.GetValueOrDefault() && AsTableValueInternal == null)
                // ReSharper disable once PossibleNullReferenceException
                // ReSharper disable once SuspiciousTypeConversion.Global
                fInsert.AsTable(x => (entity as ITableSharding).GetShardingTableName());
            else
                fInsert.AsTable(AsTableValueInternal);
            
            return fInsert.WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
        }

        public int Insert(IEnumerable<TEntity> entities)
        {
            entities = CheckInsert(entities);
            // ReSharper disable once PossibleMultipleEnumeration
            var result = _fsql.Insert(entities).AsTable(AsTableValueInternal).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
            // ReSharper disable once PossibleMultipleEnumeration
            return result;
        }

        public async Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            entity.ThrowIfNull();
            entity = CheckInsert(entity)[0];
            var fInsert = _fsql.Insert(entity);
            // 存在分表标签
            // 动态自定义分表为Null
            if (_entityExtensionAttribute.HasTableSharding.GetValueOrDefault() && AsTableValueInternal == null)
                // ReSharper disable once PossibleNullReferenceException
                // ReSharper disable once SuspiciousTypeConversion.Global
                fInsert.AsTable((x) => (entity as ITableSharding).GetShardingTableName());
            else
                fInsert.AsTable(AsTableValueInternal);
            
            return await fInsert.WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }

        public async Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            entities = CheckInsert(entities);
            // ReSharper disable once PossibleMultipleEnumeration
            var result = await _fsql.Insert(entities).AsTable(AsTableValueInternal).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            // ReSharper disable once PossibleMultipleEnumeration
            return result;
        }
        #endregion

        #region 删除
        public int Delete(object key)
        {
            if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault())
            {
                return _fsql.Update<TEntity>(key).AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).IsDeleted == true).Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
            }

            return _fsql.Delete<TEntity>(key).AsTable(AsTableValueInternal).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> DeleteAsync(object key, CancellationToken cancellationToken = default)
        {
            if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault())
            {
                return _fsql.Update<TEntity>(key).AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).IsDeleted == true).Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            }

            return _fsql.Delete<TEntity>(key).AsTable(AsTableValueInternal).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }

        public int Delete(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault() && whereExpression == null)
            {
                return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).IsDeleted == true).Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).Where(it => true).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
            }

            if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault() && whereExpression != null)
            {
                return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).IsDeleted == true).Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
            }

            if (whereExpression == null)
                return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(it => true).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
            else
                return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault() && whereExpression == null)
            {
                return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).IsDeleted == true).Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).Where(it => true).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            }

            if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault() && whereExpression != null)
            {
                return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).IsDeleted == true).Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            }

            if (whereExpression == null)
                return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(it => true).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            else
                return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }
        #endregion

        #region 更新
        public int Update(TEntity entity, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, bool ignoreNullColumns = false)
        {
            entity.ThrowIfNull();
            entity = CheckUpdate(entity)[0];

            var update = _fsql.Update<TEntity>();
            
            // 存在分表标签
            // 动态自定义分表为Null
            if (_entityExtensionAttribute.HasTableSharding.GetValueOrDefault() && AsTableValueInternal == null)
                // ReSharper disable once PossibleNullReferenceException
                // ReSharper disable once SuspiciousTypeConversion.Global
                update.AsTable((x) => (entity as ITableSharding).GetShardingTableName());
            else
                update.AsTable(AsTableValueInternal);

            if (ignoreNullColumns)
                update.SetSourceIgnore(entity, col => col == null);
            else
                update.SetSource(entity);

            if (updateColumns != null)
                update.UpdateColumns(updateColumns);

            if (ignoreColumns != null)
                update.IgnoreColumns(ignoreColumns);
            
            return update.WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
        }

        public async Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, bool ignoreNullColumns = false, CancellationToken cancellationToken = default)
        {
            entity = CheckUpdate(entity)[0];

            var update = _fsql.Update<TEntity>();
            
            // 存在分表标签
            // 动态自定义分表为Null
            if (_entityExtensionAttribute.HasTableSharding.GetValueOrDefault() && AsTableValueInternal == null)
                // ReSharper disable once PossibleNullReferenceException
                // ReSharper disable once SuspiciousTypeConversion.Global
                update.AsTable(x => (entity as ITableSharding).GetShardingTableName());
            else
                update.AsTable(AsTableValueInternal);

            if (ignoreNullColumns)
                update.SetSourceIgnore(entity, col => col == null);
            else
                update.SetSource(entity);

            if (updateColumns != null)
                update.UpdateColumns(updateColumns);

            if (ignoreColumns != null)
                update.IgnoreColumns(ignoreColumns);
            
            return await update.WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }

        public int Update(IEnumerable<TEntity> entities, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null)
        {
            entities = CheckUpdate(entities);

            // ReSharper disable once PossibleMultipleEnumeration
            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal).SetSource(entities);

            if (updateColumns != null)
                update.UpdateColumns(updateColumns);

            if (ignoreColumns != null)
                update.IgnoreColumns(ignoreColumns);

            var result = update.WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
            
            // ReSharper disable once PossibleMultipleEnumeration
            return result;
        }

        public async Task<int> UpdateAsync(IEnumerable<TEntity> entities, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, CancellationToken cancellationToken = default)
        {
            entities = CheckUpdate(entities);

            // ReSharper disable once PossibleMultipleEnumeration
            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal).SetSource(entities);

            if (updateColumns != null)
                update.UpdateColumns(updateColumns);

            if (ignoreColumns != null)
                update.IgnoreColumns(ignoreColumns);

            var result = await update.WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);

            // ReSharper disable once PossibleMultipleEnumeration
            return result;
        }

        public int UpdateColumns(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression)
        {
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(columns).Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> UpdateColumnsAsync(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(columns).Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
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

            return update.Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
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

            return update.Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }

        #endregion

        #region 查询
        public TEntity Get(object key)
        {
            return _fsql.Select<TEntity>(key).AsTable(AsTableSelectValueInternal).WithTransaction(UnitOfWork?.Transaction).First();
        }

        public Task<TEntity> GetAsync(object key, CancellationToken cancellationToken = default)
        {
            return _fsql.Select<TEntity>(key).AsTable(AsTableSelectValueInternal).WithTransaction(UnitOfWork?.Transaction).FirstAsync(cancellationToken);
        }

        public TEntity First(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(UnitOfWork?.Transaction).First();
            else
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).First();
        }

        public Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression == null)
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(UnitOfWork?.Transaction).FirstAsync(cancellationToken);
            else
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).FirstAsync(cancellationToken);
        }


        public List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null)
        {
            var queryable = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderParameters != null)
            {
                foreach (var item in orderParameters)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            return queryable.WithTransaction(UnitOfWork?.Transaction).ToList();
        }

        public Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default)
        {
            var queryable = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderParameters != null)
            {
                foreach (var item in orderParameters)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            return queryable.WithTransaction(UnitOfWork?.Transaction).ToListAsync(cancellationToken);
        }

        public List<TObject> Select<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null)
        {
            var select = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                select.Where(whereExpression);

            if (orderParameters != null)
            {
                foreach (var item in orderParameters)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        select.OrderBy(item.Expression);
                    else
                        select.OrderByDescending(item.Expression);
                }
            }

            if (selectExpression == null)
                return select.WithTransaction(UnitOfWork?.Transaction).ToList<TObject>();
            else
                return select.WithTransaction(UnitOfWork?.Transaction).ToList(selectExpression);
        }

        public Task<List<TObject>> SelectAsync<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default)
        {
            var select = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                select.Where(whereExpression);

            if (orderParameters != null)
            {
                foreach (var item in orderParameters)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        select.OrderBy(item.Expression);
                    else
                        select.OrderByDescending(item.Expression);
                }
            }

            if (selectExpression == null)
                return select.WithTransaction(UnitOfWork?.Transaction).ToListAsync<TObject>(cancellationToken);
            else
                return select.WithTransaction(UnitOfWork?.Transaction).ToListAsync(selectExpression, cancellationToken);
        }


        public List<TEntity> Top(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null)
        {
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderParameters != null)
            {
                foreach (var item in orderParameters)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            return queryable.WithTransaction(UnitOfWork?.Transaction).Take(topSize).ToList();
        }

        public Task<List<TEntity>> TopAsync(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default)
        {
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderParameters != null)
            {
                foreach (var item in orderParameters)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            return queryable.WithTransaction(UnitOfWork?.Transaction).Take(topSize).ToListAsync(cancellationToken);
        }

        public List<TObject> Top<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null)
        {
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderParameters != null)
            {
                foreach (var item in orderParameters)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            queryable.Take(topSize);

            if (selectExpression == null)
                return queryable.WithTransaction(UnitOfWork?.Transaction).ToList<TObject>();
            else
                return queryable.WithTransaction(UnitOfWork?.Transaction).ToList(selectExpression);
        }

        public Task<List<TObject>> TopAsync<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default)
        {
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderParameters != null)
            {
                foreach (var item in orderParameters)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            queryable.Take(topSize);

            if (selectExpression == null)
                return queryable.WithTransaction(UnitOfWork?.Transaction).ToListAsync<TObject>(cancellationToken);
            else
                return queryable.WithTransaction(UnitOfWork?.Transaction).ToListAsync(selectExpression, cancellationToken);
        }


        public PageResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null)
        {
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderParameters != null)
            {
                foreach (var item in orderParameters)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            var result = queryable.WithTransaction(UnitOfWork?.Transaction).Count(out var totalRows).Page(pageNumber, pageSize).ToList();

            return new PageResult<List<TEntity>>(pageNumber, pageSize, (int)totalRows, result);
        }

        public async Task<PageResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default)
        {
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderParameters != null)
            {
                foreach (var item in orderParameters)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            var result = await queryable.WithTransaction(UnitOfWork?.Transaction).Count(out var totalRows).Page(pageNumber, pageSize).ToListAsync(cancellationToken);

            return new PageResult<List<TEntity>>(pageNumber, pageSize, (int)totalRows, result);
        }

        public PageResult<List<TObject>> Paged<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null)
        {
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderParameters != null)
            {
                foreach (var item in orderParameters)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            queryable.Count(out var totalRows).Page(pageNumber, pageSize);

            var result = selectExpression == null ? 
                queryable.WithTransaction(UnitOfWork?.Transaction).ToList<TObject>() 
                : queryable.WithTransaction(UnitOfWork?.Transaction).ToList(selectExpression);

            return new PageResult<List<TObject>>(pageNumber, pageSize, (int)totalRows, result);
        }

        public async Task<PageResult<List<TObject>>> PagedAsync<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default)
        {
            var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

            if (whereExpression != null)
                queryable.Where(whereExpression);

            if (orderParameters != null)
            {
                foreach (var item in orderParameters)
                {
                    if (item.SortDirection == ListSortDirection.Ascending)
                        queryable.OrderBy(item.Expression);
                    else
                        queryable.OrderByDescending(item.Expression);
                }
            }

            queryable.Count(out var totalRows).Page(pageNumber, pageSize);

            List<TObject> result;

            if (selectExpression == null)
                result = await queryable.WithTransaction(UnitOfWork?.Transaction).ToListAsync<TObject>(cancellationToken);
            else
                result = await queryable.WithTransaction(UnitOfWork?.Transaction).ToListAsync(selectExpression, cancellationToken);

            return new PageResult<List<TObject>>(pageNumber, pageSize, (int)totalRows, result);
        }
        #endregion

        #region 函数查询
        public int Count(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(UnitOfWork?.Transaction).Count().To<int>();
            else
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).Count().To<int>();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression == null)
                return (await _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(UnitOfWork?.Transaction).CountAsync(cancellationToken)).To<int>();
            else
                return (await _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).CountAsync(cancellationToken)).To<int>();
        }

        public bool Exist(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (whereExpression == null)
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(UnitOfWork?.Transaction).Any();
            else
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).Any();
        }

        public Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
        {
            if (whereExpression == null)
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).WithTransaction(UnitOfWork?.Transaction).AnyAsync(cancellationToken);
            else
                return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).AnyAsync(cancellationToken);
        }
        #endregion

        #region 库表
        public string GetDbTableName()
        {
            var dbName = _fsql.CodeFirst.GetTableByEntity(_entityType).DbName;
            return AsTableValueInternal?.Invoke(dbName) ?? dbName;
        }

        public List<string> GetDbColumnName()
        {
            var columns = _fsql.CodeFirst.GetTableByEntity(_entityType).Columns;
            return columns.Keys.ToList();
        }

        public IRepository<TEntity> AsTable(Func<string, string> rule)
        {
            AsTableValueInternal = rule;
            AsTableSelectValueInternal = rule == null ? null : new Func<Type, string, string>((a, b) => a == _entityType ? rule(b) : null);

            return this;
        }

        public DatabaseType GetDbType()
        {
            var dataType = _fsql.Ado.DataType.ToString();
            return dataType.CastTo<DatabaseType>();
        }
        #endregion

        #region 私有方法
        
        /// <summary>
        /// 检查插入信息
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private TEntity[] CheckInsert(params TEntity[] entities)
        {
            if (!Options.CheckInsert)
            {
                return entities;
            }

            var userIdTypeName = _principal?.Identity.GetClaimValueFirstOrDefault(ClaimTypes.UserIdTypeName);
            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                entities[i] = entity.CheckCreatedTime();

                if (userIdTypeName == null)
                {
                    continue;
                }
                entity = entities[i];
                if (userIdTypeName == typeof(int).FullName)
                {
                    entities[i] = entity.CheckCreationAudited<TEntity, int>(_principal);
                }
                else if (userIdTypeName == typeof(Guid).FullName)
                {
                    entities[i] = entity.CheckCreationAudited<TEntity, Guid>(_principal);
                }
                else
                {
                    entities[i] = entity.CheckCreationAudited<TEntity, long>(_principal);
                }
            }

            return entities;
        }

        /// <summary>
        /// 检查更新信息
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private TEntity[] CheckUpdate(params TEntity[] entities)
        {
            if (!Options.CheckUpdate)
            {
                return entities;
            }

            var userIdTypeName = _principal?.Identity.GetClaimValueFirstOrDefault(ClaimTypes.UserIdTypeName);
            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                entities[i] = entity.CheckUpdateTime();

                if (userIdTypeName == null)
                {
                    continue;
                }
                if (userIdTypeName == typeof(int).FullName)
                {
                    entities[i] = entity.CheckUpdateAudited<TEntity, int>(_principal);
                }
                else if (userIdTypeName == typeof(Guid).FullName)
                {
                    entities[i] = entity.CheckUpdateAudited<TEntity, Guid>(_principal);
                }
                else
                {
                    entities[i] = entity.CheckUpdateAudited<TEntity, long>(_principal);
                }
            }

            return entities;
        }

        /// <summary>
        /// 检查插入信息
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private IEnumerable<TEntity> CheckInsert(IEnumerable<TEntity> entities)
        {
            if (!Options.CheckInsert)
            {
                return entities;
            }

            var userIdTypeName = _principal?.Identity.GetClaimValueFirstOrDefault(ClaimTypes.UserIdTypeName);
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var entity in entities)
            {
                entity.CheckCreatedTime();

                if (userIdTypeName == null)
                {
                    continue;
                }
                if (userIdTypeName == typeof(int).FullName)
                {
                    entity.CheckCreationAudited<TEntity, int>(_principal);
                }
                else if (userIdTypeName == typeof(Guid).FullName)
                {
                    entity.CheckCreationAudited<TEntity, Guid>(_principal);
                }
                else
                {
                    entity.CheckCreationAudited<TEntity, long>(_principal);
                }
            }

            // ReSharper disable once PossibleMultipleEnumeration
            return entities;
        }

        /// <summary>
        /// 检查更新信息
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private IEnumerable<TEntity> CheckUpdate(IEnumerable<TEntity> entities)
        {
            if (!Options.CheckUpdate)
            {
                return entities;
            }

            var userIdTypeName = _principal?.Identity.GetClaimValueFirstOrDefault(ClaimTypes.UserIdTypeName);
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var entity in entities)
            {
                entity.CheckUpdateTime();

                if (userIdTypeName == null)
                {
                    continue;
                }
                if (userIdTypeName == typeof(int).FullName)
                {
                    entity.CheckUpdateAudited<TEntity, int>(_principal);
                }
                else if (userIdTypeName == typeof(Guid).FullName)
                {
                    entity.CheckUpdateAudited<TEntity, Guid>(_principal);
                }
                else
                {
                    entity.CheckUpdateAudited<TEntity, long>(_principal);
                }
            }

            // ReSharper disable once PossibleMultipleEnumeration
            return entities;
        }

        #endregion
    }
}
