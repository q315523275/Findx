using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Data;
using Findx.Extensions;
using Findx.Security;
using FreeSql.Extensions.EntityUtil;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Findx.FreeSql;

/// <summary>
///     自定义主键类型仓储实现服务
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public class RepositoryWithTypedId<TEntity, TKey> : IRepository<TEntity, TKey>, IDisposable where TEntity : class, IEntity<TKey>
{
    private readonly EntityExtensionAttribute _entityExtensionAttribute;

    private readonly Type _entityType = typeof(TEntity);
    private readonly IFreeSql _fsql;
    private readonly IOptionsMonitor<FreeSqlOptions> _options;
    private readonly IPrincipal _principal;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    public RepositoryWithTypedId(IServiceProvider serviceProvider)
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

    private Func<string, string> AsTableValueInternal { get; set; }
    private Func<Type, string, string> AsTableSelectValueInternal { get; set; }

    /// <summary>
    ///     获取Orm配置
    /// </summary>
    private FreeSqlOptions Options => _options?.CurrentValue;

    /// <summary>
    ///     获取或设置：工作单元
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
            fInsert.AsTable(_ => (entity as ITableSharding).GetShardingTableName());
        else
            fInsert.AsTable(AsTableValueInternal);

        return fInsert.WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
    }

    public int Insert(IEnumerable<TEntity> entities)
    {
        entities = CheckInsert(entities);
        // ReSharper disable once PossibleMultipleEnumeration
        var result = _fsql.Insert(entities).AsTable(AsTableValueInternal).WithTransaction(UnitOfWork?.Transaction)
            .ExecuteAffrows();
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

    public int Delete(TKey key)
    {
        if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault())
            return _fsql.Update<TEntity>(key).AsTable(AsTableValueInternal)
                .Set(it => (it as ISoftDeletable).IsDeleted == true)
                .Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now)
                .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();

        return _fsql.Delete<TEntity>(key).AsTable(AsTableValueInternal).WithTransaction(UnitOfWork?.Transaction)
            .ExecuteAffrows();
    }

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

    public int Delete(Expression<Func<TEntity, bool>> whereExpression = null)
    {
        if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault() && whereExpression == null)
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal)
                .Set(it => (it as ISoftDeletable).IsDeleted == true)
                .Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).Where(it => true)
                .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();

        if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault() && whereExpression != null)
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal)
                .Set(it => (it as ISoftDeletable).IsDeleted == true)
                .Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now).Where(whereExpression)
                .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();

        if (whereExpression == null)
            return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(it => true)
                .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
        return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
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
        return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(columns).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
    }

    public int UpdateColumns(Dictionary<string, object> dict)
    {
        var tableName = GetDbTableName();
        return _fsql.UpdateDict(dict).AsTable(tableName).WherePrimary("id").WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
    }

    public int UpdateColumns(Dictionary<string, object> dict, Expression<Func<TEntity, bool>> whereExpression)
    {
        return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).SetDto(dict).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
    }

    public Task<int> UpdateColumnsAsync(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
    {
        return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(columns).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
    }

    public int UpdateColumns(List<Expression<Func<TEntity, bool>>> columns, Expression<Func<TEntity, bool>> whereExpression)
    {
        Check.NotNull(columns, nameof(columns));
        Check.NotNull(whereExpression, nameof(whereExpression));

        var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal);

        foreach (var item in columns) update.Set(item);

        return update.Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
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
        var tableName = GetDbTableName();
        return _fsql.UpdateDict(dict).AsTable(tableName).WherePrimary("id").WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
    }

    public Task<int> UpdateColumnsAsync(Dictionary<string, object> dict, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
    {
        return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).SetDto(dict).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrowsAsync(cancellationToken);
    }
    
    private readonly Dictionary<string, TEntity> _attachDict = new();

    public void Attach(TEntity entity)
    {
        var key = entity.Id.SafeString();
        if (key.IsNotNullOrWhiteSpace()) _attachDict[key] = entity;
    }
    
    public int Save(TEntity entity)
    {
        var table = _fsql.CodeFirst.GetTableByEntity(_entityType);
        if (!table.Primarys.Any()) 
            throw new Exception($"实体{table.CsName}必须存在主键配置");
        
        var key = entity.Id.SafeString();
        if (key.IsNullOrWhiteSpace())
            throw new Exception($"实体{table.CsName}的主键值不可为空");
        
        if (!_attachDict.TryGetValue(key, out var oldValue)) oldValue = Get(entity.Id);

        if (oldValue == null)
        {
            entity.SetEmptyKey();
            return Insert(entity);
        }
        
        var dic = _fsql.CompareChangeValues(entity, oldValue);
        dic.Add("id", entity.Id);
        
        _attachDict[key] = entity;
        
        return UpdateColumns(dic);
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
        dic.Add("id", entity.Id);
        
        _attachDict[key] = entity;
        
        return await UpdateColumnsAsync(dic, cancellationToken);
    }
    
    #endregion

    #region 查询

    public TEntity Get(TKey key)
    {
        return _fsql.Select<TEntity>(key).AsTable(AsTableSelectValueInternal)
            .WithTransaction(UnitOfWork?.Transaction).First();
    }

    public Task<TEntity> GetAsync(TKey key, CancellationToken cancellationToken = default)
    {
        return _fsql.Select<TEntity>(key).AsTable(AsTableSelectValueInternal)
            .WithTransaction(UnitOfWork?.Transaction).FirstAsync(cancellationToken);
    }

    public TEntity First(Expression<Func<TEntity, bool>> whereExpression = null)
    {
        if (whereExpression == null)
            return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                .WithTransaction(UnitOfWork?.Transaction).First();
        return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).First();
    }

    public Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
    {
        if (whereExpression == null)
            return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                .WithTransaction(UnitOfWork?.Transaction).FirstAsync(cancellationToken);
        return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).FirstAsync(cancellationToken);
    }


    public List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null)
    {
        var queryable = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (orderParameters != null)
            foreach (var item in orderParameters)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        return queryable.WithTransaction(UnitOfWork?.Transaction).ToList();
    }

    public Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (orderParameters != null)
            foreach (var item in orderParameters)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        return queryable.WithTransaction(UnitOfWork?.Transaction).ToListAsync(cancellationToken);
    }

    public List<TObject> Select<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null)
    {
        var select = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            select.Where(whereExpression);

        if (orderParameters != null)
            foreach (var item in orderParameters)
                if (item.SortDirection == ListSortDirection.Ascending)
                    select.OrderBy(item.Conditions);
                else
                    select.OrderByDescending(item.Conditions);

        if (selectExpression == null)
            return select.WithTransaction(UnitOfWork?.Transaction).ToList<TObject>();
        return select.WithTransaction(UnitOfWork?.Transaction).ToList(selectExpression);
    }

    public Task<List<TObject>> SelectAsync<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default)
    {
        var select = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            select.Where(whereExpression);

        if (orderParameters != null)
            foreach (var item in orderParameters)
                if (item.SortDirection == ListSortDirection.Ascending)
                    select.OrderBy(item.Conditions);
                else
                    select.OrderByDescending(item.Conditions);

        if (selectExpression == null)
            return select.WithTransaction(UnitOfWork?.Transaction).ToListAsync<TObject>(cancellationToken);
        return select.WithTransaction(UnitOfWork?.Transaction).ToListAsync(selectExpression, cancellationToken);
    }


    public List<TEntity> Top(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (orderParameters != null)
            foreach (var item in orderParameters)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        return queryable.WithTransaction(UnitOfWork?.Transaction).Take(topSize).ToList();
    }

    public Task<List<TEntity>> TopAsync(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (orderParameters != null)
            foreach (var item in orderParameters)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        return queryable.WithTransaction(UnitOfWork?.Transaction).Take(topSize).ToListAsync(cancellationToken);
    }

    public List<TObject> Top<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (orderParameters != null)
            foreach (var item in orderParameters)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        queryable.Take(topSize);

        if (selectExpression == null)
            return queryable.WithTransaction(UnitOfWork?.Transaction).ToList<TObject>();
        return queryable.WithTransaction(UnitOfWork?.Transaction).ToList(selectExpression);
    }

    public Task<List<TObject>> TopAsync<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (orderParameters != null)
            foreach (var item in orderParameters)
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
    
    public PageResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, bool returnCount = true)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (orderParameters != null)
            foreach (var item in orderParameters)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        var result = queryable.WithTransaction(UnitOfWork?.Transaction)
                              .CountIf(returnCount, out var totalRows)
                              .Page(pageNumber, pageSize)
                              .ToList();

        return new PageResult<List<TEntity>>(pageNumber, pageSize, (int)totalRows, result);
    }

    public async Task<PageResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, bool returnCount = true, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (orderParameters != null)
            foreach (var item in orderParameters)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        var result = await queryable.WithTransaction(UnitOfWork?.Transaction).CountIf(returnCount, out var totalRows)
            .Page(pageNumber, pageSize).ToListAsync(cancellationToken);

        return new PageResult<List<TEntity>>(pageNumber, pageSize, (int)totalRows, result);
    }

    public PageResult<List<TObject>> Paged<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, bool returnCount = true)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (orderParameters != null)
            foreach (var item in orderParameters)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        queryable.CountIf(returnCount, out var totalRows).Page(pageNumber, pageSize);

        var result = selectExpression == null
            ? queryable.WithTransaction(UnitOfWork?.Transaction).ToList<TObject>()
            : queryable.WithTransaction(UnitOfWork?.Transaction).ToList(selectExpression);

        return new PageResult<List<TObject>>(pageNumber, pageSize, (int)totalRows, result);
    }

    public async Task<PageResult<List<TObject>>> PagedAsync<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, bool returnCount = true, CancellationToken cancellationToken = default)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null)
            queryable.Where(whereExpression);

        if (orderParameters != null)
            foreach (var item in orderParameters)
                if (item.SortDirection == ListSortDirection.Ascending)
                    queryable.OrderBy(item.Conditions);
                else
                    queryable.OrderByDescending(item.Conditions);

        queryable.CountIf(returnCount, out var totalRows).Page(pageNumber, pageSize);

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

    public int Count(Expression<Func<TEntity, bool>> whereExpression = null)
    {
        if (whereExpression == null)
            return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                .WithTransaction(UnitOfWork?.Transaction).Count().To<int>();
        return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).Count().To<int>();
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default)
    {
        if (whereExpression == null)
            return (await _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                .WithTransaction(UnitOfWork?.Transaction).CountAsync(cancellationToken)).To<int>();
        return (await _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).CountAsync(cancellationToken)).To<int>();
    }

    public bool Exist(Expression<Func<TEntity, bool>> whereExpression = null)
    {
        if (whereExpression == null)
            return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                .WithTransaction(UnitOfWork?.Transaction).Any();
        return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).Any();
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

    #region 库表

    public string GetDataSource()
    {
        return _entityExtensionAttribute.DataSource;
    }
    
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

    public IRepository<TEntity, TKey> AsTable(Func<string, string> rule)
    {
        AsTableValueInternal = rule;
        AsTableSelectValueInternal = rule == null
            ? null
            : new Func<Type, string, string>((a, b) => a == _entityType ? rule(b) : null);

        return this;
    }

    public Dictionary<string, object[]> CompareState(TEntity newData, TEntity oldData)
    {
        if (newData == null) 
            return null;
        
        var entityType = typeof(TEntity);
        
        var table = _fsql.CodeFirst.GetTableByEntity(entityType);
        if (table.Primarys.Any() == false) 
            throw new Exception($"实体{table.CsName}必须存在主键配置");
        
        var key = _fsql.GetEntityKeyString(entityType, newData, false);
        if (string.IsNullOrEmpty(key)) 
            throw new Exception($"实体{table.CsName}的主键值不可为空");

        var res = _fsql.CompareEntityValueReturnColumns(entityType, oldData, newData, false).ToDictionary(a => a, a => new[]
        {
            _fsql.GetEntityValueWithPropertyName(entityType, newData, a),
            _fsql.GetEntityValueWithPropertyName(entityType, oldData, a)
        });

        return res;
    }

    public DatabaseType GetDbType()
    {
        var dataType = _fsql.Ado.DataType.ToString();
        return dataType.CastTo<DatabaseType>();
    }

    #endregion

    #region 私有方法

    /// <summary>
    ///     检查插入信息
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    private TEntity[] CheckInsert(params TEntity[] entities)
    {
        if (!Options.CheckInsert) return entities;

        var userIdTypeName = _principal?.Identity.GetClaimValueFirstOrDefault(ClaimTypes.UserIdTypeName);
        for (var i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            entities[i] = entity.CheckCreatedTime();

            if (userIdTypeName == null) continue;
            entity = entities[i];
            if (userIdTypeName == typeof(int).FullName)
                entities[i] = entity.CheckCreationAudited<TEntity, int>(_principal);
            else if (userIdTypeName == typeof(Guid).FullName)
                entities[i] = entity.CheckCreationAudited<TEntity, Guid>(_principal);
            else
                entities[i] = entity.CheckCreationAudited<TEntity, long>(_principal);
        }

        return entities;
    }

    /// <summary>
    ///     检查更新信息
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    private TEntity[] CheckUpdate(params TEntity[] entities)
    {
        if (!Options.CheckUpdate) return entities;

        var userIdTypeName = _principal?.Identity.GetClaimValueFirstOrDefault(ClaimTypes.UserIdTypeName);
        for (var i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            entities[i] = entity.CheckUpdateTime();

            if (userIdTypeName == null) continue;
            if (userIdTypeName == typeof(int).FullName)
                entities[i] = entity.CheckUpdateAudited<TEntity, int>(_principal);
            else if (userIdTypeName == typeof(Guid).FullName)
                entities[i] = entity.CheckUpdateAudited<TEntity, Guid>(_principal);
            else
                entities[i] = entity.CheckUpdateAudited<TEntity, long>(_principal);
        }

        return entities;
    }

    /// <summary>
    ///     检查插入信息
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    private IEnumerable<TEntity> CheckInsert(IEnumerable<TEntity> entities)
    {
        if (!Options.CheckInsert) return entities;

        var userIdTypeName = _principal?.Identity.GetClaimValueFirstOrDefault(ClaimTypes.UserIdTypeName);
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var entity in entities)
        {
            entity.CheckCreatedTime();

            if (userIdTypeName == null) continue;
            if (userIdTypeName == typeof(int).FullName)
                entity.CheckCreationAudited<TEntity, int>(_principal);
            else if (userIdTypeName == typeof(Guid).FullName)
                entity.CheckCreationAudited<TEntity, Guid>(_principal);
            else
                entity.CheckCreationAudited<TEntity, long>(_principal);
        }

        // ReSharper disable once PossibleMultipleEnumeration
        return entities;
    }

    /// <summary>
    ///     检查更新信息
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    private IEnumerable<TEntity> CheckUpdate(IEnumerable<TEntity> entities)
    {
        if (!Options.CheckUpdate) return entities;

        var userIdTypeName = _principal?.Identity.GetClaimValueFirstOrDefault(ClaimTypes.UserIdTypeName);
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var entity in entities)
        {
            entity.CheckUpdateTime();

            if (userIdTypeName == null) continue;
            if (userIdTypeName == typeof(int).FullName)
                entity.CheckUpdateAudited<TEntity, int>(_principal);
            else if (userIdTypeName == typeof(Guid).FullName)
                entity.CheckUpdateAudited<TEntity, Guid>(_principal);
            else
                entity.CheckUpdateAudited<TEntity, long>(_principal);
        }

        // ReSharper disable once PossibleMultipleEnumeration
        return entities;
    }

    #endregion

    public void Dispose()
    {
        AsTableValueInternal = null;
        AsTableSelectValueInternal = null;
        _attachDict.Clear();
    }
}