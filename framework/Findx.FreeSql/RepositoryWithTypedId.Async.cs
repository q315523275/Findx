using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Data;
using Findx.Exceptions;
using Findx.Extensions;
using FreeSql.Extensions.EntityUtil;

namespace Findx.FreeSql;

public partial class RepositoryWithTypedId<TEntity, TKey>
{
    #region 插入

    /// <summary>
    ///     插入单条数据
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
            fInsert.AsTable(_ => (entity as ITableSharding).GetShardingTableName());
        else
            fInsert.AsTable(AsTableValueInternal);

        return await fInsert.WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
    }

    /// <summary>
    ///     批量插入数据
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        entities = CheckInsert(entities);
        // ReSharper disable once PossibleMultipleEnumeration
        var result = await _fsql.Insert(entities).AsTable(AsTableValueInternal)
                                .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        // ReSharper disable once PossibleMultipleEnumeration
        return result;
    }

    #endregion

    #region 删除

    /// <summary>
    ///     根据主键删除数据
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<int> DeleteAsync(TKey key, CancellationToken cancellationToken = default)
    {
        if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault())
        {
            return _fsql.Update<TEntity>(key).AsTable(AsTableValueInternal)
                        .Set(it => (it as ISoftDeletable).IsDeleted == true)
                        .Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now)
                        .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }
        
        return _fsql.Delete<TEntity>(key).AsTable(AsTableValueInternal)
                    .WithTransaction(UnitOfWork?.Transaction) .ExecuteAffrowsAsync(cancellationToken);
    }
    
    /// <summary>
    ///     根据指定的条件删除数据
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
    {
        if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault() && whereExpression == null)
        {
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal)
                        .Set(it => (it as ISoftDeletable).IsDeleted == true)
                        .Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now)
                        .Where(it => true)
                        .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }
        
        if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault() && whereExpression != null)
        {
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal)
                        .Set(it => (it as ISoftDeletable).IsDeleted == true)
                        .Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now)
                        .Where(whereExpression)
                        .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }

        if (whereExpression == null)
        {
            return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal)
                        .Where(it => true)
                        .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        }
            
        return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal)
                    .Where(whereExpression)
                    .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
    }

    #endregion

    #region 更新

    /// <summary>
    ///     更新实体信息
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="updateColumns"></param>
    /// <param name="ignoreColumns"></param>
    /// <param name="ignoreNullColumns"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, bool ignoreNullColumns = false, CancellationToken cancellationToken = default)
    {
        entity = CheckUpdate(entity)[0];

        var update = _fsql.Update<TEntity>();

        // 存在分表标签
        // 动态自定义分表为Null
        if (_entityExtensionAttribute.HasTableSharding.GetValueOrDefault() && AsTableValueInternal == null)
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once SuspiciousTypeConversion.Global
            update.AsTable(_ => (entity as ITableSharding).GetShardingTableName());
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

    /// <summary>
    ///     批量更新实体信息
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="updateColumns"></param>
    /// <param name="ignoreColumns"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     根据指定条件更新指定列
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<int> UpdateColumnsAsync(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
    {
        return _fsql.Update<TEntity>().AsTable(AsTableValueInternal)
                    .Set(columns).Where(whereExpression)
                    .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
    }

    /// <summary>
    ///     根据指定条件更新指定列
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<int> UpdateColumnsAsync(List<Expression<Func<TEntity, bool>>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
    {
        Check.NotNull(columns, nameof(columns));
        Check.NotNull(whereExpression, nameof(whereExpression));

        var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal);

        foreach (var item in columns) update.Set(item);

        return update.Where(whereExpression).WithTransaction(UnitOfWork?.Transaction)
                     .ExecuteAffrowsAsync(cancellationToken);
    }

    /// <summary>
    ///     根据主键更新指定列
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<int> UpdateColumnsAsync(Dictionary<string, object> dict, CancellationToken cancellationToken = default)
    {
        FindxException.ThrowIf(dict == null || !dict.ContainsKey("Id"), "505", "字典更新时必须包含主键Id数据");
        var tableName = GetDbTableName();
        return _fsql.UpdateDict(dict).AsTable(tableName).WherePrimary("Id").WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
    }

    /// <summary>
    ///     根据指定条件更新指定列
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<int> UpdateColumnsAsync(Dictionary<string, object> dict, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
    {
        return _fsql.Update<TEntity>().AsTable(AsTableValueInternal)
                    .SetDto(dict).Where(whereExpression)
                    .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
    }
    
    /// <summary>
    ///     更新实体属性值变更字段
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public async Task<int> SaveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var key = entity.Id.SafeString();
        if (key.IsNullOrWhiteSpace() || key == "0")
        {
            throw new Exception("主键值不可为空");
        }

        if (!_attachDict.TryGetValue(key, out var oldValue))
        {
            oldValue = await GetAsync(entity.Id, cancellationToken);
        }

        if (oldValue == null)
        {
            entity.SetEmptyKey();
            return await InsertAsync(entity, cancellationToken);
        }

        //  变更字段集合
        var updateFields = _fsql.CompareEntityValueReturnColumns(_entityType, oldValue, entity, false);

        if (updateFields.Length > 0)
        {
            return await _fsql.Update<TEntity>().AsTable(AsTableValueInternal)
                              .SetSource(entity).UpdateColumns(x => updateFields)
                              .WithTransaction(UnitOfWork?.Transaction)
                              .ExecuteAffrowsAsync(cancellationToken);
        }

        return 1;
    }
    
    #endregion

    #region 查询

    /// <summary>
    ///     根据主键查询实体信息
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TEntity> GetAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var model = await _fsql.Select<TEntity>(key).AsTable(AsTableSelectValueInternal)
                               .WithTransaction(UnitOfWork?.Transaction)
                               .FirstAsync(cancellationToken);
        
        if (model != null) Attach(model);
        
        return model;
    }

    /// <summary>
    ///     根据指定条件查询单条信息
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
    {
        TEntity model;
        
        if (whereExpression == null)
        {
            model = await _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                               .WithTransaction(UnitOfWork?.Transaction)
                               .FirstAsync(cancellationToken);
            
            if (model != null) Attach(model);

            return model;
        }
        
        model = await _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                           .Where(whereExpression)
                           .WithTransaction(UnitOfWork?.Transaction)
                           .FirstAsync(cancellationToken);
        
        if (model != null) Attach(model);

        return model;
    }

    /// <summary>
    ///     根据指定条件及排序查询列表信息
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="sortConditions"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (sortConditions != null)
        {
            foreach (var item in sortConditions)
            {
                if (item.SortDirection == ListSortDirection.Ascending)
                {
                    queryable.OrderBy(item.Conditions);
                }
                else
                {
                    queryable.OrderByDescending(item.Conditions);
                }
            }
        }

        return queryable.WithTransaction(UnitOfWork?.Transaction).ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     根据指定条件查询列表信息
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="selectExpression"></param>
    /// <param name="sortConditions"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public Task<List<TObject>> SelectAsync<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, CancellationToken cancellationToken = default)
    {
        var select = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            select.Where(whereExpression);

        if (sortConditions != null)
        {
            foreach (var item in sortConditions)
            {
                if (item.SortDirection == ListSortDirection.Ascending)
                {
                    select.OrderBy(item.Conditions);
                }
                else
                {
                    select.OrderByDescending(item.Conditions);
                }
            }
        }

        if (selectExpression == null)
            return select.WithTransaction(UnitOfWork?.Transaction).ToListAsync<TObject>(cancellationToken);
        
        return select.WithTransaction(UnitOfWork?.Transaction).ToListAsync(selectExpression, cancellationToken);
    }

    /// <summary>
    ///     根据指定条件查询固定条数列表信息
    /// </summary>
    /// <param name="topSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="sortConditions"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<List<TEntity>> TopAsync(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (sortConditions != null)
        {
            foreach (var item in sortConditions)
            {
                if (item.SortDirection == ListSortDirection.Ascending)
                {
                    queryable.OrderBy(item.Conditions);
                }
                else
                {
                    queryable.OrderByDescending(item.Conditions);
                }
            }
        }

        return queryable.WithTransaction(UnitOfWork?.Transaction).Take(topSize).ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     根据指定条件查询固定条数列表信息
    /// </summary>
    /// <param name="topSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="selectExpression"></param>
    /// <param name="sortConditions"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public Task<List<TObject>> TopAsync<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (sortConditions != null)
        {
            foreach (var item in sortConditions)
            {
                if (item.SortDirection == ListSortDirection.Ascending)
                {
                    queryable.OrderBy(item.Conditions);
                }
                else
                {
                    queryable.OrderByDescending(item.Conditions);
                }
            }
        }

        queryable.Take(topSize);

        if (selectExpression == null)
            return queryable.WithTransaction(UnitOfWork?.Transaction).ToListAsync<TObject>(cancellationToken);
        
        return queryable.WithTransaction(UnitOfWork?.Transaction).ToListAsync(selectExpression, cancellationToken);
    }

    #endregion

    #region 分页
    
    /// <summary>
    ///     根据指定的条件进行分页查询
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="sortConditions"></param>
    /// <param name="hasTotalRows"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<PageResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, bool hasTotalRows = true, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (sortConditions != null)
        {
            foreach (var item in sortConditions)
            {
                if (item.SortDirection == ListSortDirection.Ascending)
                {
                    queryable.OrderBy(item.Conditions);
                }
                else
                {
                    queryable.OrderByDescending(item.Conditions);
                }
            }
        }

        var result = await queryable.WithTransaction(UnitOfWork?.Transaction).CountIf(hasTotalRows, out var totalRows)
                                    .Page(pageNumber, pageSize)
                                    .ToListAsync(cancellationToken);

        return new PageResult<List<TEntity>>(pageNumber, pageSize, totalRows, result);
    }
    
    /// <summary>
    ///     根据指定的条件进行分页查询
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="selectExpression"></param>
    /// <param name="sortConditions"></param>
    /// <param name="hasTotalRows"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public async Task<PageResult<List<TObject>>> PagedAsync<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, bool hasTotalRows = true, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (sortConditions != null)
        {
            foreach (var item in sortConditions)
            {
                if (item.SortDirection == ListSortDirection.Ascending)
                {
                    queryable.OrderBy(item.Conditions);
                }
                else
                {
                    queryable.OrderByDescending(item.Conditions);
                }
            }
        }

        queryable.CountIf(hasTotalRows, out var totalRows).Page(pageNumber, pageSize);

        List<TObject> result;

        if (selectExpression == null)
            result = await queryable.WithTransaction(UnitOfWork?.Transaction)
                                    .ToListAsync<TObject>(cancellationToken);
        else
            result = await queryable.WithTransaction(UnitOfWork?.Transaction)
                                    .ToListAsync(selectExpression, cancellationToken);

        return new PageResult<List<TObject>>(pageNumber, pageSize, totalRows, result);
    }

    #endregion

    #region 函数查询

    /// <summary>
    ///     查询指定的条件的数量
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long> CountAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
    {
        if (whereExpression == null)
            return (await _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                .WithTransaction(UnitOfWork?.Transaction).CountAsync(cancellationToken)).To<int>();
        return (await _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).CountAsync(cancellationToken)).To<int>();
    }

    /// <summary>
    ///     判断指定的条件是否存在
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
    {
        if (whereExpression == null)
            return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                        .WithTransaction(UnitOfWork?.Transaction).AnyAsync(cancellationToken);
        
        return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                    .Where(whereExpression)
                    .WithTransaction(UnitOfWork?.Transaction).AnyAsync(cancellationToken);
    }
    
    #endregion
}