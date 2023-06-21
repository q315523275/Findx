using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<long> EnqueueAsync<TJob>(TimeSpan? delay = null, IDictionary<string, string> parameter = null, CancellationToken cancellationToken = default) where TJob : IJob;

	/// <summary>
	///     调度一个延时执行任务
	/// </summary>
	/// <typeparam name="TJob"></typeparam>
	/// <param name="parameter"></param>
	/// <param name="dateTime"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<long> EnqueueAsync<TJob>(DateTime? dateTime = null, IDictionary<string, string> parameter = null, CancellationToken cancellationToken = default) where TJob : IJob;

	/// <summary>
	///     调度一个循环执行任务
	/// </summary>
	/// <param name="delay"></param>
	/// <param name="parameter"></param>
	/// <param name="cancellationToken"></param>
	/// <typeparam name="TJob"></typeparam>
	/// <returns></returns>
	Task<long> ScheduleAsync<TJob>(TimeSpan delay, IDictionary<string, string> parameter = null, CancellationToken cancellationToken = default) where TJob : IJob;

	/// <summary>
	///     调度一个循环执行任务
	/// </summary>
	/// <param name="cronExpression"></param>
	/// <param name="parameter"></param>
	/// <param name="cancellationToken"></param>
	/// <typeparam name="TJob"></typeparam>
	/// <returns></returns>
	Task<long> ScheduleAsync<TJob>([Required] string cronExpression, IDictionary<string, string> parameter = null, CancellationToken cancellationToken = default) where TJob : IJob;

	/// <summary>
	///     调度一个循环执行任务
	/// </summary>
	/// <param name="jobType"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<long> ScheduleAsync(Type jobType, CancellationToken cancellationToken = default);

	/// <summary>
	///     暂停任务
	/// </summary>
	/// <param name="id"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task PauseJob(long id, CancellationToken cancellationToken = default);

	/// <summary>
	///     恢复暂停的任务
	/// </summary>
	/// <param name="id"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task ResumeJob(long id, CancellationToken cancellationToken = default);

	/// <summary>
	///     删除任务
	/// </summary>
	/// <param name="id"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task RemoveJob(long id, CancellationToken cancellationToken = default);
}