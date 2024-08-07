using System;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.Extensions;
using FreeSql;

namespace Findx.FreeSql;

/// <summary>
///     仓储扩展
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    ///     置换FreeSql
    /// </summary>
    /// <param name="repository"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static IFreeSql AsFreeSql<TEntity>(this IRepository<TEntity> repository) where TEntity: class, IEntity<Guid>
    {
        var dataSource = repository.GetDataSource();
        var clients = ServiceLocator.GetService<FreeSqlClient>();
        return clients.GetOrDefault(dataSource);
    }

    /// <summary>
    ///     置换FreeSql
    /// </summary>
    /// <param name="repository"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey">主键</typeparam>
    /// <returns></returns>
    public static IFreeSql AsFreeSql<TEntity, TKey>(this IRepository<TEntity, TKey> repository) where TEntity : class, IEntity<TKey>
    {
        var dataSource = repository.GetDataSource();
        var clients = ServiceLocator.GetService<FreeSqlClient>();
        return clients.GetOrDefault(dataSource);
    }
    
    /// <summary>
    ///     置换ISelect
    /// </summary>
    /// <param name="repository"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static ISelect<TEntity> AsSelect<TEntity>(this IRepository<TEntity> repository) where TEntity : class, IEntity<Guid>
    {
        var dataSource = repository.GetDataSource();
        var clients = ServiceLocator.GetService<FreeSqlClient>();
        return clients.GetOrDefault(dataSource).Select<TEntity>().WithTransaction(repository.UnitOfWork?.Transaction);
    }
    
    /// <summary>
    ///     置换ISelect
    /// </summary>
    /// <param name="repository"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey">主键</typeparam>
    /// <returns></returns>
    public static ISelect<TEntity> AsSelect<TEntity, TKey>(this IRepository<TEntity, TKey> repository) where TEntity : class, IEntity<TKey>
    {
        var dataSource = repository.GetDataSource();
        var clients = ServiceLocator.GetService<FreeSqlClient>();
        return clients.GetOrDefault(dataSource).Select<TEntity>().WithTransaction(repository.UnitOfWork?.Transaction);
    }
    
    /// <summary>
    ///     置换IUpdate
    /// </summary>
    /// <param name="repository"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static IUpdate<TEntity> AsUpdate<TEntity>(this IRepository<TEntity> repository) where TEntity : class, IEntity<Guid>
    {
        var dataSource = repository.GetDataSource();
        var clients = ServiceLocator.GetService<FreeSqlClient>();
        return clients.GetOrDefault(dataSource).Update<TEntity>().WithTransaction(repository.UnitOfWork?.Transaction);
    }
    
    /// <summary>
    ///     置换ISelect
    /// </summary>
    /// <param name="repository"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey">主键</typeparam>
    /// <returns></returns>
    public static IUpdate<TEntity> AsUpdate<TEntity, TKey>(this IRepository<TEntity, TKey> repository) where TEntity : class, IEntity<TKey>
    {
        var dataSource = repository.GetDataSource();
        var clients = ServiceLocator.GetService<FreeSqlClient>();
        return clients.GetOrDefault(dataSource).Update<TEntity>().WithTransaction(repository.UnitOfWork?.Transaction);
    }
}