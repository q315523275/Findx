namespace Findx.Data;

/// <summary>
///     工作单元扩展
/// </summary>
public static class UnitOfWorkExtensions
{
    /// <summary>
    ///     获取工作单元仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="uow"></param>
    /// <returns></returns>
    public static IRepository<TEntity> GetRepository<TEntity>(this IUnitOfWork uow) where TEntity : class, IEntity<Guid>
    {
        var repo = uow.ServiceProvider.GetRequiredService<IRepository<TEntity>>();
        repo.UnitOfWork = uow;
        return repo;
    }
    
    /// <summary>
    ///     获取工作单元仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="uow"></param>
    /// <returns></returns>
    public static IRepository<TEntity, TKey> GetRepository<TEntity, TKey>(this IUnitOfWork uow) where TEntity : class, IEntity<TKey>
    {
        var repo = uow.ServiceProvider.GetRequiredService<IRepository<TEntity, TKey>>();
        repo.UnitOfWork = uow;
        return repo;
    }
    
    /// <summary>
    ///     设置仓库工作单元
    /// </summary>
    /// <param name="repo"></param>
    /// <param name="uow"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static IRepository<TEntity> WithUnitOfWork<TEntity>(this IRepository<TEntity> repo, IUnitOfWork uow) where TEntity : class, IEntity<Guid>
    {
        repo.UnitOfWork = uow;
        return repo;
    }

    /// <summary>
    ///     设置仓库工作单元
    /// </summary>
    /// <param name="repo"></param>
    /// <param name="uow"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <returns></returns>
    public static IRepository<TEntity, TKey> WithUnitOfWork<TEntity, TKey>(this IRepository<TEntity, TKey> repo, IUnitOfWork uow) where TEntity : class, IEntity<TKey>
    {
        repo.UnitOfWork = uow;
        return repo;
    }
}