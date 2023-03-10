using System.Threading.Tasks;
using Findx.Extensions;
using JetBrains.Annotations;

namespace Findx.Jobs.Internal
{
    /// <summary>
    /// 内存版本触发监听器
    /// </summary>
    public class InMemoryTriggerListener: ITriggerListener
	{
        private readonly IApplicationContext _applicationContext;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="applicationContext"></param>
        public InMemoryTriggerListener(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        /// <summary>
        /// 触发任务执行
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task TriggerFiredAsync([NotNull] JobInfo jobInfo, CancellationToken cancellationToken = default)
        {
            // 分布式版本
            // 记录作业执行记录
            // 标记执行时间、推送情况、重试次数等

            return _applicationContext.PublishEventAsync(jobInfo, cancellationToken);
        }
    }
}

