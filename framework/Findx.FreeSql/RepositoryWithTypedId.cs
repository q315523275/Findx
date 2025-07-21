using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using Findx.Common;
using Findx.Data;
using Findx.Exceptions;
using Findx.Extensions;
using Findx.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Findx.FreeSql;

/// <summary>
///     自定义主键类型仓储实现服务
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public partial class RepositoryWithTypedId<TEntity, TKey> : IRepository<TEntity, TKey>, IDisposable where TEntity : class, IEntity<TKey>
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

    /// <summary>
    ///     单条插入
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public int Insert(TEntity entity)
    {
        entity.ThrowIfNull();
        entity = CheckInsert(entity)[0];
        var fInsert = _fsql.Insert(entity);
        // 存在分表标签
        // 动态自定义分表为Null
        if (_entityExtensionAttribute.HasTableSharding.GetValueOrDefault() && AsTableValueInternal == null)
            // ReSharper disable once SuspiciousTypeConversion.Global
            fInsert.AsTable(_ => (entity as ITableSharding)?.GetShardingTableName());
        else
            fInsert.AsTable(AsTableValueInternal);

        return fInsert.WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
    }

    /// <summary>
    ///     批量插入
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public int Insert(IEnumerable<TEntity> entities)
    {
        entities = CheckInsert(entities);
        return _fsql.Insert(entities).AsTable(AsTableValueInternal).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
    }

    #endregion

    #region 删除

    /// <summary>
    ///     根据主键删除数据
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public int Delete(TKey key)
    {
        if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault())
        {
            return _fsql.Update<TEntity>(key).AsTable(AsTableValueInternal) 
                        .Set(it => (it as ISoftDeletable).IsDeleted == true)
                        .Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now)
                        .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
        }
        
        return _fsql.Delete<TEntity>(key).AsTable(AsTableValueInternal).WithTransaction(UnitOfWork?.Transaction) .ExecuteAffrows();
    }
    
    /// <summary>
    ///     根据指定条件删除数据
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public int Delete(Expression<Func<TEntity, bool>> whereExpression = null)
    {
        if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault() && whereExpression == null)
        {
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal)
                        .Set(it => (it as ISoftDeletable).IsDeleted == true)
                        .Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now)
                        .Where(it => true) // freeSql 必须设置
                        .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
        }

        if (_entityExtensionAttribute.HasSoftDeletable.GetValueOrDefault() && whereExpression != null)
        {
            return _fsql.Update<TEntity>().AsTable(AsTableValueInternal)
                        .Set(it => (it as ISoftDeletable).IsDeleted == true)
                        .Set(it => (it as ISoftDeletable).DeletionTime == DateTime.Now)
                        .Where(whereExpression)
                        .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
        }
        
        if (whereExpression == null)
        {
            return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal)
                        .Where(it => true) // freeSql 必须设置
                        .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
        }

        return _fsql.Delete<TEntity>().AsTable(AsTableValueInternal)
                    .Where(whereExpression)
                    .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
    }

    #endregion

    #region 更新

    /// <summary>
    ///     更新单条实体信息
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="updateColumns"></param>
    /// <param name="ignoreColumns"></param>
    /// <param name="ignoreNullColumns"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     更新批量实体信息
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="updateColumns"></param>
    /// <param name="ignoreColumns"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     根据指定条件更新指定字段信息
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public int UpdateColumns(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression)
    {
        return _fsql.Update<TEntity>().AsTable(AsTableValueInternal).Set(columns).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
    }

    /// <summary>
    ///     根据主键Id更新指定字段信息
    /// </summary>
    /// <param name="dict"></param>
    /// <returns></returns>
    public int UpdateColumns(Dictionary<string, object> dict)
    {
        FindxException.ThrowIf(dict == null || !dict.ContainsKey("Id"), "505", "字典更新时必须包含主键Id数据");
        var tableName = GetDbTableName();
        return _fsql.UpdateDict(dict).AsTable(tableName).WherePrimary("Id").WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
    }

    /// <summary>
    ///     根据指定条件更新指定字段信息
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public int UpdateColumns(Dictionary<string, object> dict, Expression<Func<TEntity, bool>> whereExpression)
    {
        return _fsql.Update<TEntity>().AsTable(AsTableValueInternal)
                    .SetDto(dict).Where(whereExpression)
                    .WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
    }

    /// <summary>
    ///     根据指定条件更新指定字段信息
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public int UpdateColumns(List<Expression<Func<TEntity, bool>>> columns, Expression<Func<TEntity, bool>> whereExpression)
    {
        Check.NotNull(columns, nameof(columns));
        Check.NotNull(whereExpression, nameof(whereExpression));

        var update = _fsql.Update<TEntity>().AsTable(AsTableValueInternal);

        foreach (var item in columns) update.Set(item);

        return update.Where(whereExpression).WithTransaction(UnitOfWork?.Transaction).ExecuteAffrows();
    }

    /// <summary>
    ///     快照字典
    /// </summary>
    private readonly Dictionary<string, TEntity> _attachDict = new();

    /// <summary>
    ///     记录快照
    /// </summary>
    /// <param name="entity"></param>
    public void Attach(TEntity entity)
    {
        var key = entity.Id.SafeString();
        if (key.IsNotNullOrWhiteSpace())
        {
            _attachDict[key] = entity.Clone().As<TEntity>();
        }
    }
    
    /// <summary>
    ///     保存变更字段
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="NotSupportedException"></exception>
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
        
        _attachDict[key] = entity;
        
        return dic.Count > 1 ? UpdateColumns(dic) : 1;
    }

    #endregion

    #region 查询

    /// <summary>
    ///     根据主键查询单条信息
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public TEntity Get(TKey key)
    {
        var model = _fsql.Select<TEntity>(key)
                         .AsTable(AsTableSelectValueInternal)
                         .WithTransaction(UnitOfWork?.Transaction)
                         .First();
        
        if (model != null) Attach(model);
        
        return model;
    }

    /// <summary>
    ///     根据指定条件查询单条信息
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public TEntity First(Expression<Func<TEntity, bool>> whereExpression = null)
    {
        TEntity model;
        if (whereExpression == null)
        {
            model = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                         .WithTransaction(UnitOfWork?.Transaction).First();
            
            if (model != null) Attach(model);
            
            return model;
        }

        model = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                     .Where(whereExpression)
                     .WithTransaction(UnitOfWork?.Transaction).First();
        
        if (model != null) Attach(model);
        
        return model;
    }

    /// <summary>
    ///     根据指定条件及排序查询列表信息
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="sortConditions"></param>
    /// <returns></returns>
    public List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null)
    {
        var queryable = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null) queryable.Where(whereExpression);

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

        return queryable.WithTransaction(UnitOfWork?.Transaction).ToList();
    }

    /// <summary>
    ///     根据指定条件查询列表信息
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="selectExpression"></param>
    /// <param name="sortConditions"></param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public List<TObject> Select<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null)
    {
        var select = _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null) select.Where(whereExpression);

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
            return select.WithTransaction(UnitOfWork?.Transaction).ToList<TObject>();
        
        return select.WithTransaction(UnitOfWork?.Transaction).ToList(selectExpression);
    }
    
    /// <summary>
    ///     根据指定条件查询固定条数列表信息
    /// </summary>
    /// <param name="topSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="sortConditions"></param>
    /// <returns></returns>
    public List<TEntity> Top(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null) queryable.Where(whereExpression);

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
        
        return queryable.WithTransaction(UnitOfWork?.Transaction).Take(topSize).ToList();
    }

    /// <summary>
    ///     根据指定条件查询固定条数列表信息
    /// </summary>
    /// <param name="topSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="selectExpression"></param>
    /// <param name="sortConditions"></param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public List<TObject> Top<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null)
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
            return queryable.WithTransaction(UnitOfWork?.Transaction).ToList<TObject>();
        
        return queryable.WithTransaction(UnitOfWork?.Transaction).ToList(selectExpression);
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
    /// <returns></returns>
    public PageResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, bool hasTotalRows = true)
    {
        var queryable = _fsql.Queryable<TEntity>().AsTable(AsTableSelectValueInternal);

        if (whereExpression != null) queryable.Where(whereExpression);

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
        
        var result = queryable.WithTransaction(UnitOfWork?.Transaction)
                              .CountIf(hasTotalRows, out var totalRows)
                              .Page(pageNumber, pageSize)
                              .ToList();

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
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public PageResult<List<TObject>> Paged<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<SortCondition<TEntity>> sortConditions = null, bool hasTotalRows = true)
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

        var result = selectExpression == null
                            ? queryable.WithTransaction(UnitOfWork?.Transaction).ToList<TObject>()
                            : queryable.WithTransaction(UnitOfWork?.Transaction).ToList(selectExpression);

        return new PageResult<List<TObject>>(pageNumber, pageSize, totalRows, result);
    }

    #endregion

    #region 函数查询

    /// <summary>
    ///     查询指定的条件的数量
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public long Count(Expression<Func<TEntity, bool>> whereExpression = null)
    {
        if (whereExpression == null)
        {
            return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                        .WithTransaction(UnitOfWork?.Transaction).Count();
        }
            
        return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                    .Where(whereExpression)
                    .WithTransaction(UnitOfWork?.Transaction).Count();
    }

    /// <summary>
    ///     判断指定的条件是否存在
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public bool Exist(Expression<Func<TEntity, bool>> whereExpression = null)
    {
        if (whereExpression == null)
            return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal)
                .WithTransaction(UnitOfWork?.Transaction).Any();
        return _fsql.Select<TEntity>().AsTable(AsTableSelectValueInternal).Where(whereExpression)
            .WithTransaction(UnitOfWork?.Transaction).Any();
    }
    
    #endregion

    #region 库表

    /// <summary>
    ///     获取数据来源名称
    /// </summary>
    /// <returns></returns>
    public string GetDataSource()
    {
        return _entityExtensionAttribute.DataSource;
    }
    
    /// <summary>
    ///     获取表名
    /// </summary>
    /// <returns></returns>
    public string GetDbTableName()
    {
        var dbName = _fsql.CodeFirst.GetTableByEntity(_entityType).DbName;
        return AsTableValueInternal?.Invoke(dbName) ?? dbName;
    }

    /// <summary>
    ///     获取表字段名称集合
    /// </summary>
    /// <returns></returns>
    public List<string> GetDbColumnName()
    {
        var columns = _fsql.CodeFirst.GetTableByEntity(_entityType).Columns;
        return columns.Keys.ToList();
    }

    /// <summary>
    ///     更改表名规则
    /// </summary>
    /// <param name="rule"></param>
    /// <returns></returns>
    public IRepository<TEntity, TKey> AsTable(Func<string, string> rule)
    {
        AsTableValueInternal = rule;
        AsTableSelectValueInternal = rule == null
            ? null
            : new Func<Type, string, string>((a, b) => a == _entityType ? rule(b) : null);

        return this;
    }

    /// <summary>
    ///     实体属性值变更比较
    /// </summary>
    /// <param name="newData"></param>
    /// <returns></returns>
    public Dictionary<string, object[]> CompareState(TEntity newData)
    {
        if (!_attachDict.TryGetValue(newData.Id.SafeString(), out var oldData))
        {
            oldData = Get(newData.Id);
        }
        return _fsql.CompareState(newData, oldData);
    }

    /// <summary>
    ///     实体属性值变更比较
    /// </summary>
    /// <param name="newData"></param>
    /// <param name="oldData"></param>
    /// <returns></returns>
    public Dictionary<string, object[]> CompareState(TEntity newData, TEntity oldData)
    {
        return _fsql.CompareState(newData, oldData);
    }

    /// <summary>
    ///     获取数据库类型
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        AsTableValueInternal = null;
        AsTableSelectValueInternal = null;
        _attachDict.Clear();
    }
}