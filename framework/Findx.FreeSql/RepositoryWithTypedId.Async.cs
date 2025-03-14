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

namespace Findx.FreeSql;

public partial class RepositoryWithTypedId<TEntity, TKey>
{
    #region 插入

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

    public Task<int> DeleteAsync(TKey key, CancellationToken cancellationToken = default)
    {
        if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault())
            return _fsql.Update<TEntity>(key).AsTable(AsTableValueInternal)
                .Set(it => (it as ISoftDeletable).IsDeleted == true)
                .Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now)
                .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);

        return _fsql.Delete<TEntity>(key).AsTable(AsTableValueInternal).WithTransaction(UnitOfWork?.Transaction)
            .ExecuteAffrowsAsync(cancellationToken);
    }
    
    public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
    {
        if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault() && whereExpression == null)
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal)
                .Set(it => (it as ISoftDeletable).IsDeleted == true)
                .Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).Where(it => true)
                .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);

        if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault() && whereExpression != null)
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal)
                .Set(it => (it as ISoftDeletable).IsDeleted == true)
                .Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).Where(whereExpression)
                .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);

        if (whereExpression == null)
            return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(it => true)
                .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
        return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
    }

    #endregion

    #region 更新

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

    public Task<int> UpdateColumnsAsync(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
    {
        return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(columns).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
    }

    public Task<int> UpdateColumnsAsync(List<Expression<Func<TEntity, bool>>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
    {
        Check.NotNull(columns, nameof(columns));
        Check.NotNull(whereExpression, nameof(whereExpression));

        var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal);

        foreach (var item in columns) update.Set(item);

        return update.Where(whereExpression).WithTransaction(UnitOfWork?.Transaction)
            .ExecuteAffrowsAsync(cancellationToken);
    }

    public Task<int> UpdateColumnsAsync(Dictionary<string, object> dict, CancellationToken cancellationToken = default)
    {
        FindxException.ThrowIf(dict == null || !dict.ContainsKey("Id"), "505", "字典更新时必须包含主键Id数据");
        var tableName = GetDbTableName();
        return _fsql.UpdateDict(dict).AsTable(tableName).WherePrimary("id").WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
    }

    public Task<int> UpdateColumnsAsync(Dictionary<string, object> dict, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
    {
        return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).SetDto(dict).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
    }
    
    public async Task<int> SaveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var table = _fsql.CodeFirst.GetTableByEntity(_entityType);
        if (!table.Primarys.Any()) 
            throw new Exception($"实体{table.CsName}必须存在主键配置");
        
        var key = entity.Id.SafeString();
        if (key.IsNullOrWhiteSpace())
            throw new Exception($"实体{table.CsName}的主键值不可为空");
        
        if (!_attachDict.TryGetValue(key, out var oldValue)) oldValue = await GetAsync(entity.Id, cancellationToken);

        if (oldValue == null)
        {
            entity.SetEmptyKey();
            return await InsertAsync(entity, cancellationToken);
        }

        var dic = _fsql.CompareChangeValues(entity, oldValue);
        var columns = _fsql.CodeFirst.GetTableByEntity(_entityType).Columns;
        foreach (var keyValue in dic)
        {
            if (columns.TryGetValue(keyValue.Key, out var columnInfo) && columnInfo.Attribute != null)
            {
                if (columnInfo.Attribute.MapType == columnInfo.CsType) continue;
                var csTypeIsPrimitive = columnInfo.CsType.IsPrimitiveExtendedIncludingNullable(true);
                var mapType = columnInfo.Attribute.MapType;
                var mapTypeIsPrimitive = mapType?.IsPrimitiveExtendedIncludingNullable()?? false;
                if (mapType != null && csTypeIsPrimitive && mapTypeIsPrimitive)
                {
                    dic[keyValue.Key] = keyValue.Value.CastTo(mapType);
                }
                else if (mapType != null && !csTypeIsPrimitive && mapTypeIsPrimitive)
                {
                    var value = keyValue.Value?.ToJson();
                    dic[keyValue.Key] = value.CastTo(mapType);
                }
                else
                {
                    throw new NotSupportedException($"变更保存暂不支持“{mapType?.Name}”转换");
                }
            }
        }
        dic.Add("Id", entity.Id);
        
        // 替换新的追踪实体
        _attachDict[key] = entity;
        
        return dic.Count > 1 ? await UpdateColumnsAsync(dic, cancellationToken) : 1;
    }
    
    #endregion

    #region 查询

    public Task<TEntity> GetAsync(TKey key, CancellationToken cancellationToken = default)
    {
        return _fsql.Select<TEntity>(key).AsTable(AsTableSelectValueInternal)
                    .WithTransaction(UnitOfWork?.Transaction)
                    .FirstAsync(cancellationToken);
    }

    public Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
    {
        if (whereExpression == null)
            return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                        .WithTransaction(UnitOfWork?.Transaction)
                        .FirstAsync(cancellationToken);
        
        return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression)
                    .WithTransaction(UnitOfWork?.Transaction)
                    .FirstAsync(cancellationToken);
    }

    public Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (sortConditions != null)
            foreach (var item in sortConditions)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        return queryable.WithTransaction(UnitOfWork?.Transaction).ToListAsync(cancellationToken);
    }

    public Task<List<TObject>> SelectAsync<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, CancellationToken cancellationToken = default)
    {
        var select = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            select.Where(whereExpression);

        if (sortConditions != null)
            foreach (var item in sortConditions)
                if (item.SortDirection == ListSortDirection.Ascending)
                    select.OrderBy(item.Conditions);
                else
                    select.OrderByDescending(item.Conditions);

        if (selectExpression == null)
            return select.WithTransaction(UnitOfWork?.Transaction).ToListAsync<TObject>(cancellationToken);
        
        return select.WithTransaction(UnitOfWork?.Transaction).ToListAsync(selectExpression, cancellationToken);
    }

    public Task<List<TEntity>> TopAsync(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (sortConditions != null)
            foreach (var item in sortConditions)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        return queryable.WithTransaction(UnitOfWork?.Transaction).Take(topSize).ToListAsync(cancellationToken);
    }

    public Task<List<TObject>> TopAsync<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (sortConditions != null)
            foreach (var item in sortConditions)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        queryable.Take(topSize);

        if (selectExpression == null)
            return queryable.WithTransaction(UnitOfWork?.Transaction).ToListAsync<TObject>(cancellationToken);
        
        return queryable.WithTransaction(UnitOfWork?.Transaction).ToListAsync(selectExpression, cancellationToken);
    }

    #endregion

    #region 分页
    
    public async Task<PageResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, bool isReturnTotal = true, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (sortConditions != null)
            foreach (var item in sortConditions)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        var result = await queryable.WithTransaction(UnitOfWork?.Transaction).CountIf(isReturnTotal, out var totalRows)
                                    .Page(pageNumber, pageSize)
                                    .ToListAsync(cancellationToken);

        return new PageResult<List<TEntity>>(pageNumber, pageSize, (int)totalRows, result);
    }
    
    public async Task<PageResult<List<TObject>>> PagedAsync<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, bool isReturnTotal = true, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (sortConditions != null)
            foreach (var item in sortConditions)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        queryable.CountIf(isReturnTotal, out var totalRows).Page(pageNumber, pageSize);

        List<TObject> result;

        if (selectExpression == null)
            result = await queryable.WithTransaction(UnitOfWork?.Transaction)
                                    .ToListAsync<TObject>(cancellationToken);
        else
            result = await queryable.WithTransaction(UnitOfWork?.Transaction)
                                    .ToListAsync(selectExpression, cancellationToken);

        return new PageResult<List<TObject>>(pageNumber, pageSize, (int)totalRows, result);
    }

    #endregion

    #region 函数查询

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
    {
        if (whereExpression == null)
            return (await _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                .WithTransaction(UnitOfWork?.Transaction).CountAsync(cancellationToken)).To<int>();
        return (await _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).CountAsync(cancellationToken)).To<int>();
    }

    public Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
    {
        if (whereExpression == null)
            return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                .WithTransaction(UnitOfWork?.Transaction).AnyAsync(cancellationToken);
        return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).AnyAsync(cancellationToken);
    }
    
    #endregion
}