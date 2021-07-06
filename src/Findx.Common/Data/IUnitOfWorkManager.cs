namespace Findx.Data
{
    /// <summary>
    /// 工作单元管理器
    /// </summary>
    public interface IUnitOfWorkManager
    {
        /// <summary>
        /// 开启事务
        /// </summary>
        void BeginTran();
        /// <summary>
        /// 提交事务
        /// </summary>
        void CommitTran();
        /// <summary>
        /// 回滚事务
        /// </summary>
        void RollbackTran();
    }
}
