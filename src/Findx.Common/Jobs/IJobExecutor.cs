using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Findx.Jobs
{
    /// <summary>
    /// 定义一个作业执行器
    /// </summary>
    public interface IJobExecutor
	{
		/// <summary>
		/// 执行作业
		/// </summary>
		/// <param name="context"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task RunAsync([NotNull] IJobContext context, CancellationToken cancellationToken = default);
	}
}

