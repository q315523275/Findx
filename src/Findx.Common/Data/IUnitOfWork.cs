using System.Data;

namespace Findx.Data
{
    /// <summary>
    /// 工作单元
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// 开启事务
        /// </summary>
        void BeginTran();
        /// <summary>
        /// 开启事务
        /// </summary>
        /// <param name="il"></param>
        void BeginTran(IsolationLevel il);
        /// <summary>
        /// 提交事务
        /// </summary>
        void CommitTran();
        /// <summary>
        /// 回滚事务
        /// </summary>
        void RollbackTran();
    }
    /// <summary>
    /// 泛型工作单元
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUnitOfWork<T> : IUnitOfWork
    {
        /// <summary>
        /// 获取泛型实例
        /// </summary>
        /// <returns></returns>
        T GetInstance();
    }
}
