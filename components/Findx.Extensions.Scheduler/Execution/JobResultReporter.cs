using System.Collections.Generic;
using Findx.Extensions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Findx.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     执行结果上报器：解析当前 Master 地址并把 JobResultReport 回传。
///     <para>从 Controller 抽出，供执行线程（而非 HTTP 请求线程）异步调用。</para>
/// </summary>
public class JobResultReporter
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRedisClientProvider _redisClientProvider;
    private readonly ILogger<JobResultReporter> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public JobResultReporter(IOptions<SchedulerOptions> options, IHttpClientFactory httpClientFactory,
        IRedisClientProvider redisClientProvider, ILogger<JobResultReporter> logger)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _redisClientProvider = redisClientProvider;
        _logger = logger;
    }

    /// <summary>
    ///     上报一次执行结果（含 Master 地址解析与有限重试）
    /// </summary>
    public async Task ReportAsync(DispatchRequest request, bool success, string error, string message,
        long elapsedMs, CancellationToken cancellationToken = default)
    {
        if (request == null) return;

        var masterAddress = await ResolveMasterAddressAsync(request.MasterAddress, cancellationToken);
        if (string.IsNullOrWhiteSpace(masterAddress))
        {
            _logger.LogError("任务 [{JobName}] TraceId={TraceId} 无可用 Master 回调地址（MasterInfo 读取失败且分发地址为空）",
                request.JobName, request.TraceId);
            return;
        }

        var report = new JobResultReport
        {
            TraceId = request.TraceId,
            JobId = request.JobId,
            JobName = request.JobName,
            IsSuccess = success,
            Error = error,
            Message = message,
            ElapsedMs = elapsedMs,
            WorkerAddress = _options.Value.NodeAddress
        };

        // 回调失败有限重试（500ms/1s/2s），避免单次网络抖动直接丢结果；最终失败由 Master 侧超时兜底接管
        await PostResultWithRetryAsync(masterAddress, report, cancellationToken);
    }

    /// <summary>
    ///     解析当前 Master 回调地址：读 Redis MasterInfoKey 拿最新 Master（故障转移后旧分发包地址已失效），失败回退分发地址
    /// </summary>
    private async Task<string> ResolveMasterAddressAsync(string fallback, CancellationToken cancellationToken)
    {
        try
        {
            var redis = _redisClientProvider.CreateClient();
            var json = await redis.StringGetAsync(_options.Value.MasterInfoKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var info = json.ToObject<SchedulerMasterInfo>();
                if (info != null && !string.IsNullOrWhiteSpace(info.Address))
                    return info.Address.TrimEnd('/');
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "读取 MasterInfo 失败，回退使用分发时携带的 Master 地址");
        }

        return fallback?.TrimEnd('/');
    }

    /// <summary>
    ///     结果回调：最多 3 次（指数退避 500ms/1s/2s），仍失败则记错误（Master 侧 ExecutionTimeoutMinutes 超时兜底会接管）
    /// </summary>
    private async Task PostResultWithRetryAsync(string masterAddress, JobResultReport report, CancellationToken cancellationToken)
    {
        var opts = _options.Value;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                Dictionary<string, string> headers = null;
                if (!string.IsNullOrWhiteSpace(opts.ClusterToken))
                    headers = new Dictionary<string, string> { ["X-Scheduler-Token"] = opts.ClusterToken };

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, opts.DispatchHttpTimeoutSeconds));
                var response = await httpClient.PostAsJsonAsync($"{masterAddress}/api/scheduler/jobs/result", report, headers, cancellationToken);
                if (response.IsSuccessStatusCode) return;

                _logger.LogWarning("回调 Master {Address} HTTP {Status}，第 {Attempt}/3 次重试", masterAddress, (int)response.StatusCode, attempt + 1);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "回调 Master {Address} 异常，第 {Attempt}/3 次重试", masterAddress, attempt + 1);
            }

            if (attempt < 2) await Task.Delay(TimeSpan.FromMilliseconds(500 * (attempt + 1)), cancellationToken);
        }

        _logger.LogError("任务 [{JobName}] TraceId={TraceId} 回调 Master 失败，已重试 3 次放弃（Master 侧超时兜底将按失败处理）",
            report.JobName, report.TraceId);
    }
}
