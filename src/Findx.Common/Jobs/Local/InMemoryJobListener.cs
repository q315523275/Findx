using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Findx.Jobs.Local
{
    /// <summary>
    /// 内存工作任务监听
    /// </summary>
    public class InMemoryJobListener : IJobListener
    {
        private readonly ILogger<InMemoryJobListener> _logger;
        private readonly JobTypeDictionary _dict;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dict"></param>
        public InMemoryJobListener(ILogger<InMemoryJobListener> logger, JobTypeDictionary dict)
        {
            _logger = logger;
            _dict = dict;
        }

        /// <summary>
        /// 作业任务执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        public async Task JobToRunAsync([NotNull] IJobContext context, CancellationToken cancellationToken = default)
        {
            if (!_dict.TryGetValue(context.FullName, out var jobType))
                return;

            // TODO 当前为内存版本,直接执行任务
            
            // 分布式情况
            // 任务节点
            // 作业监听器决定作业是推队列、推线程池、直接执行
            // 作业执行器包括调用作业执行、异常捕获、重试策略、作业执行情况上报

            var job = context.ServiceProvider.GetService(jobType) as IJob;
            Check.NotNull(job, nameof(job));

            try
            {
                await job.RunAsync(context, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"作业 {context.JobName} 执行记录 {context.JobId} 执行失败");
            }
        }
    }
}
