using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler.Options;

/// <summary>
///     分布式调度组件配置项（配置节 Findx:Scheduler）。
///     <para>基础版本只依赖数据库与 HTTP：租约、节点注册、执行账本全部落库，不再需要 Redis。</para>
/// </summary>
public class SchedulerOptions : IOptions<SchedulerOptions>
{
    /// <summary>
    ///     当前节点名称（WorkerId 推导依据，建议每个节点配置不同值）
    /// </summary>
    public string NodeName { set; get; } = Environment.MachineName;

    /// <summary>
    ///     当前节点对外可访问地址（http://host:port）：写入分发包供执行端回调，并作为执行端地址被调度端分发
    /// </summary>
    public string NodeAddress { set; get; } = "http://localhost:5000";

    /// <summary>
    ///     是否启用调度端职责（Master 选举 + 到期扫描分发 + 结果处理 + 超时兜底）
    /// </summary>
    public bool SchedulerEnabled { set; get; } = true;

    /// <summary>
    ///     是否启用执行端职责（心跳注册 + 接收执行 HTTP 接口）
    /// </summary>
    public bool WorkerEnabled { set; get; } = true;

    /// <summary>
    ///     调度端候选地址列表（执行端心跳与结果回调的兜底目标）。
    ///     <para>执行端自己不做主节点发现，按顺序上报即可；调度端会把心跳落库。</para>
    /// </summary>
    public string[] MasterEndpoints { set; get; } = [];

    /// <summary>
    ///     Master 租约键（多集群共用一个数据源时用于区分）
    /// </summary>
    public string MasterLockKey { set; get; } = "findx:scheduler:master";

    /// <summary>
    ///     Master 租期（秒）。持有期间按 TTL/3 续期；租期失效只影响谁在扫描，不影响触发正确性
    /// </summary>
    public int MasterLockTtlSeconds { set; get; } = 15;

    /// <summary>
    ///     Worker 心跳存活阈值（秒），超过即视为下线，注册行会被调度端清理
    /// </summary>
    public int WorkerHeartbeatTtlSeconds { set; get; } = 45;

    /// <summary>
    ///     Worker 心跳上报间隔（秒），应明显小于 WorkerHeartbeatTtlSeconds
    /// </summary>
    public int WorkerHeartbeatIntervalSeconds { set; get; } = 10;

    /// <summary>
    ///     本地任务清单（IJob 实现类全名）刷新间隔（秒）。
    ///     <para>每个执行节点承载的任务可能不同，心跳上报的清单必须能跟上运行时变化（插件热加载、条件注册），
    ///     而不是只在启动时扫一次；扫描成本低，默认 60 秒。</para>
    /// </summary>
    public int CapabilityRefreshSeconds { set; get; } = 60;

    /// <summary>
    ///     Master 到期扫描循环间隔（秒）。
    ///     <para>扫描本身是纯内存比对（窗口已在内存里），成本与任务数线性、不产生 I/O，
    ///     因此默认按 1 秒对齐：到期判定误差上限就是一个间隔，5 秒会让稳态任务天然晚 0~5 秒。</para>
    /// </summary>
    public int SchedulerScanIntervalSeconds { set; get; } = 1;

    /// <summary>
    ///     超时兜底扫描间隔（秒）。兜底是批处理写库，必须与「到期扫描」解耦，
    ///     否则一次大批量超时会把调度节奏吃掉。
    /// </summary>
    public int SweepIntervalSeconds { set; get; } = 30;

    /// <summary>
    ///     单轮兜底最多处理的执行行条数（Claimed / Dispatched 各自最多该数量）
    /// </summary>
    public int SweepBatchSize { set; get; } = 100;

    /// <summary>
    ///     兜底批处理中每处理若干行后的让出间隔（毫秒），避免长时间占住连接与线程
    /// </summary>
    public int SweepBatchDelayMs { set; get; } = 50;

    /// <summary>
    ///     单周期最大重试次数（首次失败后最多再重试该次数，超过后死信隔离）
    /// </summary>
    public int MaxRetryCount { set; get; } = 5;

    /// <summary>
    ///     重试退避基数（毫秒）：delay = min(base * 2^(n-1), max)
    /// </summary>
    public int RetryBaseDelayMs { set; get; } = 1000;

    /// <summary>
    ///     重试退避上限（毫秒）
    /// </summary>
    public int RetryMaxDelayMs { set; get; } = 60000;

    /// <summary>
    ///     未确认受理的兜底时限（秒）：领取后超过该时长仍停在 Claimed，说明进程崩溃或投递未送达，
    ///     按失败进入退避重试
    /// </summary>
    public int ClaimTimeoutSeconds { set; get; } = 30;

    /// <summary>
    ///     已受理执行的超时兜底（分钟），0=关闭；超时无回调按失败处理进入重试/死信。
    ///     <para>兜底读的是执行账本表，换主后依然有效。</para>
    /// </summary>
    public int ExecutionTimeoutMinutes { set; get; } = 30;

    /// <summary>
    ///     超过最大重试次数后是否写 JobDeadLetter 记录（隔离本身总是生效）
    /// </summary>
    public bool DeadLetterEnabled { set; get; } = true;

    /// <summary>
    ///     分发策略名称（当前内置 RoundRobin）
    /// </summary>
    public string DispatchStrategy { set; get; } = "RoundRobin";

    /// <summary>
    ///     调度端分发队列容量（有界通道）。调度循环只入队不等待网络；队列满时该周期延后并告警，任务不丢弃。
    ///     <para>容量必须大于「单波到期任务数」，否则一波 5000 条会瞬间填满队列，
    ///     部分任务被推到下一个周期（压测 4000 档在 4096 容量下出现 37 次饱和）。默认 16384。</para>
    /// </summary>
    public int DispatchQueueCapacity { set; get; } = 0;

    /// <summary>
    ///     独立分发线程数：领取事务 + HTTP 投递 + 失败退避都在这些线程里执行，不占用调度扫描线程
    /// </summary>
    public int DispatchWorkerCount { set; get; } = 0;

    /// <summary>
    ///     批量领取条数：一个分发线程一次从队列取多少条、共用同一个事务与一次提交。
    ///     <para>1 = 退化为逐条领取；调大能显著摊薄事务提交成本（压测中单条领取的主要开销是 COMMIT 刷盘），
    ///     但批次过大会放大单次事务的锁持有时间与回滚影响面，默认 50。</para>
    /// </summary>
    public int DispatchClaimBatchSize { set; get; } = 50;

    /// <summary>
    ///     分发在飞请求上限：全局同时未完成的接收请求数。
    ///     <para>吞吐上限 ≈ 在飞上限 / 单次往返耗时，与线程数无关——固定线程数会让每轮批量被串行化，
    ///     单次往返 5ms 时 10 线程也只能做到约 2000/s，而且一条超时就会卡住整批。
    ///     并发投递 + 复用一个 HttpClient 之后，单次往返降到毫秒级，该值决定最终吞吐，默认 64。</para>
    /// </summary>
    public int DispatchMaxInFlight { set; get; } = 0;

    /// <summary>
    ///     分发/回调 HTTP 超时（秒）
    /// </summary>
    public int DispatchHttpTimeoutSeconds { set; get; } = 2;

    /// <summary>
    ///     执行日志落库的最大字符数（结果回调里携带的日志尾部摘要，默认 20000，超出保留尾部）
    /// </summary>
    public int MaxExecutionLogChars { set; get; } = 20000;

    /// <summary>
    ///     执行日志上报队列容量（有界，满时丢最旧）
    /// </summary>
    public int LogShipQueueCapacity { set; get; } = 4096;

    /// <summary>
    ///     执行日志单批上报的分片数上限
    /// </summary>
    public int LogShipBatchSize { set; get; } = 20;

    /// <summary>
    ///     执行日志合并上报窗口（毫秒）：把同一次执行的连续分片攒成更少的 HTTP 请求
    /// </summary>
    public int LogShipIntervalMs { set; get; } = 2000;

    /// <summary>
    ///     结果上报队列容量（有界）：批量上报时结果先入队，满则回落逐条上报，不丢结果
    /// </summary>
    public int ResultShipQueueCapacity { set; get; } = 0;

    /// <summary>
    ///     结果单批上报条数：多个执行结果合并成一次 HTTP；1 = 关闭批量，回落逐条回调（旧行为）
    /// </summary>
    public int ResultShipBatchSize { set; get; } = 20;

    /// <summary>
    ///     结果合并上报窗口（毫秒）：攒批等待时间，越大越省请求、回调越晚
    /// </summary>
    public int ResultShipIntervalMs { set; get; } = 1000;

    /// <summary>
    ///     结果批量上报的并发批次数：单批一次 HTTP 往返，串行发送时上行吞吐被往返时延锁死；
    ///     并发发送多批可在同一窗口内把上行吞吐提高数倍（结果的终结本身是幂等 CAS，与顺序无关）
    /// </summary>
    public int ResultShipConcurrency { set; get; } = 0;

    /// <summary>
    ///     执行日志脱敏开关（默认关闭：日志原样采集入库）
    /// </summary>
    public bool LogMaskingEnabled { set; get; } = false;

    /// <summary>
    ///     执行日志脱敏的附加正则（命中替换为 ***；内置已覆盖密码/令牌/手机号/身份证号）
    /// </summary>
    public List<string> LogMaskPatterns { set; get; } = new();

    /// <summary>
    ///     执行历史保留天数（账本 + 日志明细；&lt;=0 表示不清理）
    /// </summary>
    public int RetentionDays { set; get; } = 30;

    /// <summary>
    ///     执行历史清理扫描间隔（分钟，最小 10）
    /// </summary>
    public int RetentionSweepIntervalMinutes { set; get; } = 1440;

    /// <summary>
    ///     执行历史单批删除条数
    /// </summary>
    public int RetentionBatchSize { set; get; } = 500;

    /// <summary>
    ///     执行端受理队列容量（有界通道）。受理接口入队即返回，队列满时返回失败让调度端走重试
    /// </summary>
    public int ExecutionQueueCapacity { set; get; } = 0;

    /// <summary>
    ///     执行端执行线程数：长任务只占用自己的执行线程，不影响受理接口响应
    /// </summary>
    public int ExecutionWorkerCount { set; get; } = 0;

    /// <summary>
    ///     预取窗口（秒）：调度端每次把「窗口内即将到期」的任务一次性拉到内存
    /// </summary>
    public int LookaheadWindowSeconds { set; get; } = 60;

    /// <summary>
    ///     预取窗口刷新间隔（秒）
    /// </summary>
    public int LookaheadRefreshSeconds { set; get; } = 30;

    /// <summary>
    ///     错过周期（misfire）处理策略：任务迟到超过一个预取窗口时的行为
    /// </summary>
    public SchedulerMisfireStrategy MisfireStrategy { set; get; } = SchedulerMisfireStrategy.FireOnceNow;

    /// <summary>
    ///     是否补齐「过去错过的多个触发点」
    /// </summary>
    public bool CatchUpWithinWindow { set; get; } = false;

    /// <summary>
    ///     单轮单任务最多补触发次数（仅 CatchUpWithinWindow 生效）
    /// </summary>
    public int CatchUpMaxCount { set; get; } = 30;

    /// <summary>
    ///     机器间接口共享令牌（配置后请求须携带 X-Scheduler-Token 头一致才放行）
    /// </summary>
    public string ClusterToken { set; get; }

    //  容量类参数一律「0 = 按机器能力自动推导」。运维只需要关心业务语义（周期、重试、超时），
    //  不必去猜线程开几个、队列开多大——这些值随 CPU 核数线性推导，和实测最优点一致。

    /// <summary>
    ///     实际执行线程数：未配置时按 2 × CPU 核数推导（夹在 4~32）。
    ///     <para>压测依据：5000 条一波、单条 50ms 时，8 线程（32 并发）队列满 9527 次，32 线程（128 并发）为 0 次。</para>
    /// </summary>
    public int EffectiveExecutionWorkerCount => ExecutionWorkerCount > 0
        ? ExecutionWorkerCount
        : Math.Clamp(Environment.ProcessorCount * 2, 4, 32);

    /// <summary>
    ///     实际执行队列容量：未配置时按「每线程 256 条缓冲」推导，下限 2048。
    ///     <para>队列要能吞下单波突发：16 线程 → 4096，32 线程 → 8192。</para>
    /// </summary>
    public int EffectiveExecutionQueueCapacity => ExecutionQueueCapacity > 0
        ? ExecutionQueueCapacity
        : Math.Max(2048, EffectiveExecutionWorkerCount * 256);

    /// <summary>
    ///     实际结果上报并发：未配置时按「执行线程数 / 4」推导（夹在 2~8）。
    ///     <para>上行吞吐 ≈ 并发批次数 / 单批往返；与执行线程同比例放大才不会让结果回写成为新瓶颈。</para>
    /// </summary>
    public int EffectiveResultShipConcurrency => ResultShipConcurrency > 0
        ? ResultShipConcurrency
        : Math.Clamp(EffectiveExecutionWorkerCount / 4, 2, 8);

    /// <summary>
    ///     实际结果上报队列容量：未配置时取执行队列的 2 倍（结果产生速率不会快于受理速率）
    /// </summary>
    public int EffectiveResultShipQueueCapacity => ResultShipQueueCapacity > 0
        ? ResultShipQueueCapacity
        : EffectiveExecutionQueueCapacity * 2;

    /// <summary>
    ///     实际分发线程数：未配置时按 CPU 核数推导（夹在 2~16）
    /// </summary>
    public int EffectiveDispatchWorkerCount => DispatchWorkerCount > 0
        ? DispatchWorkerCount
        : Math.Clamp(Environment.ProcessorCount, 2, 16);

    /// <summary>
    ///     实际在飞请求上限：未配置时按「分发线程数 × 8」推导。
    ///     <para>吞吐 ≈ 在飞上限 / 单次往返，8 核机器推导为 64，与实测最优点一致（64 与 128 无差异，256 反而劣化）。</para>
    /// </summary>
    public int EffectiveDispatchMaxInFlight => DispatchMaxInFlight > 0
        ? DispatchMaxInFlight
        : EffectiveDispatchWorkerCount * 8;

    /// <summary>
    ///     实际分发队列容量：未配置时 16384（必须大于单波到期任务数）
    /// </summary>
    public int EffectiveDispatchQueueCapacity => DispatchQueueCapacity > 0
        ? DispatchQueueCapacity
        : 16384;

    /// <summary>
    ///     this
    /// </summary>
    public SchedulerOptions Value => this;
}
