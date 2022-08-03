﻿using Findx.Data;
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
        private static IDictionary<Type, DataEntityAttribute> DataEntityMap = new ConcurrentDictionary<Type, DataEntityAttribute>();
        private static IDictionary<Type, (bool softDeletable, bool customSharding)> BaseOnMap = new ConcurrentDictionary<Type, (bool softDeletable, bool customSharding)>();

        private readonly IFreeSql _fsql;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptionsMonitor<FreeSqlOptions> _options;
        private readonly bool _softDeletable;

        private readonly Type _entityType = typeof(TEntity);
        private Func<string, string> AsTableValueInternal { get; set; }
        private Func<Type, string, string> AsTableSelectValueInternal { get; set; }

        public FreeSqlRepository(FreeSqlClient clients, IUnitOfWorkManager uowManager, IOptionsMonitor<FreeSqlOptions> options)
        {
            var js = DateTime.Now;

            Check.NotNull(options.CurrentValue, "FreeSqlOptions");

            _options = options;

            var attribute = DataEntityMap.GetOrAdd(_entityType , () => _entityType .GetAttribute<DataEntityAttribute>());

            var primary = attribute?.DataSource ?? Options.Primary ?? "";

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
            _unitOfWork = uowManager.GetConnUnitOfWork(true, (primary == Options.Primary ? null : primary));

            // 基类标记
            var baseOns = BaseOnMap.GetOrAdd(_entityType , () =>
            {
                // 是否标记实体逻辑删除
                var softDeletable = _entityType.IsBaseOn(typeof(ISoftDeletable));
                // 是否标记自定义分表函数
                var customSharding = _entityType.IsBaseOn(typeof(ITableSharding));
                return (softDeletable, customSharding);
            });
            _softDeletable = baseOns.softDeletable;
            // _customSharding = baseOns.customSharding;

            // 初始化分表计算
            if (attribute?.TableShardingType == ShardingType.Time)
            {
                AsTableValueInternal = (oldName) => $"{oldName}_{DateTime.Now.ToString(attribute.TableShardingExt)}";
                AsTableSelectValueInternal = (type, oldName) => $"{oldName}_{DateTime.Now.ToString(attribute.TableShardingExt)}";
            }

            Debug.WriteLine($"仓储构造函数耗时:{(DateTime.Now - js).TotalMilliseconds:0.000}毫秒");
        }

        private FreeSqlOptions Options => _options?.CurrentValue;

        public IUnitOfWork GetUnitOfWork()
        {
            return _unitOfWork;
        }

        #region 插入
        public int Insert(TEntity entity)
        {
            return _fsql.Insert(entity).AsTable(AsTableValueInternal).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public int Insert(IEnumerable<TEntity> entities)
        {
            return _fsql.Insert(entities).AsTable(AsTableValueInternal).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return _fsql.Insert(entity).AsTable(AsTableValueInternal).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }

        public Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return _fsql.Insert(entities).AsTable(AsTableValueInternal).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }
        #endregion

        #region 删除
        public int Delete(object key)
        {
            if (_softDeletable)
            {
                return _fsql.Update<TEntity>(key).AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).IsDeleted == true).Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
            }

            return _fsql.Delete<TEntity>(key).AsTable(AsTableValueInternal).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> DeleteAsync(object key, CancellationToken cancellationToken = default)
        {
            if (_softDeletable)
            {
                return _fsql.Update<TEntity>(key).AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).IsDeleted == true).Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            }

            return _fsql.Delete<TEntity>(key).AsTable(AsTableValueInternal).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }

        public int Delete(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            if (_softDeletable && whereExpression == null)
            {
                return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).IsDeleted == true).Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).Where(it => true).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
            }

            if (_softDeletable && whereExpression != null)
            {
                return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).IsDeleted == true).Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
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
                return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).IsDeleted == true).Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).Where(it => true).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            }

            if (_softDeletable && whereExpression != null)
            {
                return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(it => (it as ISoftDeletable).IsDeleted == true).Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            }

            if (whereExpression == null)
                return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(it => true).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
            else
                return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }
        #endregion

        #region 更新
        public int Update(TEntity entity, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, bool ignoreNullColumns = false)
        {
            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal);

            if (ignoreNullColumns)
                update.SetSourceIgnore(entity, col => col == null);
            else
                update.SetSource(entity);

            if (updateColumns != null)
                update.UpdateColumns(updateColumns);

            if (ignoreColumns != null)
                update.IgnoreColumns(ignoreColumns);

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, bool ignoreNullColumns = false, CancellationToken cancellationToken = default)
        {
            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal);

            if (ignoreNullColumns)
                update.SetSourceIgnore(entity, col => col == null);
            else
                update.SetSource(entity);

            if (updateColumns != null)
                update.UpdateColumns(updateColumns);

            if (ignoreColumns != null)
                update.IgnoreColumns(ignoreColumns);

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }

        public int Update(IEnumerable<TEntity> entityList, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null)
        {
            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal).SetSource(entityList);

            if (updateColumns != null)
                update.UpdateColumns(updateColumns);

            if (ignoreColumns != null)
                update.IgnoreColumns(ignoreColumns);

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> UpdateAsync(IEnumerable<TEntity> entityList, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, CancellationToken cancellationToken = default)
        {
            var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal).SetSource(entityList);

            if (updateColumns != null)
                update.UpdateColumns(updateColumns);

            if (ignoreColumns != null)
                update.IgnoreColumns(ignoreColumns);

            return update.WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }

        public int UpdateColumns(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression)
        {
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(columns).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrows();
        }

        public Task<int> UpdateColumnsAsync(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(columns).Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
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

            return update.Where(whereExpression).WithTransaction(_unitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
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


        public List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression = null, params OrderByParameter<TEntity>[] orderParameters)
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

            return queryable.WithTransaction(_unitOfWork?.Transaction).ToList();
        }

        public Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default, params OrderByParameter<TEntity>[] orderParameters)
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

            return queryable.WithTransaction(_unitOfWork?.Transaction).ToListAsync(cancellationToken);
        }

        public List<TObject> Select<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, params OrderByParameter<TEntity>[] orderParameters)
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
                return select.WithTransaction(_unitOfWork?.Transaction).ToList<TObject>();
            else
                return select.WithTransaction(_unitOfWork?.Transaction).ToList(selectExpression);
        }

        public Task<List<TObject>> SelectAsync<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, CancellationToken cancellationToken = default, params OrderByParameter<TEntity>[] orderParameters)
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
                return select.WithTransaction(_unitOfWork?.Transaction).ToListAsync<TObject>(cancellationToken);
            else
                return select.WithTransaction(_unitOfWork?.Transaction).ToListAsync(selectExpression, cancellationToken);
        }


        public List<TEntity> Top(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, params OrderByParameter<TEntity>[] orderParameters)
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

            return queryable.WithTransaction(_unitOfWork?.Transaction).Take(topSize).ToList();
        }

        public Task<List<TEntity>> TopAsync(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default, params OrderByParameter<TEntity>[] orderParameters)
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

            return queryable.WithTransaction(_unitOfWork?.Transaction).Take(topSize).ToListAsync(cancellationToken);
        }

        public List<TObject> Top<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, params OrderByParameter<TEntity>[] orderParameters)
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
                return queryable.WithTransaction(_unitOfWork?.Transaction).ToList<TObject>();
            else
                return queryable.WithTransaction(_unitOfWork?.Transaction).ToList(selectExpression);
        }

        public Task<List<TObject>> TopAsync<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, CancellationToken cancellationToken = default, params OrderByParameter<TEntity>[] orderParameters)
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
                return queryable.WithTransaction(_unitOfWork?.Transaction).ToListAsync<TObject>(cancellationToken);
            else
                return queryable.WithTransaction(_unitOfWork?.Transaction).ToListAsync(selectExpression, cancellationToken);
        }


        public PageResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, params OrderByParameter<TEntity>[] orderParameters)
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

            var result = queryable.WithTransaction(_unitOfWork?.Transaction).Count(out var totalRows).Page(pageNumber, pageSize).ToList();

            return new PageResult<List<TEntity>>(pageNumber, pageSize, (int)totalRows, result);
        }

        public async Task<PageResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default, params OrderByParameter<TEntity>[] orderParameters)
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

            var result = await queryable.WithTransaction(_unitOfWork?.Transaction).Count(out var totalRows).Page(pageNumber, pageSize).ToListAsync(cancellationToken);

            return new PageResult<List<TEntity>>(pageNumber, pageSize, (int)totalRows, result);
        }

        public PageResult<List<TObject>> Paged<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, params OrderByParameter<TEntity>[] orderParameters)
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
                queryable.WithTransaction(_unitOfWork?.Transaction).ToList<TObject>() 
                : queryable.WithTransaction(_unitOfWork?.Transaction).ToList(selectExpression);

            return new PageResult<List<TObject>>(pageNumber, pageSize, (int)totalRows, result);
        }

        public async Task<PageResult<List<TObject>>> PagedAsync<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, CancellationToken cancellationToken = default, params OrderByParameter<TEntity>[] orderParameters)
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
                result = await queryable.WithTransaction(_unitOfWork?.Transaction).ToListAsync<TObject>(cancellationToken);
            else
                result = await queryable.WithTransaction(_unitOfWork?.Transaction).ToListAsync(selectExpression, cancellationToken);

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
            var dbName = _fsql.CodeFirst.GetTableByEntity(_entityType ).DbName;
            return AsTableValueInternal?.Invoke(dbName) ?? dbName;
        }

        public List<string> GetDbColumnName()
        {
            var columns = _fsql.CodeFirst.GetTableByEntity(_entityType ).Columns;
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
    }
}
