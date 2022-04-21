using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using JetBrains.Annotations;

namespace Findx.Jobs.Local
{
    public class InMemoryTriggerListener: ITriggerListener
	{
        private readonly IApplicationContext _applicationContext;

        public InMemoryTriggerListener(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public Task TriggerFiredAsync([NotNull] JobInfo jobInfo, CancellationToken cancellationToken = default)
        {
            // 分布式版本
            // 记录作业执行记录
            // 标记执行时间、推送情况、重试次数等

            return _applicationContext.PublishEventAsync(new LocalJobRequest { Detail = jobInfo }, cancellationToken);
        }
    }
}

