using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Jobs
{
	/// <summary>
    /// 定义一个工作调度器
    /// </summary>
	public interface IJobScheduler
	{
        /// <summary>
        /// 调度一个延时执行任务
        /// </summary>
        /// <typeparam name="TJob"></typeparam>
        /// <param name="jobArgs"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        Task<long> EnqueueAsync<TJob>(object jobArgs, TimeSpan? delay = null) where TJob : IJob;

        /// <summary>
        /// 调度一个延时执行任务
        /// </summary>
        /// <typeparam name="TJob"></typeparam>
        /// <param name="jobArgs"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        Task<long> EnqueueAsync<TJob>(object jobArgs, DateTime? dateTime = null) where TJob : IJob;

        /// <summary>
        /// 调度一个循环执行任务
        /// </summary>
        /// <typeparam name="TTaskHandler"></typeparam>
        /// <param name="taskArgs"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        Task<long> ScheduleAsync<TJob>(object jobArgs, TimeSpan delay) where TJob : IJob;

        /// <summary>
        /// 调度一个循环执行任务
        /// </summary>
        /// <typeparam name="TTaskHandler"></typeparam>
        /// <param name="taskArgs"></param>
        /// <param name="cronExpression"></param>
        /// <returns></returns>
        Task<long> ScheduleAsync<TJob>(object jobArgs, string cronExpression) where TJob : IJob;

        /// <summary>
        /// 调度一个循环执行任务
        /// </summary>
        /// <param name="jobType"></param>
        /// <returns></returns>
        Task<long> ScheduleAsync(Type jobType);

        /// <summary>
        /// 开始服务
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 停止服务
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}

