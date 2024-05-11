using System.Threading.Tasks;

namespace Findx.Jobs.Client;

/// <summary>
///     定义一个作业执行器
/// </summary>
public interface IJobExecutor
{
	/// <summary>
	///     执行作业
	/// </summary>
	/// <param name="context"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task ExecuteAsync(JobExecuteContext context, CancellationToken cancellationToken = default);
}