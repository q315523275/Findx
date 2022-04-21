using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Findx.Jobs
{
    /// <summary>
    /// 定义一个触发器
    /// </summary>
    public interface ITriggerListener
	{
        /// <summary>
        /// 任务触发
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <returns></returns>
        Task TriggerFiredAsync([NotNull] JobInfo jobInfo, CancellationToken cancellationToken = default);
	}
}

