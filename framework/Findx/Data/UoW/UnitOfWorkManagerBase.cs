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
    /// <param name="dbPrimary"></param>
    /// <param name="enableTransaction">是否启用事务</param>
    /// <param name="beginTransaction">是否开启事物</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IUnitOfWork> GetConnUnitOfWorkAsync(bool enableTransaction = false, bool beginTransaction = false, string dbPrimary = default, CancellationToken cancellationToken = default)
    {
        var cacheKey = dbPrimary ?? GetDataSourcePrimary();
        var unitOfWork = _scopedDictionary.GetConnUnitOfWork(cacheKey);
        if (unitOfWork != null)
        {
            if (enableTransaction) unitOfWork.EnableTransaction();

            if (beginTransaction) await unitOfWork.BeginOrUseTransactionAsync(cancellationToken);
            return unitOfWork;
        }

        unitOfWork = CreateConnUnitOfWork(dbPrimary);
        _scopedDictionary.SetConnUnitOfWork(cacheKey, unitOfWork);
        if (enableTransaction) unitOfWork.EnableTransaction();
        if (beginTransaction) await unitOfWork.BeginOrUseTransactionAsync(cancellationToken);
        
        return unitOfWork;
    }

    /// <summary>
    ///     根据实体获取工作单元
    /// </summary>
    /// <param name="enableTransaction">是否启用事务</param>
    /// <param name="beginTransaction">是否启用事务</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IUnitOfWork> GetEntityUnitOfWorkAsync<TEntity>(bool enableTransaction = false, bool beginTransaction = false, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        var extensionAttribute = entityType.GetEntityExtensionAttribute();
        var dataSource = extensionAttribute?.DataSource ?? GetDataSourcePrimary();

        var unitOfWork = _scopedDictionary.GetEntityUnitOfWork(entityType);
        if (unitOfWork != null)
        {
            if (enableTransaction) unitOfWork.EnableTransaction();
            if (beginTransaction) await unitOfWork.BeginOrUseTransactionAsync(cancellationToken);
            return unitOfWork;
        }

        unitOfWork = CreateConnUnitOfWork(dataSource);
        _scopedDictionary.SetEntityUnitOfWork(entityType, unitOfWork);
        if (enableTransaction) unitOfWork.EnableTransaction();
        if (beginTransaction) await unitOfWork.BeginOrUseTransactionAsync(cancellationToken);

        return unitOfWork;
    }

    /// <summary>
    ///     获取所以已创建工作单元
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IEnumerable<IUnitOfWork>> GetAllConnUnitOfWorkAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_scopedDictionary.GetAllConnUnitOfWork());
    }

    /// <summary>
    ///     获取所以已创建工作单元
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IEnumerable<IUnitOfWork>> GetAllEntityUnitOfWorkAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_scopedDictionary.GetAllEntityUnitOfWork());
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