//using Findx.Tasks.Scheduling;
//using System;
//using System.Threading;
//using System.Threading.Tasks;

//namespace WebApplication1.Scheduling
//{
//    [Scheduled(FixedDelay = 3, Name = "测试任务")]
//    public class TestScheduledTask : IScheduledTask
//    {
//        public async Task ExecuteAsync(ITaskContext context, CancellationToken cancellationToken = default)
//        {
//            await Task.Delay(30);
//            Console.WriteLine($"我是{context.TaskId}条测试任务========" + DateTime.Now + "==========" + Thread.CurrentThread.ManagedThreadId);
//        }
//    }
//}
