using System.Threading;
using System.Threading.Tasks;
using Findx.Data;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件工作单元
    /// </summary>
    public interface IEventUnitOfWork
    {
        /// <summary>
        /// 实际执行工作单元
        /// </summary>
        IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// 添加至缓冲区
        /// </summary>
        void AddToBuffer(Message message);

        /// <summary>
        /// 提交当前上下文的事务更改
        /// </summary>
        void Commit();
        
        /// <summary>
        /// 异步提交当前上下文的事务更改
        /// </summary>
        /// <returns></returns>
        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}