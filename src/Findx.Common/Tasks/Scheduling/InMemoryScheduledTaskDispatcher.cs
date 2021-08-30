using Findx.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Tasks.Scheduling
{
    internal class InMemoryScheduledTaskDispatcher : IScheduledTaskDispatcher
    {
        private readonly IMessageNotifySender sender;

        public InMemoryScheduledTaskDispatcher(IMessageNotifySender sender)
        {
            this.sender = sender;
        }

        /// <summary>
        /// 升级分布式调度时使用方法
        /// </summary>
        /// <param name="executeInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task EnqueueToExecuteAsync(SchedulerTaskExecuteInfo executeInfo, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 推送执行任务
        /// </summary>
        /// <param name="executeInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task EnqueueToPublishAsync(SchedulerTaskExecuteInfo executeInfo, CancellationToken cancellationToken = default)
        {
            // 先简易使用线程消息触发任务执行

            return sender.PublishAsync(new ScheduledTaskCommand(executeInfo), cancellationToken);
        }
    }

    internal class ScheduledTaskCommand : IMessageNotify
    {
        public ScheduledTaskCommand(SchedulerTaskExecuteInfo schedulerTaskExecuteInfo)
        {
            Check.NotNull(schedulerTaskExecuteInfo, nameof(schedulerTaskExecuteInfo));

            SchedulerTaskExecuteInfo = schedulerTaskExecuteInfo;
        }

        public SchedulerTaskExecuteInfo SchedulerTaskExecuteInfo { get; }
    }
    internal class ScheduledTaskCommandHandler : IMessageNotifyHandler<ScheduledTaskCommand>
    {
        private readonly IScheduledTaskExecuter _executer;
        private readonly IServiceProvider _provider;

        public ScheduledTaskCommandHandler(IScheduledTaskExecuter executer, IServiceProvider provider)
        {
            _executer = executer;
            _provider = provider;
        }

        public async Task Handle(ScheduledTaskCommand message, CancellationToken cancellationToken = default)
        {
            var context = new TaskExecutionContext(_provider, message.SchedulerTaskExecuteInfo);

            await _executer.ExecuteAsync(context, cancellationToken);
        }
    }
}
