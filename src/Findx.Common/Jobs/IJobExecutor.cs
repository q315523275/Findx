using System.Threading;
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
		/// <param name="jobDetail"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
		Task RunAsync([NotNull] IJobContext context, CancellationToken cancellationToken = default);
	}
}

