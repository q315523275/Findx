using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     <see cref="ISchedulerMasterChannel" /> 的 HTTP 实现。
///     <para>本类只存在于执行端进程：不注入任何仓储 / 数据库原语，只做 HTTP 出站。
///     执行端与调度端同进程时，若未配置 MasterEndpoints，则回环到本机 NodeAddress 走同一套协议，
///     避免「同进程就直接写库」这种绕过协议的特例。</para>
/// </summary>
public class HttpSchedulerMasterChannel : ISchedulerMasterChannel
{
    private const string BeatPath = "/api/scheduler/workers/beat";
    private const string OfflinePath = "/api/scheduler/workers/offline";
    private const string ResultPath = "/api/scheduler/jobs/result";
    private const string ResultsPath = "/api/scheduler/jobs/results";
    private const string LogsPath = "/api/scheduler/jobs/logs";

    private readonly IOptions<SchedulerOptions> _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpSchedulerMasterChannel> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public HttpSchedulerMasterChannel(IOptions<SchedulerOptions> options, IHttpClientFactory httpClientFactory,
        ILogger<HttpSchedulerMasterChannel> logger)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Protocol => "http";

    /// <inheritdoc />
    public Task<bool> HeartbeatAsync(WorkerNodeInfo info, CancellationToken cancellationToken = default)
    {
        //  心跳只做一轮（每个候选地址一次）：漏一拍由下一拍补上，不在心跳里叠加退避
        return PostAsync(BeatPath, info, null, 1, TimeSpan.Zero, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> OfflineAsync(WorkerNodeInfo info, CancellationToken cancellationToken = default)
    {
        //  停机通知：调用方通常在 Stopping 阶段，用较短超时与单次尝试，失败也不阻塞停机
        return PostAsync(OfflinePath, info, null, 1, TimeSpan.Zero, CancellationToken.None);
    }

    /// <inheritdoc />
    public Task<bool> ReportResultAsync(JobResultReport report, string preferredMasterAddress,
        CancellationToken cancellationToken = default)
    {
        //  结果回调是闭环关键：单地址最多 3 次（500ms / 1s 退避），全失败交由调度端账本超时兜底
        return PostAsync(ResultPath, report, preferredMasterAddress, 3, TimeSpan.FromMilliseconds(500), cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ReportResultsAsync(string preferredMasterAddress, IReadOnlyList<JobResultReport> reports,
        CancellationToken cancellationToken = default)
    {
        //  批量结果同样是闭环关键：单地址最多 3 次（500ms / 1s 退避）
        return PostAsync(ResultsPath, reports, preferredMasterAddress, 3, TimeSpan.FromMilliseconds(500), cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ShipLogsAsync(string preferredMasterAddress, IReadOnlyList<ExecutionLogChunk> chunks,
        CancellationToken cancellationToken = default)
    {
        //  日志是旁证：单地址最多 2 次（300ms 退避），失败只由调用方计数，不阻塞执行
        return PostAsync(LogsPath, chunks, preferredMasterAddress, 2, TimeSpan.FromMilliseconds(300), cancellationToken);
    }

    /// <summary>
    ///     统一出站：按候选地址顺序尝试，单个地址内做有限重试
    /// </summary>
    private async Task<bool> PostAsync<TBody>(string path, TBody body, string preferredMasterAddress, int attempts,
        TimeSpan retryDelay, CancellationToken cancellationToken)
    {
        var addresses = ResolveAddresses(preferredMasterAddress);
        if (addresses.Count == 0)
        {
            _logger.LogWarning("调度端地址为空：未配置 MasterEndpoints，且本机未同时启用调度端（path={Path}）", path);
            return false;
        }

        var opts = _options.Value;
        Dictionary<string, string> headers = null;
        if (!string.IsNullOrWhiteSpace(opts.ClusterToken))
            headers = new Dictionary<string, string> { ["X-Scheduler-Token"] = opts.ClusterToken };

        foreach (var address in addresses)
        {
            for (var attempt = 0; attempt < attempts; attempt++)
            {
                try
                {
                    //  不 Dispose 工厂客户端：Handler 由工厂复用，Dispose 会破坏连接池
                    var client = _httpClientFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(1, opts.DispatchHttpTimeoutSeconds));
                    var response = await client.PostAsJsonAsync($"{address}{path}", body, headers, cancellationToken);
                    if (response.IsSuccessStatusCode) return true;

                    _logger.LogWarning("上报调度端 {Address} {Path} 失败，HTTP {Status}，第 {Attempt}/{Attempts} 次",
                        address, path, (int)response.StatusCode, attempt + 1, attempts);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "上报调度端 {Address} {Path} 异常，第 {Attempt}/{Attempts} 次",
                        address, path, attempt + 1, attempts);
                }

                if (attempt < attempts - 1 && retryDelay > TimeSpan.Zero)
                    await Task.Delay(retryDelay * (attempt + 1), cancellationToken);
            }
        }

        return false;
    }

    /// <summary>
    ///     候选调度端地址：分发包优先，其次配置的 MasterEndpoints；
    ///     两者都没有但本机同时启用了调度端时，回环到 NodeAddress（仍然走协议，不直接写库）
    /// </summary>
    private List<string> ResolveAddresses(string preferredMasterAddress)
    {
        var opts = _options.Value;
        var result = new List<string>(4);

        void Add(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return;
            var normalized = address.TrimEnd('/');
            if (!result.Contains(normalized)) result.Add(normalized);
        }

        Add(preferredMasterAddress);
        foreach (var endpoint in opts.MasterEndpoints ?? Array.Empty<string>()) Add(endpoint);

        if (result.Count == 0 && opts.SchedulerEnabled) Add(opts.NodeAddress);

        return result;
    }
}
