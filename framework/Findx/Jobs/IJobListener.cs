using System.Threading.Tasks;

namespace Findx.Jobs;

/// <summary>
///     定义一个工作监听器
/// </summary>
public interface IJobListener
{
	/// <summary>
	///     作业开始执行
	/// </summary>
	/// <param name="context"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task JobToRunAsync(IJobExecutionContext context, CancellationToken cancellationToken = default);
}