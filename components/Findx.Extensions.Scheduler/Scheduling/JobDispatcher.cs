using System.Net;
using System.Net.Http.Json;
using Findx.Data;
using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     任务分发器：把已领取的执行行投递到具备该能力的 Worker。
///     <para>只做投递与受理确认：受理成功把执行行标为 Dispatched；受理失败原样返回结果，
///     由调用方决定是否进入重试。不持有任何锁——执行权已经由执行行唯一键与任务行 CAS 决定。</para>
///     <para>投递只做逐条 HTTP：Worker「接收即返回」，执行结果仍由 Worker 逐条回调，两条链路互不阻塞。
///     同一波次的并发度由调用方（分发队列）的全局在飞闸门控制，不再维护批量端点。</para>
/// </summary>
public class JobDispatcher
{
    private readonly IOptions<SchedulerOptions> _options;

    /// <summary>
    ///     投递专用 HttpClient：整个进程只建一次并复用连接池。
    ///     <para>投递是本组件最热的出站调用。此前每次投递都 CreateClient() 新建实例，
    ///     每次都落在全新连接池上（新 TCP 握手 + 不复用 keep-alive），把单次往返从毫秒级抬到几十毫秒，
    ///     直接成为分发吞吐的上限。改为单例复用后，单次往返只受 Worker 处理耗时约束。</para>
    /// </summary>
    private readonly HttpClient _httpClient;

    private readonly WorkerRegistry _workerRegistry;
    private readonly IDispatchStrategy _dispatchStrategy;
    private readonly IExecutionLedger _ledger;
    private readonly IClusterClock _clock;
    private readonly ILogger<JobDispatcher> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="httpClientFactory"></param>
    /// <param name="workerRegistry"></param>
    /// <param name="dispatchStrategy"></param>
    /// <param name="ledger"></param>
    /// <param name="clock"></param>
    /// <param name="logger"></param>
    public JobDispatcher(IOptions<SchedulerOptions> options, IHttpClientFactory httpClientFactory, WorkerRegistry workerRegistry
        , IDispatchStrategy dispatchStrategy, IExecutionLedger ledger, IClusterClock clock, ILogger<JobDispatcher> logger)
    {
        _options = options;
        //  只在构造时取一次：连接池随实例复用（之后每个请求都是 keep-alive 复用连接）
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.Value.DispatchHttpTimeoutSeconds));
        _workerRegistry = workerRegistry;
        _dispatchStrategy = dispatchStrategy;
        _ledger = ledger;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    ///     是否存在可执行该任务的节点（领取前预检，避免无节点时反复产生执行流水）
    /// </summary>
    /// <param name="jobName"></param>
    /// <param name="handlerType"></param>
    /// <returns></returns>
    public bool HasCandidates(string jobName, string handlerType)
    {
        return _workerRegistry.GetCandidates(jobName, handlerType).Count > 0;
    }

    /// <summary>
    ///     无节点原因的定性描述：区分「集群没有执行节点」与「有节点但都没注册这个任务」，
    ///     后者是部署/配置问题，需要在管理面与日志里能一眼看出
    /// </summary>
    /// <param name="jobName"></param>
    /// <param name="handlerType"></param>
    /// <returns></returns>
    public string NoWorkerReason(string jobName, string handlerType)
    {
        var alive = _workerRegistry.AliveWorkerCount;
        var key = string.IsNullOrWhiteSpace(handlerType) ? jobName : handlerType;

        return alive == 0 ? "分发失败：集群当前无存活执行节点" : $"分发失败：存活执行节点 {alive} 个，但均未注册任务 {key}";
    }

    /// <summary>
    ///     选点：按任务能力反查候选节点后套用分发策略。
    ///     <para>纯内存决策，不产生 I/O：批量投递需要先逐条选点再按节点聚合，选点不能阻塞。</para>
    /// </summary>
    /// <param name="jobName"></param>
    /// <param name="handlerType"></param>
    /// <returns>选中的节点；无候选或策略未选中返回 null</returns>
    public WorkerNodeInfo SelectWorker(string jobName, string handlerType)
    {
        var candidates = _workerRegistry.GetCandidates(jobName, handlerType);
        if (candidates.Count == 0) return null;

        return _dispatchStrategy.Select(candidates, jobName);
    }

    /// <summary>
    ///     分发一条已领取的执行：选点后投递
    /// </summary>
    /// <param name="execution">执行行（已含 TraceId / Period / 参数）</param>
    /// <param name="cancellationToken"></param>
    /// <returns>受理结果</returns>
    public async Task<DispatchAck> DispatchAsync(SchedulerExecution execution, CancellationToken cancellationToken = default)
    {
        if (execution == null) return DispatchAck.Rejected;

        //  反查该任务的可执行健康节点，再按分发策略选点（默认 RoundRobin）
        var worker = SelectWorker(execution.JobName, execution.HandlerType);
        if (worker == null)
        {
            _logger.LogWarning("任务 [{JobName}] {Reason}，本轮跳过", execution.JobName, NoWorkerReason(execution.JobName, execution.HandlerType));
            return DispatchAck.NoWorker;
        }

        return await DispatchToAsync(worker, execution, cancellationToken);
    }

    /// <summary>
    ///     投递到指定节点（批量回落、重试换点等需要固定目标的场景使用）
    /// </summary>
    /// <param name="worker">目标节点</param>
    /// <param name="execution">执行行</param>
    /// <param name="cancellationToken"></param>
    /// <returns>受理结果</returns>
    public async Task<DispatchAck> DispatchToAsync(WorkerNodeInfo worker, SchedulerExecution execution, CancellationToken cancellationToken = default)
    {
        if (worker == null || execution == null) return DispatchAck.NoWorker;

        var ack = await PostExecuteAsync(worker.Address, BuildRequest(execution), cancellationToken);
        if (ack != DispatchAck.Accepted) return ack;

        //  受理确认：Claimed -> Dispatched（CAS，失败说明该行已被超时兜底接管）
        try
        {
            var marked = await _ledger.MarkDispatchedAsync(execution.TraceId, worker.Address, _clock.Now, cancellationToken);
            if (!marked)
            {
                _logger.LogWarning("任务 [{JobName}] TraceId={TraceId} 已受理但状态已被其他流程终结，忽略本次确认", execution.JobName, execution.TraceId);
            }
            else
            {
                _logger.LogInformation("任务 [{JobName}] 第 {Attempt} 次尝试已分发至 {Worker}，TraceId={TraceId}", execution.JobName, execution.Attempt + 1, worker.Address, execution.TraceId);
            }
        }
        catch (Exception ex)
        {
            //  执行端已经入队，只是本地记账失败：不能当成未受理（否则会立即重投并重复执行），交给超时兜底
            _logger.LogError(ex, "任务 [{JobName}] TraceId={TraceId} 已受理但记账失败，交由超时兜底", execution.JobName, execution.TraceId);
        }

        return DispatchAck.Accepted;
    }

    /// <summary>
    ///     只投递、不记账：供「并发投递 + 一次批量记账」路径使用。
    ///     <para>并发投递时若每条都各自 UPDATE 一次，批量摊薄效果会被抵消；调用方应收齐全部受理项后
    ///     用一次 <see cref="IExecutionLedger.MarkDispatchedBatchAsync" /> 落库。</para>
    /// </summary>
    /// <param name="worker">目标节点</param>
    /// <param name="execution">执行行</param>
    /// <param name="cancellationToken"></param>
    /// <returns>受理结果</returns>
    public Task<DispatchAck> PostWithoutMarkAsync(WorkerNodeInfo worker, SchedulerExecution execution, CancellationToken cancellationToken = default)
    {
        if (worker == null || execution == null) return Task.FromResult(DispatchAck.NoWorker);
        return PostExecuteAsync(worker.Address, BuildRequest(execution), cancellationToken);
    }


    /// <summary>
    ///     单条投递：POST {address}/api/scheduler/jobs/execute
    /// </summary>
    private async Task<DispatchAck> PostExecuteAsync(string address, DispatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            //  只约束「投递」这一步：Worker 假死时按配置超时失败，不拖住整轮调度
            var response = await _httpClient.PostAsJsonAsync($"{address}/api/scheduler/jobs/execute", request, BuildHeaders(), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                //  4xx/5xx 属于明确拒绝：Worker 没接活，可以安全重投
                _logger.LogWarning("任务 [{JobName}] 分发到 {Address} 被拒绝，HTTP {Status}", request.JobName, address, (int)response.StatusCode);
                return DispatchAck.Rejected;
            }

            //  执行端用 200 + code 同时表达「受理」与「业务拒绝」（未启用执行端 / 队列满）：
            //  不识别 code 会把拒绝当成受理，任务静默滞留到超时兜底才重试
            var body = await response.Content.ReadFromJsonAsync<CommonResult>(cancellationToken: cancellationToken);
            if (body != null && !string.Equals(body.Code, "0000", StringComparison.Ordinal))
            {
                _logger.LogWarning("任务 [{JobName}] 分发到 {Address} 未受理，{Code} {Msg}", request.JobName, address, body.Code, body.Msg);
                return DispatchAck.Rejected;
            }

            return DispatchAck.Accepted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "任务 [{JobName}] 分发到 {Address} 异常", request.JobName, address);
            return DispatchAck.Unreachable;
        }
    }

    /// <summary>
    ///     构造分发请求体
    /// </summary>
    /// <param name="execution"></param>
    /// <returns></returns>
    private DispatchRequest BuildRequest(SchedulerExecution execution)
    {
        return new DispatchRequest
        {
            JobId = execution.JobId,
            JobName = execution.JobName,
            HandlerType = execution.HandlerType,
            Parameter = execution.Parameter,
            TraceId = execution.TraceId,
            Period = execution.Period,
            MasterAddress = _options.Value.NodeAddress?.TrimEnd('/')
        };
    }

    /// <summary>
    ///     机器间共享令牌头（未配置 ClusterToken 时为空）
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, string> BuildHeaders()
    {
        var token = _options.Value.ClusterToken;
        return string.IsNullOrWhiteSpace(token) ? null : new Dictionary<string, string> { ["X-Scheduler-Token"] = token };
    }
}
