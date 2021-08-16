using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.EventBus
{
    /// <summary>
    /// 事件事务基类
    /// </summary>
    public abstract class EventTransactionBase : IEventTransaction
    {
        private readonly IEventDispatcher _dispatcher;

        private readonly ConcurrentQueue<EventMediumMessage> _bufferList;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="dispatcher"></param>
        protected EventTransactionBase(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _bufferList = new ConcurrentQueue<EventMediumMessage>();
        }

        public bool AutoCommit { get; set; }

        public object DbTransaction { get; set; }

        protected internal virtual void AddToBuffer(EventMediumMessage msg)
        {
            _bufferList.Enqueue(msg);
        }

        protected virtual void Flush()
        {
            while (!_bufferList.IsEmpty)
            {
                _bufferList.TryDequeue(out var message);

                _dispatcher.EnqueueToPublish(message);
            }
        }


        public abstract void Begin();
        public abstract Task BeginAsync(CancellationToken cancellationToken = default);
        public abstract void Commit();
        public abstract Task CommitAsync(CancellationToken cancellationToken = default);
        public abstract void Rollback();
        public abstract Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
