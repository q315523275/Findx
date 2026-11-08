using Microsoft.Extensions.Options;

namespace Findx.Extensions.Scheduler;

/// <summary>
///     分布式调度组件配置项（配置节 Findx:Scheduler）
/// </summary>
public class SchedulerOptions : IOptions<SchedulerOptions>
{
    /// <summary>
    ///     当前节点名称（WorkerId 推导依据，建议每个节点配置不同值）
    /// </summary>
    public string NodeName { set; get; } = Environment.MachineName;

    /// <summary>
    ///     当前节点对外可访问地址（http://host:port），Master 写入 Redis 供 Worker 回调，Worker 上报供 Master 分发
    /// </summary>
    public string NodeAddress { set; get; } = "http://localhost:5000";

    /// <summary>
    ///     是否启用调度端职责（Master 选举 + 到期扫描分发 + 结果处理）
    /// </summary>
    public bool SchedulerEnabled { set; get; } = true;

    /// <summary>
    ///     是否启用执行端职责（Worker 心跳注册 + 接收执行 HTTP 接口）
    /// </summary>
    public bool WorkerEnabled { set; get; } = true;

    /// <summary>
    ///     Master 选举 Redis 锁 key
    /// </summary>
    public string MasterLockKey { set; get; } = "findx:scheduler:master";

    /// <summary>
    ///     Master 锁租期（秒），持有期间按 TTL/3 自动续期
    /// </summary>
    public int MasterLockTtlSeconds { set; get; } = 10;

    /// <summary>
    ///     Master 节点信息（Address 等）写入的 Redis key
    /// </summary>
    public string MasterInfoKey { set; get; } = "findx:scheduler:master:info";

    /// <summary>
    ///     Worker 注册表 key 前缀（历史字段：曾以 String key 逐节点注册，现由 WorkerIndexKey 的 Hash 注册表取代，保留兼容）
    /// </summary>
    public string WorkerKeyPrefix { set; get; } = "findx:scheduler:worker:";

    /// <summary>
    ///     Worker 注册表索引 key（Redis Hash：field=WorkerId，value=WorkerNodeInfo JSON）。
    ///     心跳 HSET 覆盖写，Master 侧 HGetAll 一次拉全量，避免对 WorkerKeyPrefix+“*”执行 Redis KEYS 扫描
    /// </summary>
    public string WorkerIndexKey { set; get; } = "findx:scheduler:worker:index";

    /// <summary>
    ///     Worker 心跳注册 TTL（秒），过期即视为下线，注册过期自动剔除
    /// </summary>
    public int WorkerHeartbeatTtlSeconds { set; get; } = 30;

    /// <summary>
    ///     Worker 心跳上报间隔（秒），应明显小于 WorkerHeartbeatTtlSeconds
    /// </summary>
    public int WorkerHeartbeatIntervalSeconds { set; get; } = 10;

    /// <summary>
    ///     Master 到期扫描循环间隔（秒）
    /// </summary>
    public int SchedulerScanIntervalSeconds { set; get; } = 5;

    /// <summary>
    ///     单周期最大重试次数（超过后写死信表）
    /// </summary>
    public int MaxRetryCount { set; get; } = 5;

    /// <summary>
    ///     重试退避基数（毫秒）：delay = min(base * 2^attempt, max)
    /// </summary>
    public int RetryBaseDelayMs { set; get; } = 1000;

    /// <summary>
    ///     重试退避上限（毫秒）
    /// </summary>
    public int RetryMaxDelayMs { set; get; } = 60000;

    /// <summary>
    ///     是否开启同一「Job+周期」执行权 Redis 锁，防止重复分发/执行
    /// </summary>
    public bool EnableExecutionLock { set; get; } = true;

    /// <summary>
    ///     在途执行超时兜底（分钟），0=关闭；超时无回调的执行按失败处理进入重试/死信。
    ///     默认 10：Master 故障转移后旧 Master 在途任务由新 Master 超时兜底接管，避免静默丢失
    /// </summary>
    public int ExecutionTimeoutMinutes { set; get; } = 10;

    /// <summary>
    ///     超过最大重试次数后是否写 JobDeadLetter 死信表
    /// </summary>
    public bool DeadLetterEnabled { set; get; } = true;

    /// <summary>
    ///     分发策略名称（当前内置 RoundRobin；Weight 字段已预留权重扩展位）
    /// </summary>
    public string DispatchStrategy { set; get; } = "RoundRobin";

    /// <summary>
    ///     机器间接口共享令牌（配置后请求须携带 X-Scheduler-Token 头一致才放行）；留空则不校验
    /// </summary>
    public string ClusterToken { set; get; }

    /// <summary>
    ///     this
    /// </summary>
    public SchedulerOptions Value => this;
}
