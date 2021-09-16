using Findx.Tasks.Scheduling;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.WebHost.Scheduling
{
    [Scheduled(Cron = "* * * * * ?", Name = "测试任务4")]
    public class TestScheduledTask4 : IScheduledTask
    {
        public async Task ExecuteAsync(ITaskContext context, CancellationToken cancellationToken = default)
        {
            await Task.Delay(5);
            Console.WriteLine($"我是{context.TaskId}条测试任务========" + DateTime.Now + "==========" + Thread.CurrentThread.ManagedThreadId);
        }
    }
}
