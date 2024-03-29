﻿using System.Threading.Tasks;

namespace Findx.Data;

/// <summary>
///     工作单元管理器
/// </summary>
public interface IUnitOfWorkManager
{
    /// <summary>
    ///     根据数据源名获取工作单元
    /// </summary>
    /// <param name="dbPrimary">连接Key</param>
    /// <param name="enableTransaction">是否启用事务</param>
    /// <param name="beginTransaction">是否开启事物</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IUnitOfWork> GetConnUnitOfWorkAsync(bool enableTransaction = false, bool beginTransaction = false, string dbPrimary = default, CancellationToken cancellationToken = default);

    /// <summary>
    ///     根据实体获取工作单元
    /// </summary>
    /// <param name="enableTransaction">是否启用事务</param>
    /// <param name="beginTransaction">是否开启事物</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IUnitOfWork> GetEntityUnitOfWorkAsync<TEntity>(bool enableTransaction = false, bool beginTransaction = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     获取所以已创建工作单元
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<IUnitOfWork>> GetAllConnUnitOfWorkAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     获取所以已创建工作单元
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<IUnitOfWork>> GetAllEntityUnitOfWorkAsync(CancellationToken cancellationToken = default);
}