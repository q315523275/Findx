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
    ///     根据库标识获取工作单元
    /// </summary>
    /// <param name="primary">工作单元标识</param>
    /// <param name="enableTransaction">是否启用事务</param>
    /// <param name="beginTransaction">是否开启事物</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IUnitOfWork> GetUnitOfWorkAsync(string primary, bool enableTransaction, bool beginTransaction, CancellationToken cancellationToken = default)
    {
        var primaryKey = NormalizePrimary(primary ?? GetDefaultPrimary());
        var unitOfWork = _scopedDictionary.GetUnitOfWork(primaryKey);
        if (unitOfWork != null)
        {
            if (enableTransaction) unitOfWork.EnableTransaction();
            if (beginTransaction) await unitOfWork.BeginOrUseTransactionAsync(cancellationToken);

            return unitOfWork;
        }

        unitOfWork = CreateConnUnitOfWork(primaryKey);
        _scopedDictionary.SetUnitOfWork(primaryKey, unitOfWork);
        
        if (enableTransaction) unitOfWork.EnableTransaction();
        if (beginTransaction) await unitOfWork.BeginOrUseTransactionAsync(cancellationToken);

        return unitOfWork;
    }

    /// <summary>
    ///     根据实体获取工作单元
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="enableTransaction">是否启用事务</param>
    /// <param name="beginTransaction">是否开启事物</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IUnitOfWork> GetEntityUnitOfWorkAsync(Type entityType, bool enableTransaction, bool beginTransaction, CancellationToken cancellationToken = default)
    {
        return GetUnitOfWorkAsync(entityType.GetEntityExtensionAttribute()?.DataSource, enableTransaction, beginTransaction, cancellationToken);
    }

    /// <summary>
    ///     根据实体获取工作单元
    /// </summary>
    /// <param name="enableTransaction">是否启用事务</param>
    /// <param name="beginTransaction">是否开启事物</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IUnitOfWork> GetEntityUnitOfWorkAsync<TEntity>(bool enableTransaction, bool beginTransaction, CancellationToken cancellationToken = default)
    {
        return GetEntityUnitOfWorkAsync(typeof(TEntity), enableTransaction, beginTransaction, cancellationToken);
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
    ///     获取默认数据源标识
    ///     <para>调用方未指定数据源时使用，与选项里的 Primary 同义</para>
    /// </summary>
    /// <returns></returns>
    protected abstract string GetDefaultPrimary();

    /// <summary>
    ///     归一化数据源标识
    ///     <para>多个标识共享同一连接时（如 FreeSql 的 DataSourceSharing 别名），归到同一标识才能复用同一个工作单元，否则会各开一个连接、各开一个事务</para>
    /// </summary>
    /// <param name="primary">数据源标识</param>
    /// <returns></returns>
    protected virtual string NormalizePrimary(string primary)
    {
        return primary;
    }
}