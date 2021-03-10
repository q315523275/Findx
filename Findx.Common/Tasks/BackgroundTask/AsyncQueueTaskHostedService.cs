using Findx.Extensions;
using Findx.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Tasks.BackgroundTask
{
    /// <summary>
    /// 异步排队任务后台服务
    /// </summary>
    public class AsyncQueueTaskHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly StartupLogger _startupLogger;
        public AsyncQueueTaskHostedService(IBackgroundTaskQueue taskQueue, IServiceProvider serviceProvider)
        {
            TaskQueue = taskQueue;
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetLogger<AsyncQueueTaskHostedService>();
            _startupLogger = _serviceProvider.GetService<StartupLogger>();
        }
        public IBackgroundTaskQueue TaskQueue { get; }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _startupLogger.LogInformation("异步排队任务启动...", "QueuedTaskHostedService");

            await BackgroundProcessing(stoppingToken);
        }
        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(stoppingToken);
                if (workItem != null)
                {
                    try
                    {
                        await workItem(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"异步排队任务{nameof(workItem)}");
                    }
                }
                else
                {
                    await Task.Delay(500);
                }
            }
        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("异步排队任务停止...");

            await base.StopAsync(stoppingToken);
        }
    }
}
