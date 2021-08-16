using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件事务器
    /// </summary>
    public interface IEventTransaction
    {
        /// <summary>
        /// 是否自动提交
        /// </summary>
        bool AutoCommit { get; set; }

        /// <summary>
        /// 存储事务对象
        /// </summary>
        object DbTransaction { get; set; }

        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit();

        /// <summary>
        /// 异步提交事务
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback();

        /// <summary>
        /// 异步回滚事务
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
