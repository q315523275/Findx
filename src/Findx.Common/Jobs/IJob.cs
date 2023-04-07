using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Jobs
{
	/// <summary>
	/// 定义一个作业
	/// </summary>
	public interface IJob
	{
		/// <summary>
        /// 作业执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
		Task<JobResult> RunAsync(IJobExecutionContext context, CancellationToken cancellationToken = default);
	}
}

