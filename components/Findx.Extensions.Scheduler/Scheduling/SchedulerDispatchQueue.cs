using System.Threading.Channels;
using Findx.Extensions.Scheduler.Abstractions;
using Findx.Extensions.Scheduler.Models;
using Findx.Extensions.Scheduler.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     Master 分发队列：调度循环只做「查到期 + 入队」（纯内存操作），
///     「事务领取 + HTTP 投递 + 失败退避」全部由独立消费线程执行。
///     <para>目的：让到期扫描节奏不被单次网络往返/Worker 假死拖偏，任务越多、链路越慢越明显。</para>
/// </summary>
public class SchedulerDispatchQueue : IHostedService, IDisposable
{
    private readonly Channel<DispatchItem> _channel;
    private readonly List<Task> _consumers = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly IPeriodClaimer _claimer;
    private readonly JobDispatcher _dispatcher;
    private readonly JobResultHandler _resultHandler;
    private readonly IExecutionLedger _ledger;
    private readonly IClusterClock _clock;
    private readonly SchedulerJobBuffer _buffer;
    private readonly ILogger<SchedulerDispatchQueue> _logger;
    private readonly int _workerCount;
    private readonly int _claimBatchSize;
    private readonly int _maxInFlight;

    /// <summary>
    ///     全局在飞请求闸门：吞吐上限 ≈ 该值 / 单次往返耗时。
    ///     <para>跨消费线程共享，保证「并发度」由在飞请求数决定，而不是被固定线程数串行化。</para>
    /// </summary>
    private readonly SemaphoreSlim _inFlight;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="claimer"></param>
    /// <param name="dispatcher"></param>
    /// <param name="resultHandler"></param>
    /// <param name="ledger"></param>
    /// <param name="clock"></param>
    /// <param name="buffer"></param>
    /// <param name="logger"></param>
    public SchedulerDispatchQueue(IOptions<SchedulerOptions> options, IPeriodClaimer claimer, JobDispatcher dispatcher,
        JobResultHandler resultHandler, IExecutionLedger ledger, IClusterClock clock, SchedulerJobBuffer buffer,
        ILogger<SchedulerDispatchQueue> logger)
    {
        _claimer = claimer;
        _dispatcher = dispatcher;
        _resultHandler = resultHandler;
        _ledger = ledger;
        _clock = clock;
        _buffer = buffer;
        _logger = logger;
        _workerCount = options.Value.EffectiveDispatchWorkerCount;
        //  批量领取条数：1 表示逐条领取（旧行为）
        _claimBatchSize = Math.Clamp(options.Value.DispatchClaimBatchSize, 1, 500);
        //  在飞请求上限：并发投递的闸门，与线程数解耦
        _maxInFlight = options.Value.EffectiveDispatchMaxInFlight;
        _inFlight = new SemaphoreSlim(_maxInFlight, _maxInFlight);

        var capacity = Math.Max(64, options.Value.EffectiveDispatchQueueCapacity);
        //  有界通道：写满即背压，避免 Worker 集体变慢时把到期任务无限堆在内存里
        _channel = Channel.CreateBounded<DispatchItem>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = true
        });
    }

    /// <summary>
    ///     尝试入队。返回 false 表示队列已饱和（调用方降级为同步分发，任务不丢弃）
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool TryEnqueue(DispatchItem item) => _channel.Writer.TryWrite(item);

    /// <summary>
    ///     启动分发消费线程
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var reader = _channel.Reader;
        _consumers.Clear();
        for (var i = 0; i < _workerCount; i++)
        {
            _consumers.Add(Task.Run(() => ConsumeAsync(reader, _cts.Token), CancellationToken.None));
        }

        _logger.LogInformation("任务分发队列已启动，消费线程数={Count}，在飞请求上限={InFlight}", _workerCount, _maxInFlight);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     停止消费线程
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
#if NET8_0_OR_GREATER
        await _cts.CancelAsync();
#else
        _cts.Cancel();
#endif
        _channel.Writer.TryComplete();
        try
        {
            await Task.WhenAll(_consumers);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "等待任务分发消费线程退出时发生异常");
        }
    }

    /// <summary>
    ///     释放
    /// </summary>
    public void Dispose()
    {
        try
        {
            _cts.Cancel();
            _channel.Writer.TryComplete();
        }
        catch (ObjectDisposedException)
        {
            //  已释放或已停止，忽略
        }
        _cts.Dispose();
        _inFlight.Dispose();
    }

    /// <summary>
    ///     消费循环：从队列成批取到期条目 -> 一次事务批量领取 -> 逐条投递。
    ///     <para>批量领取把 N 条任务的事务提交摊薄成 1 次；投递仍按条执行，因为 HTTP 与 Worker 是逐条的。
    ///     无候选节点、CAS 未命中、单条投递异常都只影响该条，不拖累整批。</para>
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="cancellationToken"></param>
    private async Task ConsumeAsync(ChannelReader<DispatchItem> reader, CancellationToken cancellationToken)
    {
        try
        {
            while (await reader.WaitToReadAsync(cancellationToken))
            {
                var batch = new List<DispatchItem>(_claimBatchSize);
                while (batch.Count < _claimBatchSize && reader.TryRead(out var item))
                {
                    batch.Add(item);
                }

                if (batch.Count == 0) continue;
                await ProcessBatchAsync(batch, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            //  停止时正常退出
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "任务分发消费循环异常退出");
        }
    }

    /// <summary>
    ///     处理一批到期条目：预检节点 -> 批量领取 -> 逐条投递
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="cancellationToken"></param>
    private async Task ProcessBatchAsync(List<DispatchItem> batch, CancellationToken cancellationToken)
    {
        //  第一步：没有节点能接单的条目直接延后（不建执行行），不进入领取批次
        var claimable = new List<DispatchItem>(batch.Count);
        foreach (var item in batch)
        {
            if (cancellationToken.IsCancellationRequested) return;

            if (!_dispatcher.HasCandidates(item.Job.JobName, item.Job.HandlerType))
            {
                try
                {
                    await _resultHandler.DeferNoWorkerAsync(item, _dispatcher.NoWorkerReason(item.Job.JobName, item.Job.HandlerType), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "任务 [{JobName}] 无节点延后失败", item.Job?.JobName);
                }

                _buffer.Remove(item.Job?.Id ?? 0);
                continue;
            }

            claimable.Add(item);
        }

        if (claimable.Count == 0) return;

        //  第二步：一批共用同一个事务与一次提交
        var requests = new List<ClaimBatchRequest>(claimable.Count);
        foreach (var item in claimable)
        {
            requests.Add(new ClaimBatchRequest
            {
                Job = item.Job,
                Periods = item.Periods,
                Attempt = item.Attempt,
                Kind = item.Kind,
                ExpectedTime = item.ExpectedTime,
                NewNextRunTime = item.NewNextRunTime,
                LastRunTime = item.LastRunTime
            });
        }

        IReadOnlyList<ClaimBatchResult> results;
        try
        {
            results = await _claimer.TryClaimBatchAsync(requests, cancellationToken);
        }
        catch (Exception ex)
        {
            //  领取器内部已处理可预期的失败；走到这里说明是意外异常，整批剔除等下一轮重查
            _logger.LogError(ex, "批量领取异常，{Count} 条任务按未领取处理", claimable.Count);
            foreach (var item in claimable) _buffer.Remove(item.Job.Id);
            return;
        }

        if (results == null || results.Count != claimable.Count)
        {
            _logger.LogError("批量领取返回结果数量不匹配（请求 {Request}，结果 {Result}），整批按未领取处理",
                claimable.Count, results?.Count ?? 0);
            foreach (var item in claimable) _buffer.Remove(item.Job.Id);
            return;
        }

        //  第三步：逐条选点（纯内存）后按节点聚合，同一节点一波只发一次批量请求
        var plans = new List<DispatchPlan>(claimable.Count);
        for (var index = 0; index < claimable.Count; index++)
        {
            var item = claimable[index];
            var result = results[index];
            if (!result.Succeeded)
            {
                //  领取失败：该周期已被其他节点处理，剔除内存条目让后续重查校正
                _buffer.Remove(item.Job.Id);
                continue;
            }

            foreach (var execution in result.Executions)
            {
                plans.Add(new DispatchPlan(execution, _dispatcher.SelectWorker(execution.JobName, execution.HandlerType)));
            }
        }

        if (plans.Count == 0) return;

        //  按节点地址聚合（大小写不敏感：同一节点不能因为地址写法不同被拆成两组）
        var groups = new Dictionary<string, List<DispatchPlan>>(StringComparer.OrdinalIgnoreCase);
        var workers = new Dictionary<string, WorkerNodeInfo>(StringComparer.OrdinalIgnoreCase);
        var noWorkerPlans = new List<DispatchPlan>();
        foreach (var plan in plans)
        {
            if (plan.Worker == null)
            {
                //  预检后节点消失（心跳过期/下线）：逐条按无节点分流，不消耗重试次数
                noWorkerPlans.Add(plan);
                continue;
            }

            if (!groups.TryGetValue(plan.Worker.Address, out var list))
            {
                list = new List<DispatchPlan>();
                groups[plan.Worker.Address] = list;
                workers[plan.Worker.Address] = plan.Worker;
            }

            list.Add(plan);
        }

        var failures = new List<DispatchFailure>();

        foreach (var pair in groups)
        {
            var worker = workers[pair.Key];
            var group = pair.Value;

            //  组内并发投递：并发度由全局在飞闸门决定，不受消费线程数限制
            await DispatchConcurrentlyAsync(worker, group, failures, cancellationToken);
        }

        foreach (var plan in noWorkerPlans)
            failures.Add(new DispatchFailure(plan, DispatchAck.NoWorker));

        await HandleFailuresAsync(failures, cancellationToken);
    }

    /// <summary>
    ///     并发投递同一节点的整组执行行（逐条 HTTP）。
    ///     <para>并发度由全局在飞闸门限制：吞吐上限 ≈ 在飞上限 / 单次往返耗时，不再被「消费线程数 × 串行」卡住。
    ///     受理项先收集，最后用一次批量 UPDATE 置 Dispatched——并发投递不能同时逐条记账，
    ///     否则批量摊薄带来的 SQL 省量会被抵消掉。</para>
    /// </summary>
    /// <param name="worker">目标节点（组内同一地址）</param>
    /// <param name="group">该节点的执行行</param>
    /// <param name="failures">未受理结果收集</param>
    /// <param name="cancellationToken"></param>
    private async Task DispatchConcurrentlyAsync(WorkerNodeInfo worker, List<DispatchPlan> group,
        List<DispatchFailure> failures, CancellationToken cancellationToken)
    {
        if (group.Count == 0) return;

        var accepted = new List<SchedulerExecution>(group.Count);

        async Task DispatchOneAsync(DispatchPlan plan)
        {
            await _inFlight.WaitAsync(cancellationToken);
            try
            {
                DispatchAck ack;
                try
                {
                    ack = await _dispatcher.PostWithoutMarkAsync(worker, plan.Execution, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    //  单条异常只影响该条：执行行保持 Claimed，交给超时兜底或退避重试
                    _logger.LogError(ex, "分发异常，TraceId={TraceId}（节点 {Address}）", plan.Execution.TraceId, worker.Address);
                    ack = DispatchAck.Unreachable;
                }

                if (ack == DispatchAck.Accepted) lock (accepted) accepted.Add(plan.Execution);
                else lock (failures) failures.Add(new DispatchFailure(plan, ack));
            }
            finally
            {
                _inFlight.Release();
            }
        }

        var tasks = new Task[group.Count];
        for (var i = 0; i < group.Count; i++)
        {
            var plan = group[i];
            tasks[i] = DispatchOneAsync(plan);
        }

        await Task.WhenAll(tasks);

        if (accepted.Count == 0) return;

        var ids = new List<string>(accepted.Count);
        foreach (var execution in accepted) ids.Add(execution.TraceId);

        try
        {
            var affected = await _ledger.MarkDispatchedBatchAsync(ids, worker.Address, _clock.Now, cancellationToken);
            if (affected < ids.Count)
            {
                _logger.LogWarning("受理确认部分失效：{Affected}/{Total} 行仍为 Claimed（其余已被超时兜底或其他流程终结）", affected, ids.Count);
            }
            else
            {
                _logger.LogInformation("并发分发 {Count} 条至 {Worker}", ids.Count, worker.Address);
            }
        }
        catch (Exception ex)
        {
            //  执行端已经入队，只是本地记账失败：按已受理返回，交给超时兜底，绝不按未受理立即重投
            _logger.LogError(ex, "受理确认失败，{Count} 条已受理但未置 Dispatched，交由超时兜底（节点 {Address}）", ids.Count, worker.Address);
        }
    }

    /// <summary>
    ///     失败处理：逐条终结并按受理结果分流（无节点只延后重试，其余按失败计次）。
    ///     <para>逐条投递下每条都有独立受理结果，不再需要「同一任务同一批只计一次失败」的分组——
    ///     那套逻辑只服务于批量投递：一次整批不可达会把一个任务的重试预算一次性耗尽。</para>
    /// </summary>
    /// <param name="failures"></param>
    /// <param name="cancellationToken"></param>
    private async Task HandleFailuresAsync(List<DispatchFailure> failures, CancellationToken cancellationToken)
    {
        foreach (var failure in failures)
            await FinalizeAndFailAsync(failure.Plan.Execution, failure.Ack, cancellationToken);
    }


    /// <summary>
    ///     终结执行行并按受理结果分流：无节点只延后重试，其余按失败计次
    /// </summary>
    /// <param name="execution"></param>
    /// <param name="ack"></param>
    /// <param name="cancellationToken"></param>
    private async Task FinalizeAndFailAsync(SchedulerExecution execution, DispatchAck ack, CancellationToken cancellationToken)
    {
        var message = ack.ToMessage();
        await _ledger.TryFinalizeAsync(execution.TraceId, ExecutionState.Failed, message, _clock.Now, null, cancellationToken);

        if (ack == DispatchAck.NoWorker) await _resultHandler.HandleNoWorkerAsync(execution, cancellationToken);
        else await _resultHandler.HandleFailureAsync(execution, message, cancellationToken);
    }

    /// <summary>
    ///     投递计划：一条执行行 + 所属条目 + 选点结果。只在内存中流转，用于按节点聚合与失败归因。
    /// </summary>
    private sealed class DispatchPlan
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="execution"></param>
        /// <param name="worker"></param>
        /// <param name="seq"></param>
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="worker"></param>
        public DispatchPlan(SchedulerExecution execution, WorkerNodeInfo worker)
        {
            Execution = execution;
            Worker = worker;
        }

        /// <summary>
        ///     执行行
        /// </summary>
        public SchedulerExecution Execution { get; }

        /// <summary>
        ///     选点结果（null 表示无节点）
        /// </summary>
        public WorkerNodeInfo Worker { get; }
    }

    /// <summary>
    ///     未受理的执行行
    /// </summary>
    private sealed class DispatchFailure
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="ack"></param>
        public DispatchFailure(DispatchPlan plan, DispatchAck ack)
        {
            Plan = plan;
            Ack = ack;
        }

        /// <summary>
        ///     投递计划
        /// </summary>
        public DispatchPlan Plan { get; }

        /// <summary>
        ///     未受理类型
        /// </summary>
        public DispatchAck Ack { get; }
    }
}
