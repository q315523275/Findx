using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Tasks.Scheduling
{
    public class InMemoryScheduler : IScheduler, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IScheduledTaskStore _taskStore;
        private Timer _taskTimer;
        private bool _polling;
        private SchedulerOptions _options;

        public InMemoryScheduler(IServiceProvider serviceProvider, IScheduledTaskStore taskStore, IOptionsMonitor<SchedulerOptions> _optionsMonitor)
        {
            _serviceProvider = serviceProvider;
            _taskStore = taskStore;

            _options = _optionsMonitor.CurrentValue;
            _optionsMonitor.OnChange(ConfigurationOnChange);

        }
        private void ConfigurationOnChange(SchedulerOptions changeOptions)
        {
            if (changeOptions.ToString() != _options.ToString())
            {
                _options = changeOptions;
                // 重启任务调度器
                StopAsync().ConfigureAwait(false).GetAwaiter();
                Dispose();
                StartAsync();
            }
        }

        public void Dispose()
        {
            _taskTimer?.Dispose();
            _taskTimer = null;

            GC.SuppressFinalize(this);
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            var pollingInterval = _options.JobPollPeriod * 1000;

            _taskTimer = new Timer(async x =>
            {
                if (_polling)
                {
                    return;
                }

                _polling = true;
                await ExecuteOnceAsync(cancellationToken);
                _polling = false;

            }, null, pollingInterval, pollingInterval);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _taskTimer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            var tasksThatShouldRun = await _taskStore.GetShouldRunTasksAsync(_options.MaxJobFetchCount);

            var taskFactory = new TaskFactory(TaskScheduler.Current);

            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                taskThatShouldRun.IsRuning = true;

                _ = taskFactory.StartNew(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var taskLogger = scope.ServiceProvider.GetRequiredService<ILogger<InMemoryScheduler>>();
                    var context = new TaskExecutionContext(scope.ServiceProvider, taskThatShouldRun.Id);
                    try
                    {
                        var scheduledTaskExecuter = scope.ServiceProvider.GetRequiredService<IScheduledTaskExecuter>();

                        await scheduledTaskExecuter.ExecuteAsync(context, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        taskLogger.LogError(ex, $"调度任务:{taskThatShouldRun.Id},执行异常");
                    }
                }, cancellationToken)
                    .ContinueWith(it => { taskThatShouldRun.Increment(); }, TaskContinuationOptions.ExecuteSynchronously); // 任务同线程处理
            }
        }
    }
}
