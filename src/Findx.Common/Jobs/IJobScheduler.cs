using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

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
        Task<long> EnqueueAsync<TJob>(TimeSpan? delay = null, object jobArgs = null) where TJob : IJob;

        /// <summary>
        /// 调度一个延时执行任务
        /// </summary>
        /// <typeparam name="TJob"></typeparam>
        /// <param name="jobArgs"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        Task<long> EnqueueAsync<TJob>(DateTime? dateTime = null, object jobArgs = null) where TJob : IJob;

        /// <summary>
        /// 调度一个循环执行任务
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="jobArgs"></param>
        /// <typeparam name="TJob"></typeparam>
        /// <returns></returns>
        Task<long> ScheduleAsync<TJob>([NotNull] TimeSpan delay, object jobArgs = null) where TJob : IJob;

        /// <summary>
        /// 调度一个循环执行任务
        /// </summary>
        /// <param name="cronExpression"></param>
        /// <param name="jobArgs"></param>
        /// <typeparam name="TJob"></typeparam>
        /// <returns></returns>
        Task<long> ScheduleAsync<TJob>([NotNull] string cronExpression, object jobArgs = null) where TJob : IJob;

        /// <summary>
        /// 调度一个循环执行任务
        /// </summary>
        /// <param name="jobType"></param>
        /// <returns></returns>
        Task<long> ScheduleAsync([NotNull] Type jobType);

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task PauseJob(long id);
        
        /// <summary>
        /// 恢复暂停的任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task ResumeJob(long id);
        
        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task RemoveJob(long id);
	}
}

