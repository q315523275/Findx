using System.Threading.Tasks;

namespace Findx.Data;

/// <summary>
///     系统扩展 - 工作单元
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     工作单元扩展 - 获取工作单元
    /// </summary>
    /// <param name="unitOfWorkManager"></param>
    /// <param name="enableTransaction">启用事物</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<IUnitOfWork> GetUnitOfWorkAsync(this IUnitOfWorkManager unitOfWorkManager, bool enableTransaction = false, CancellationToken cancellationToken = default)
    {
        return unitOfWorkManager.GetUnitOfWorkAsync(null, enableTransaction, enableTransaction, cancellationToken);
    }

    /// <summary>
    ///     工作单元扩展 - 获取工作单元
    /// </summary>
    /// <param name="unitOfWorkManager"></param>
    /// <param name="primary">数据库标识</param>
    /// <param name="enableTransaction">启用事物</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<IUnitOfWork> GetUnitOfWorkAsync(this IUnitOfWorkManager unitOfWorkManager, string primary = null, bool enableTransaction = false, CancellationToken cancellationToken = default)
    {
        return unitOfWorkManager.GetUnitOfWorkAsync(primary, enableTransaction, enableTransaction, cancellationToken);
    }

    /// <summary>
    ///     工作单元扩展 - 根据实体类型获取工作单元
    /// </summary>
    /// <param name="unitOfWorkManager"></param>
    /// <param name="entityType">实体类型</param>
    /// <param name="enableTransaction">是否启用事务</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<IUnitOfWork> GetEntityUnitOfWorkAsync(this IUnitOfWorkManager unitOfWorkManager, Type entityType, bool enableTransaction = false, CancellationToken cancellationToken = default)
    {
        return unitOfWorkManager.GetEntityUnitOfWorkAsync(entityType, enableTransaction, enableTransaction, cancellationToken);
    }

    /// <summary>
    ///     工作单元扩展 - 根据实体获取工作单元
    /// </summary>
    /// <param name="unitOfWorkManager"></param>
    /// <param name="enableTransaction">是否启用事务</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<IUnitOfWork> GetEntityUnitOfWorkAsync<TEntity>(this IUnitOfWorkManager unitOfWorkManager, bool enableTransaction = false, CancellationToken cancellationToken = default)
    {
        return unitOfWorkManager.GetEntityUnitOfWorkAsync(typeof(TEntity), enableTransaction, enableTransaction, cancellationToken);
    }
}