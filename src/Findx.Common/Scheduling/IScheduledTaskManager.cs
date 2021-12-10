using System;
using System.Threading.Tasks;

namespace Findx.Scheduling
{
    /// <summary>
    /// 定义一个调度任务管理器
    /// </summary>
    public interface IScheduledTaskManager
    {
        /// <summary>
        /// 调度一个延时执行任务
        /// </summary>
        /// <typeparam name="TTaskHandler"></typeparam>
        /// <param name="taskArgs"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        Task<string> EnqueueAsync<TTaskHandler>(object taskArgs, TimeSpan? delay = null) where TTaskHandler : IScheduledTask;
        /// <summary>
        /// 调度一个延时执行任务
        /// </summary>
        /// <typeparam name="TTaskHandler"></typeparam>
        /// <param name="taskArgs"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        Task<string> EnqueueAsync<TTaskHandler>(object taskArgs, DateTime? dateTime = null) where TTaskHandler : IScheduledTask;
        /// <summary>
        /// 调度一个循环执行任务
        /// </summary>
        /// <typeparam name="TTaskHandler"></typeparam>
        /// <param name="taskArgs"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        Task<string> ScheduleAsync<TTaskHandler>(object taskArgs, TimeSpan delay) where TTaskHandler : IScheduledTask;
        /// <summary>
        /// 调度一个循环执行任务
        /// </summary>
        /// <typeparam name="TTaskHandler"></typeparam>
        /// <param name="taskArgs"></param>
        /// <param name="cronExpression"></param>
        /// <returns></returns>
        Task<string> ScheduleAsync<TTaskHandler>(object taskArgs, string cronExpression) where TTaskHandler : IScheduledTask;
        /// <summary>
        /// 调度一个循环执行任务
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        Task<string> ScheduleAsync(SchedulerTaskWrapper wrapper, TimeSpan delay);
        /// <summary>
        /// 调度一个循环执行任务
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="cronExpression"></param>
        /// <returns></returns>
        Task<string> ScheduleAsync(SchedulerTaskWrapper wrapper, string cronExpression);
        /// <summary>
        /// 调度一个循环执行任务
        /// </summary>
        /// <param name="wrapper"></param>
        /// <returns></returns>
        Task<string> ScheduleAsync(SchedulerTaskWrapper wrapper);
    }
}
