using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Findx.Jobs.Local
{
	public class InMemorySchedulerWorker: BackgroundService, IJobSchedulerWorker
	{
        private readonly IOptions<JobOptions> _options;

        private readonly IJobStorage _storage;

        private readonly ITriggerListener _trigger;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="storage"></param>
        /// <param name="trigger"></param>
        public InMemorySchedulerWorker(IOptions<JobOptions> options, IJobStorage storage, ITriggerListener trigger)
        {
            _options = options;
            _storage = storage;
            _trigger = trigger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(_options.Value.Delay), stoppingToken);

                await ExecuteOnceAsync(stoppingToken);
            }
        }

        /// <summary>
        /// 调度执行
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            var shouldRunJobs = await _storage.GetShouldRunJobsAsync(_options.Value.MaxFetchJobCount);

            foreach (var jobDetail in shouldRunJobs)
            {
                // 当前为内存存储版本，不宜记录过多
                // 直接传递使用作业对象
                // 暂时不使用作业分派记录

                // 内存作业派遣方式
                // 通过内存消息事件或者内存队列推送至执行侧

                // 分布式作业派遣方式
                // 作业派送器主要决定哪些作业需要执行、创建派遣记录
                // 作业触发监听器通过协议(rpc、http、websocket)等来进行作业执行节点通知
                // 作业触发监听器包含作业节点通知、负载、重试、故障转移、并行通知等服务

                // 作业监听器包含节点执行的方式方法，单节点串行执行控制
                // 作业执行者包含作业执行的参数构建等等


                // 当前使用最简单方式

                await _trigger.TriggerFiredAsync(jobDetail, cancellationToken);

                // 固定时间执行任务直接计算下次执行时间
                if (!jobDetail.CronExpress.IsNullOrWhiteSpace())
                    jobDetail.Increment();

                // 固定间隔任务，从推送开始标识执行中
                if (jobDetail.FixedDelay > 0)
                    jobDetail.IsRuning = true;
            }
        }
    }
}

