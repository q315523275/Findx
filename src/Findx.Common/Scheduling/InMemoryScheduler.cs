using Findx.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace Findx.Scheduling
{
    /// <summary>
    /// 内存调度器
    /// </summary>
    public class InMemoryScheduler : IScheduler
    {
        private readonly IScheduledTaskStore _storage;
        private readonly IScheduledTaskDispatcher _dispatcher;
        private readonly ILogger<InMemoryScheduler> _logger;

        private SchedulerOptions _options;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="dispatcher"></param>
        /// <param name="optionsMonitor"></param>
        /// <param name="logger"></param>
        public InMemoryScheduler(IScheduledTaskStore storage, IScheduledTaskDispatcher dispatcher, IOptionsMonitor<SchedulerOptions> optionsMonitor, ILogger<InMemoryScheduler> logger)
        {
            _storage = storage;
            _dispatcher = dispatcher;
            _logger = logger;

            optionsMonitor.OnChange(ConfigurationOnChange);
            _options = optionsMonitor.CurrentValue;
        }

        /// <summary>
        /// 配置发生变更
        /// </summary>
        /// <param name="changeOptions"></param>
        private void ConfigurationOnChange(SchedulerOptions changeOptions)
        {
            if (changeOptions.ToString() != _options.ToString())
            {
                _options = changeOptions;

                _logger.LogInformation($"InMemoryScheduler调度器发生了配置变更,内容{_options}");
            }
        }

        /// <summary>
        /// 启动调度
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return Task.CompletedTask;

            Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested && _options.Enabled)
                {
                    await Task.Delay(_options.ScheduleMillisecondsDelay);
                    await ExecuteOnceAsync(cancellationToken);
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 停止调度
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 调度执行
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            var tasksThatShouldRun = await _storage.GetShouldRunTasksAsync(_options.MaxJobFetchCount);

            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                var executeInfo = new SchedulerTaskExecuteInfo
                {
                    ExecuteId = Guid.NewGuid(),
                    Status = 0,
                    TaskArgs = taskThatShouldRun.TaskArgs,
                    TaskFullName = taskThatShouldRun.TaskFullName,
                    TaskId = taskThatShouldRun.Id,
                    TaskName = taskThatShouldRun.TaskName,
                    TaskTime = taskThatShouldRun.NextRunTime.Value,
                };

                // 固定时间执行任务直接计算下次执行时间
                if (!taskThatShouldRun.CronExpress.IsNullOrWhiteSpace())
                    taskThatShouldRun.Increment();

                // 固定间隔任务，从推送开始标识执行中
                if (taskThatShouldRun.FixedDelay > 0)
                    taskThatShouldRun.IsRuning = true;

                await _dispatcher.EnqueueToPublishAsync(executeInfo, cancellationToken);
            }
        }
    }
}
