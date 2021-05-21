using Findx.Data;
using Findx.DependencyInjection;
using Findx.Tasks.Scheduling;
using SqlSugar;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebHost.Scheduling
{
    [Scheduled(FixedDelay = 10, Name = "测试任务")]
    public class TestScheduledTask : IScheduledTask
    {
        public async Task ExecuteAsync(ITaskContext context, CancellationToken cancellationToken = default)
        {
            var unitOfWork222 = ServiceLocator.GetService<IUnitOfWork<SqlSugarClient>>();

            await Task.Delay(30);
            Console.WriteLine($"我是{context.TaskId}条测试任务========" + DateTime.Now + "==========" + Thread.CurrentThread.ManagedThreadId);
        }
    }
}
