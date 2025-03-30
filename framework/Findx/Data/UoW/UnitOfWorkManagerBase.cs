using System.Threading.Tasks;

namespace Findx.Data;

/// <summary>
///     工作单元管理类基类
/// </summary>
public abstract class UnitOfWorkManagerBase : IUnitOfWorkManager
{
    private readonly ScopedDictionary _scopedDictionary;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="scopedDictionary"></param>
    protected UnitOfWorkManagerBase(ScopedDictionary scopedDictionary)
    {
        _scopedDictionary = scopedDictionary;
    }
    
    /// <summary>
    ///     获取指定库工作单元
    /// </summary>
    /// <param name="primary">工作单元标识</param>
    /// <param name="enableTransaction">是否启用事务</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IUnitOfWork> GetUnitOfWorkAsync(string primary = null, bool enableTransaction = false, CancellationToken cancellationToken = default)
    {
        var primaryKey = primary ?? GetDataSourcePrimary();
        var unitOfWork = _scopedDictionary.GetUnitOfWork(primaryKey);
        if (unitOfWork != null)
        {
            if (enableTransaction)
            {
                unitOfWork.EnableTransaction();
                await unitOfWork.BeginOrUseTransactionAsync(cancellationToken);
            }

            return unitOfWork;
        }

        unitOfWork = CreateConnUnitOfWork(primaryKey);
        _scopedDictionary.SetUnitOfWork(primaryKey, unitOfWork);
        
        if (enableTransaction)
        {
            unitOfWork.EnableTransaction();
            await unitOfWork.BeginOrUseTransactionAsync(cancellationToken);
        }

        return unitOfWork;
    }

    /// <summary>
    ///     根据实体获取工作单元
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="enableTransaction">是否启用事务</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IUnitOfWork> GetEntityUnitOfWorkAsync(Type entityType, bool enableTransaction = false, CancellationToken cancellationToken = default)
    {
        return GetUnitOfWorkAsync(entityType.GetEntityExtensionAttribute()?.DataSource, enableTransaction, cancellationToken);
    }

    /// <summary>
    ///     根据实体获取工作单元
    /// </summary>
    /// <param name="enableTransaction">是否启用事务</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IUnitOfWork> GetEntityUnitOfWorkAsync<TEntity>(bool enableTransaction = false, CancellationToken cancellationToken = default)
    {
        return GetEntityUnitOfWorkAsync(typeof(TEntity), enableTransaction, cancellationToken);
    }

    /// <summary>
    ///     获取所以已创建工作单元
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IEnumerable<IUnitOfWork>> GetAllUnitOfWorkAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_scopedDictionary.GetAllUnitOfWork());
    }
    
    /// <summary>
    ///     获取指定DB连接工作单元
    /// </summary>
    /// <param name="dbPrimary"></param>
    /// <returns></returns>
    protected abstract IUnitOfWork CreateConnUnitOfWork(string dbPrimary);
    
    /// <summary>
    ///     获取默认数据库链接标识
    /// </summary>
    /// <returns></returns>
    protected abstract string GetDataSourcePrimary();
}