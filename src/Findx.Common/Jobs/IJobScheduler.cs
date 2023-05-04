using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Findx.Jobs;

/// <summary>
///     定义一个工作调度器
/// </summary>
public interface IJobScheduler
{
	/// <summary>
	///     调度一个延时执行任务
	/// </summary>
	/// <typeparam name="TJob"></typeparam>
	/// <param name="parameter"></param>
	/// <param name="delay"></param>
	/// <returns></returns>
	Task<long> EnqueueAsync<TJob>(TimeSpan? delay = null, IDictionary<string, string> parameter = null)
        where TJob : IJob;

	/// <summary>
	///     调度一个延时执行任务
	/// </summary>
	/// <typeparam name="TJob"></typeparam>
	/// <param name="parameter"></param>
	/// <param name="dateTime"></param>
	/// <returns></returns>
	Task<long> EnqueueAsync<TJob>(DateTime? dateTime = null, IDictionary<string, string> parameter = null)
        where TJob : IJob;

	/// <summary>
	///     调度一个循环执行任务
	/// </summary>
	/// <param name="delay"></param>
	/// <param name="parameter"></param>
	/// <typeparam name="TJob"></typeparam>
	/// <returns></returns>
	Task<long> ScheduleAsync<TJob>([NotNull] TimeSpan delay, IDictionary<string, string> parameter = null)
        where TJob : IJob;

	/// <summary>
	///     调度一个循环执行任务
	/// </summary>
	/// <param name="cronExpression"></param>
	/// <param name="parameter"></param>
	/// <typeparam name="TJob"></typeparam>
	/// <returns></returns>
	Task<long> ScheduleAsync<TJob>([NotNull] string cronExpression, IDictionary<string, string> parameter = null)
        where TJob : IJob;

	/// <summary>
	///     调度一个循环执行任务
	/// </summary>
	/// <param name="jobType"></param>
	/// <returns></returns>
	Task<long> ScheduleAsync([NotNull] Type jobType);

	/// <summary>
	///     暂停任务
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	Task PauseJob(long id);

	/// <summary>
	///     恢复暂停的任务
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	Task ResumeJob(long id);

	/// <summary>
	///     删除任务
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	Task RemoveJob(long id);
}