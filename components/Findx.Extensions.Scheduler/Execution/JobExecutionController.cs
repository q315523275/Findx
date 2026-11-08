using System.Diagnostics;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Scheduling;
using Findx.Jobs.Client;
using Findx.Jobs;
using Findx.Redis;
using Findx.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     执行端接口：接收 Master 分发的任务并执行 IJob，执行完回调 Master 结果
/// </summary>
[Area("findx")]
[Route("api/scheduler/jobs")]
[ApiExplorerSettings(GroupName = "scheduler"), Description("调度任务执行")]
public class JobExecutionController : AreaApiControllerBase
{
    private readonly IOptions<SchedulerOptions> _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRedisClientProvider _redisClientProvider;

    /// <summary>
    ///     Ctor
    /// </summary>
    public JobExecutionController(IOptions<SchedulerOptions> options, IHttpClientFactory httpClientFactory,
        IRedisClientProvider redisClientProvider)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _redisClientProvider = redisClientProvider;
    }

    /// <summary>
    ///     接收 Master 分发的任务并执行
    /// </summary>
    [HttpPost("execute")]
    [Description("执行任务")]
    public async Task<CommonResult> ExecuteAsync([FromBody] DispatchRequest request, CancellationToken cancellationToken)
    {
        // 机器间接口共享令牌校验（配置 ClusterToken 后启用）
        if (!TryAuthorize(out var authResult)) return authResult;

        if (request == null || string.IsNullOrWhiteSpace(request.HandlerType))
            return CommonResult.Fail("400", "请求参数缺失");

        var watch = Stopwatch.StartNew();
        var jobExecuteInfo = new JobExecuteInfo
        {
            Id = SnowflakeIdUtility.Default().NextId(),
            JobId = request.JobId,
            Name = request.JobName,
            Parameter = request.Parameter,
            FullName = request.HandlerType,
            RunTime = DateTimeOffset.Now
        };

        var context = new JobExecuteContext(HttpContext.RequestServices, jobExecuteInfo);
        var executor = HttpContext.RequestServices.GetRequiredService<IJobExecutor>();

        bool success;
        string error;
        string message;
        try
        {
            await executor.ExecuteAsync(context, cancellationToken);
            success = context.JobResult.IsSuccess && !context.JobResult.IsCancelled;
            error = context.JobResult.Error?.Message;
            message = context.JobResult.Message;
        }
        catch (Exception ex)
        {
            success = false;
            error = ex.Message;
            message = ex.ToString();
        }

        watch.Stop();

        // 回调 Master 结果：优先解析 Redis 中「当前 Master」地址（故障转移安全），失败回退分发时携带的地址
        var masterAddress = await ResolveMasterAddressAsync(request.MasterAddress, cancellationToken);
        if (!string.IsNullOrWhiteSpace(masterAddress))
        {
            var report = new JobResultReport
            {
                TraceId = request.TraceId,
                JobId = request.JobId,
                JobName = request.JobName,
                IsSuccess = success,
                Error = error,
                Message = message,
                ElapsedMs = watch.ElapsedMilliseconds,
                WorkerAddress = _options.Value.NodeAddress
            };

            // 回调失败有限重试（500ms/1s/2s），避免单次网络抖动直接丢结果；最终失败由 Master 侧超时兜底接管
            await PostResultWithRetryAsync(masterAddress, report, cancellationToken);
        }
        else
        {
            Logger.LogError("任务 [{JobName}] TraceId={TraceId} 无可用 Master 回调地址（MasterInfo 读取失败且分发地址为空）",
                request.JobName, request.TraceId);
        }

        return CommonResult.Success();
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
            Logger.LogWarning(ex, "读取 MasterInfo 失败，回退使用分发时携带的 Master 地址");
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
                var response = await httpClient.PostAsJsonAsync($"{masterAddress}/api/scheduler/jobs/result", report, headers, cancellationToken);
                if (response.IsSuccessStatusCode) return;

                Logger.LogWarning("回调 Master {Address} HTTP {Status}，第 {Attempt}/3 次重试", masterAddress, (int)response.StatusCode, attempt + 1);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "回调 Master {Address} 异常，第 {Attempt}/3 次重试", masterAddress, attempt + 1);
            }

            if (attempt < 2) await Task.Delay(TimeSpan.FromMilliseconds(500 * (attempt + 1)), cancellationToken);
        }

        Logger.LogError("任务 [{JobName}] TraceId={TraceId} 回调 Master 失败，已重试 3 次放弃（Master 侧超时兜底将按失败处理）",
            report.JobName, report.TraceId);
    }

    /// <summary>
    ///     共享令牌校验
    /// </summary>
    private bool TryAuthorize(out CommonResult result)
    {
        result = null;
        var token = _options.Value.ClusterToken;
        if (string.IsNullOrWhiteSpace(token)) return true;

        var headerToken = HttpContext.Request.Headers["X-Scheduler-Token"].FirstOrDefault();
        if (!string.Equals(headerToken, token, StringComparison.Ordinal))
        {
            result = CommonResult.Fail("auth.fail", "调度令牌无效");
            return false;
        }

        return true;
    }
}
