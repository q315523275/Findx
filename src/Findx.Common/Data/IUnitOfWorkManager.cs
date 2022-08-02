﻿namespace Findx.Data
{
    /// <summary>
    /// 工作单元管理器
    /// </summary>
    public interface IUnitOfWorkManager
    {
        /// <summary>
        /// 根据数据源名获取工作单元
        /// </summary>
        /// <param name="dbPrimary">连接Key</param>
        /// <param name="enableTransaction">是否启用事务</param>
        /// <returns></returns>
        IUnitOfWork GetConnUnitOfWork(string dbPrimary = default, bool enableTransaction = false);
    }
}
