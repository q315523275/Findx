using System.Threading.Tasks;

namespace Findx.Jobs;

/// <summary>
///     定义一个触发器
/// </summary>
public interface ITriggerListener
{
	/// <summary>
	///     任务触发
	/// </summary>
	/// <param name="jobInfo"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task TriggerFiredAsync(JobInfo jobInfo, CancellationToken cancellationToken = default);
}