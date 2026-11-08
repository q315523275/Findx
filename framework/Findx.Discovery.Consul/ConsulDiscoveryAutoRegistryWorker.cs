using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Discovery.Consul;

public class ConsulDiscoveryAutoRegistryWorker : BackgroundService
{
    private static readonly TimeSpan MinRetryDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);

    private readonly IOptionsMonitor<DiscoveryOptions> _options;
    private readonly IConsulRegistration _registration;
    private readonly IConsulServiceRegistry _registry;
    private readonly ILogger<ConsulDiscoveryAutoRegistryWorker> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="registry"></param>
    /// <param name="registration"></param>
    /// <param name="logger"></param>
    public ConsulDiscoveryAutoRegistryWorker(IOptionsMonitor<DiscoveryOptions> options, IConsulServiceRegistry registry, IConsulRegistration registration, ILogger<ConsulDiscoveryAutoRegistryWorker> logger)
    {
        _options = options;
        _registry = registry;
        _registration = registration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.CurrentValue.Enabled || !_options.CurrentValue.Register)
            return;

        var retryDelay = MinRetryDelay;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _registry.RegisterAsync(_registration, stoppingToken).ConfigureAwait(false);

                _logger.LogInformation("服务 {ServiceName}/{InstanceId} 注册 Consul 成功", _registration.ServiceName, _registration.InstanceId);

                //  注册成功后按健康检查间隔周期重注册，作为 Consul 重启/会话过期后的保活手段
                var interval = TimeSpanUtility.ToTimeSpan(string.IsNullOrEmpty(_options.CurrentValue.HealthCheckInterval) ? "30s" : _options.CurrentValue.HealthCheckInterval);

                await Task.Delay(interval, stoppingToken).ConfigureAwait(false);

                retryDelay = MinRetryDelay;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                //  关闭信号，退出循环
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "服务注册 Consul 失败，{Delay}s 后重试", retryDelay.TotalSeconds);

                try
                {
                    await Task.Delay(retryDelay, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                //  指数退避，封顶 30s，避免 Consul 长时间不可用期间形成重试风暴
                retryDelay = TimeSpan.FromSeconds(Math.Min(retryDelay.TotalSeconds * 2, MaxRetryDelay.TotalSeconds));
            }
        }
    }
}
