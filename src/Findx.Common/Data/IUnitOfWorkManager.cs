namespace Findx.Data
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
        /// <param name="beginTransaction">是否开启事物</param>
        /// <returns></returns>
        IUnitOfWork GetConnUnitOfWork(bool enableTransaction = false, bool beginTransaction = false, string dbPrimary = default);

        /// <summary>
        /// 根据实体获取工作单元
        /// </summary>
        /// <param name="enableTransaction">是否启用事务</param>
        /// <param name="beginTransaction">是否开启事物</param>
        /// <returns></returns>
        IUnitOfWork GetEntityUnitOfWork<TEntity>(bool enableTransaction = false, bool beginTransaction = false);
    }
}
