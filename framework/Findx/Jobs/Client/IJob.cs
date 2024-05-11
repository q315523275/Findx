using System.Threading.Tasks;
using Findx.Jobs.Common;

namespace Findx.Jobs.Client;

/// <summary>
///     定义一个作业
/// </summary>
public interface IJob
{
	/// <summary>
	///     作业执行
	/// </summary>
	/// <param name="context"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<JobResult> RunAsync(JobExecuteContext context, CancellationToken cancellationToken = default);
}