using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Findx.Data;

namespace Findx.EventBus.Internal
{
    /// <summary>
    /// 事件包装工作单元
    /// </summary>
    public class EventUnitOfWork: IEventUnitOfWork, IDisposable
    {
        private readonly IEventDispatcher _dispatcher;
        private readonly ConcurrentQueue<Message> _bufferList;
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="dispatcher"></param>
        public EventUnitOfWork(IUnitOfWork unitOfWork, IEventDispatcher dispatcher)
        {
            UnitOfWork = unitOfWork;
            _dispatcher = dispatcher;

            _bufferList = new ConcurrentQueue<Message>();
        }

        /// <summary>
        /// 数据库工作单元
        /// </summary>
        public IUnitOfWork UnitOfWork { get; }
        
        /// <summary>
        /// 添加到缓冲区
        /// </summary>
        public void AddToBuffer(Message message)
        {
            _bufferList.Enqueue(message);
        }

        /// <summary>
        /// 提交事物
        /// </summary>
        public void Commit()
        {
            UnitOfWork?.Commit();

            Flush();
        }

        /// <summary>
        /// 异步提交事物
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await UnitOfWork?.CommitAsync(cancellationToken)!;
            
            Flush();
        }
        
        /// <summary>
        /// 清空缓冲数据
        /// </summary>
        protected virtual void Flush()
        {
            while (!_bufferList.IsEmpty)
            {
                if (_bufferList.TryDequeue(out var message))
                {
                    _dispatcher.EnqueueToPublish(message).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _bufferList?.Clear();
        }
    }
}