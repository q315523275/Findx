# Findx.Extensions.Scheduler

分布式任务调度组件（Master / Worker 分工 + Redis 协调 + HTTP 机器间通信）。作业定义持久化在 `scheduler_job` 表，Master 按 Cron 计算到期时刻并分发到具备该作业能力的 Worker 执行，Worker 受理后立即返回、异步执行并回调结果，失败按指数退避重试、超限写死信。

作业本身仍然实现 `Findx.Jobs` 的 `IJob`，所以**同一份 Job 代码，内存版和分布式版通用**，用哪个由 `Findx:Jobs:ScheduleType` 决定。

> Nuget 包引用

```
Findx.Extensions.Scheduler
```

目标框架：`net6.0` / `net8.0` / `net10.0`，依赖 `Findx.Jobs`（`IJob` 契约与执行器）、`Findx.Redis`（选举锁、注册表、幂等键）、`Findx.Locks`（`RLock`）。

> 与内存版 Jobs 的关系

`SchedulerModule` 声明 `[DependsOnModules([typeof(JobModule)])]`。当 `Findx:Jobs:ScheduleType = Distributed` 时：

- `JobModule` 不注册任何调度/执行服务（内存版整体跳过）
- 本模块自行注册 Master 侧与 Worker 侧服务
- `IJob` 类型由 `IJobFinder` 统一扫描，Worker 上报的能力就是这些类型的 `FullName`

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 JobModule + SchedulerModule
```

## 架构总览

```mermaid
flowchart TB
    subgraph Redis[(Redis)]
        R1[Master 锁<br/>findx:scheduler:master]
        R2[Master 地址<br/>findx:scheduler:master:info]
        R3[Worker 注册表 Hash<br/>findx:scheduler:worker:index]
        R4[执行权锁<br/>findx:scheduler:exec:{jobId}:{period}]
        R5[结果幂等键<br/>findx:scheduler:result:{traceId}]
    end

    subgraph Master[Master 节点]
        E[MasterElectionService<br/>抢锁 + 续期 + 脑裂检测]
        S[DistributedScheduler<br/>扫描循环 10s]
        B[SchedulerJobBuffer<br/>内存预取窗口 60s]
        Q[SchedulerDispatchQueue<br/>有界队列 4096]
        D[JobDispatcher<br/>选点 + 执行权锁 + HTTP]
        RH[JobResultHandler<br/>幂等 + 重试/死信]
        IF[InFlightExecutionRegistry<br/>在途执行快照]
    end

    subgraph Worker[Worker 节点]
        C[JobExecutionController<br/>POST /execute]
        EQ[JobExecutionQueue<br/>有界队列 1024]
        EX[IJobExecutor 执行 IJob]
        RP[JobResultReporter]
    end

    DB[(scheduler_job<br/>scheduler_job_dead_letter)]

    E --> R1
    E --> R2
    S --> B
    B -->|窗口到期| Q
    B -->|CAS 抢占| DB
    Q --> D
    D --> R4
    D --> R3
    D -->|HTTP /execute| C
    D --> IF
    C --> EQ
    EQ --> EX
    EX --> RP
    RP -->|HTTP /result| RH
    RH --> R5
    RH --> IF
    RH -->|成功推进 / 失败退避| DB
    RH -->|超限| DB
```

| 角色 | 职责 | 开关 |
|---|---|---|
| 调度端（Master） | Master 选举、扫描到期、CAS 抢占、选点分发、结果处理、超时兜底 | `SchedulerEnabled` |
| 执行端（Worker） | 心跳注册 + 能力上报、受理执行请求、执行 `IJob`、回报结果 | `WorkerEnabled` |

两个开关可以只开一个，也可以同时开（单机或小规模部署常见），组件会自动区分职责。

## Json 配置

```json
{
  "Findx": {
    "Jobs": {
      "Enabled": true,
      "ScheduleType": "Distributed"
    },
    "Scheduler": {
      "NodeName": "app-node-1",
      "NodeAddress": "http://10.0.0.11:8080",
      "SchedulerEnabled": true,
      "WorkerEnabled": true,
      "ClusterToken": "替换为随机长串",
      "SchedulerScanIntervalSeconds": 10,
      "LookaheadWindowSeconds": 60,
      "LookaheadRefreshSeconds": 30,
      "DispatchWorkerCount": 4,
      "ExecutionWorkerCount": 8,
      "MaxRetryCount": 5,
      "ExecutionTimeoutMinutes": 10,
      "MisfireStrategy": "FireOnceNow",
      "DeadLetterEnabled": true
    }
  }
}
```

> 配置项

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `NodeName` | 机器名 | 节点名，推导 `WorkerId`，多节点必须不同 |
| `NodeAddress` | `http://localhost:5000` | 本节点对外地址。Master 写进 Redis 供 Worker 回调，Worker 上报供 Master 分发 |
| `SchedulerEnabled` | `true` | 是否参与调度端职责 |
| `WorkerEnabled` | `true` | 是否参与执行端职责 |
| `ClusterToken` | 空 | 机器间接口令牌。**启用调度端或执行端时必填**，否则启动直接抛异常 |
| `MasterLockKey` | `findx:scheduler:master` | Master 选举锁 key（实际 Redis key 前缀为 `lock:`） |
| `MasterLockTtlSeconds` | `10` | 锁租期，按 TTL/3 自动续期 |
| `MasterInfoKey` | `findx:scheduler:master:info` | Master 地址写入的 key |
| `WorkerIndexKey` | `findx:scheduler:worker:index` | Worker 注册表 Hash（field = WorkerId） |
| `WorkerKeyPrefix` | `findx:scheduler:worker:` | 历史字段，保留兼容（现由 Hash 注册表取代，不再使用 KEYS 扫描） |
| `WorkerHeartbeatTtlSeconds` | `30` | 心跳 TTL，超时视为下线并摘除注册 |
| `WorkerHeartbeatIntervalSeconds` | `10` | 心跳间隔，应明显小于 TTL |
| `SchedulerScanIntervalSeconds` | `10` | Master 扫描循环间隔 |
| `LookaheadWindowSeconds` | `60` | 预取窗口：一次把窗口内即将到期的任务拉进内存，窗口内不再查库 |
| `LookaheadRefreshSeconds` | `30` | 窗口刷新间隔，后台执行，调度线程不等待 |
| `MaxRetryCount` | `5` | 允许的重试次数上限，达到后仍失败才进死信 |
| `RetryBaseDelayMs` | `1000` | 退避基数，`delay = min(base * 2^attempt, max)` |
| `RetryMaxDelayMs` | `60000` | 退避上限 |
| `EnableExecutionLock` | `true` | 同一「Job + 周期」的 Redis 执行权锁，防重复分发/执行 |
| `ExecutionTimeoutMinutes` | `10` | 在途执行超时兜底，`0` 关闭。超时无回调按失败处理 |
| `DeadLetterEnabled` | `true` | 超限后是否写 `scheduler_job_dead_letter` |
| `DispatchStrategy` | `RoundRobin` | 选点策略名（内置 `RoundRobin`，`Weight` 已预留） |
| `DispatchQueueCapacity` | `4096` | Master 分发队列容量，满时降级同步分发并告警 |
| `DispatchWorkerCount` | `4` | 分发线程数（取锁 + HTTP 投递 + 失败入库都在这里） |
| `DispatchHttpTimeoutSeconds` | `5` | 投递 HTTP 超时，只约束「送到 Worker」这一步 |
| `ExecutionQueueCapacity` | `1024` | Worker 受理队列容量，满时返回 503 让 Master 重试 |
| `ExecutionWorkerCount` | `8` | Worker 执行线程数 |
| `MisfireStrategy` | `FireOnceNow` | 迟到超过一个窗口：`FireOnceNow` 补触发一次，`DoNothing` 只推进不触发 |
| `CatchUpWithinWindow` | `false` | 是否补齐 now 之前错过的多个触发点 |
| `CatchUpMaxCount` | `30` | 单轮单任务最多补触发次数（仅 `CatchUpWithinWindow` 生效） |

> 两条硬约束

1. **`ClusterToken` 必填**：`SchedulerEnabled` 或 `WorkerEnabled` 为 true 且 token 为空时，`SchedulerModule` 在启动阶段直接抛 `InvalidOperationException`。原因是 `/api/scheduler/jobs/execute` 与 `/result` 属于机器间接口，零鉴权等于内网任意客户端可触发任意 `IJob` 执行（RCE 面）或伪造结果污染任务状态
2. **`NodeAddress` 不要留默认值**：多节点部署沿用 `http://localhost:5000` 会导致分发与回调打到本机自环，组件启动时会打警告

## 数据库表

| 表 | 说明 | 字段 |
|---|---|---|
| `scheduler_job` | 作业定义 | `Id` / `JobName` / `HandlerType` / `CronExpression` / `Parameter` / `Status` / `LastRunTime` / `NextRunTime` / `RetryAtTime` / `TryCount` / `Remark` + 审计字段 |
| `scheduler_job_dead_letter` | 死信 | `JobId` / `JobName` / `HandlerType` / `Parameter` / `CronExpression` / `TryCount` / `LastError` / `WorkerAddress` / `FiredTime` / `TraceId` |

`HandlerType` 必须是 **`IJob` 实现类的完整类型名**（如 `Findx.WebHost.Jobs.TestJob`）。Master 拿 `JobName` 和 `HandlerType` 去匹配 Worker 上报的能力列表（能力 = 本地所有 `IJob` 类型的 `FullName`），填错类型名会导致"无可执行 Worker 节点"而分发失败。

> 作业定义写入

组件只提供机器间接口（`/execute`、`/result`），**没有作业 CRUD 的 REST 接口**。作业定义由业务侧通过 `ScheduledJobStorage` 维护：

```csharp
var storage = serviceProvider.GetRequiredService<ScheduledJobStorage>();

await storage.AddAsync(new ScheduledJob
{
    JobName = "清理临时文件",
    HandlerType = typeof(CleanTempJob).FullName,   //  {命名空间}.CleanTempJob
    CronExpression = "0 0 3 * * ?",                //  每天 03:00
    Parameter = "{\"days\":7}",                    //  透传给 JobExecuteContext.JobExecuteInfo.Parameter
    Status = JobStatus.Normal,
    NextRunTime = CronUtility.GetNextOccurrence("0 0 3 * * ?", DateTime.Now)
});
```

`ScheduledJobStorage` 的写操作会触发 `JobsChanged` 事件，`SchedulerJobBuffer` 订阅后标记脏；Master 的预取窗口在下次刷新时重新拉取，新作业最长 `LookaheadRefreshSeconds`（默认 30s）内生效。（示例中的 `CronUtility` 位于 `Findx.Utilities`）

## 作业实现

与内存版完全一致，不需要任何分布式适配：

```csharp
[Job(Cron = "0 */5 * * * ?", Name = "清理临时文件", Description = "清理 N 天前的临时文件")]
public class CleanTempJob : IJob
{
    private readonly IFileService _fileService;

    public CleanTempJob(IFileService fileService) => _fileService = fileService;

    public async Task<JobResult> RunAsync(JobExecuteContext context, CancellationToken cancellationToken = default)
    {
        //  context.JobExecuteInfo.Parameter 是 scheduler_job.Parameter 原样透传
        var days = context.JobExecuteInfo.Parameter.ToObject<CleanParam>()?.Days ?? 7;
        var removed = await _fileService.CleanTempAsync(days, cancellationToken);

        //  返回值决定成败：IsSuccess=false 或 IsCancelled=true 都按失败处理，进入重试
        return new JobResult { IsSuccess = true, Message = $"清理 {removed} 个文件" };
    }
}
```

每次执行都会创建独立 DI 作用域（`CreateScope`），`IJob` 及其依赖按 Scoped 生命周期释放，长跑进程不会有实例累积。

## 执行链路

### 1. Master 选举

`MasterElectionService` 是 `BackgroundService`：

1. 非阻塞抢 Redis 锁 `findx:scheduler:master`（`renew = true`，内部按 TTL/3 用 Lua 原子续期）
2. 抢到即 `IsMaster = true`，写 Master 地址到 `findx:scheduler:master:info`（TTL ≈ 锁租期 3 倍）
3. 持锁期间每 TTL/3 校验一次 `lock:{MasterLockKey}` 的值是否仍是自己的锁 ID，发现易主（网络分区 / GC 停顿）立刻降级 Standby
4. 抢不到则按 `MasterLockTtlSeconds / 2` 间隔重试

只有 `IsMaster = true` 的节点才会跑调度循环，其余节点空转等待选举。

### 2. 扫描与预取（不查库）

`DistributedScheduler` 每 `SchedulerScanIntervalSeconds` 跑一轮，每轮用「周期 - 本轮耗时」做补偿，避免扫描耗时累积成调度漂移。

到期判定全部走内存：

```
SchedulerJobBuffer（预取窗口）
  ├─ 首次调用同步 LoadAsync：一次查出窗口内即将到期的任务
  ├─ 之后每轮纯内存 ScanDueAsync，不发 SQL
  ├─ 窗口耗尽或超过 LookaheadRefreshSeconds → 后台重查（调度线程不等结果）
  └─ 重试到期优先于周期到期
```

窗口内的判断：

| 情况 | 处理 |
|---|---|
| `RetryAtTime <= now` | 生成 `DispatchKind.Retry` 条目，重试优先处理 |
| `NextRunTime <= now`，迟到未超窗口 | 按 Cron 计算下一次，生成 `DispatchKind.Cycle` 条目 |
| `NextRunTime <= now`，迟到超窗口且 `DoNothing` | 只推进 `NextRunTime`，不触发 |
| 迟到超窗口但 `FireOnceNow` | 补触发一次 |
| 开启 `CatchUpWithinWindow` | 补齐 now 之前错过的多个触发点，受 `CatchUpMaxCount` 限制 |

### 3. CAS 抢占（防重复）

条目入队后，**由分发线程做 CAS 抢占**，不是调度线程：

- `ClaimDueAsync`：以 `ExpectedNextRunTime` 为乐观锁，把 `NextRunTime` 推进到新值、写 `LastRunTime`
- `ClaimRetryAsync`：清 `RetryAtTime`

抢占失败说明该周期已被其它实例处理，直接从内存窗口剔除该任务，等下次重查校正。这样即使 Master 刚切换、旧 Master 还没完全停机，同一周期也只会被分发一次。

### 4. 分发

`JobDispatcher.DispatchAsync` 的五个步骤：

1. **反查可执行节点**：`WorkerRegistry.GetCandidates(JobName, HandlerType)` → 只挑上报了该能力且心跳未过期的 Worker
2. **选点**：`IDispatchStrategy`（默认 `RoundRobinDispatchStrategy`，`Weight` 字段已预留）
3. **执行权锁**：`findx:scheduler:exec:{jobId}:{period}`，TTL = `max(60s, ExecutionTimeoutMinutes*60, MasterLockTtlSeconds*2)`，抢不到说明该周期已在别处执行，跳过
4. **HTTP 投递**：`POST {WorkerAddress}/api/scheduler/jobs/execute`，带 `X-Scheduler-Token` 头，超时 `DispatchHttpTimeoutSeconds`
5. **登记在途**：`InFlightExecutionRegistry` 记录 `TraceId / Period / Attempt / WorkerAddress / ExecutionLock`，供结果回调与超时兜底使用

分发失败（无节点、不可达、锁占用）统一走 `HandleFailureAsync` 进入退避重试，不会丢任务。

### 5. Worker 受理（受理即返回）

`JobExecutionController.ExecuteAsync`：

1. 校验 `X-Scheduler-Token`
2. 参数校验（`HandlerType` 必填）
3. `JobExecutionQueue.TryEnqueue(request)`：入队成功立即返回 `Success`，队列满返回 `503` 让 Master 走重试

Master 侧等到的只是一次轻量 HTTP 响应，**不是整个作业的执行时长**，这是"调度越来越不准"问题的关键修正点。

### 6. Worker 执行与回调

`JobExecutionQueue` 由 `ExecutionWorkerCount` 个消费线程（默认 8）执行：

1. `CreateScope` → 取 `IJobExecutor` → 构造 `JobExecuteInfo`（`Parameter` 来自分发请求）
2. `executor.ExecuteAsync(context)`，成败由 `context.JobResult.IsSuccess / IsCancelled` 判定
3. `JobResultReporter.ReportAsync` 回调 Master `POST /api/scheduler/jobs/result`，带 `TraceId / IsSuccess / Error / Message / ElapsedMs`
4. 回调失败只记日志，由 Master 的 `ExecutionTimeoutMinutes` 兜底

### 7. 结果处理

`JobResultHandler.HandleAsync`：

1. **幂等守卫**：`SETNX findx:scheduler:result:{traceId}`，TTL 10 分钟，重复回报直接丢弃
2. 取出在途记录并释放执行权锁
3. 成功 → `ResetSuccessAsync`：`TryCount = 0`、清 `RetryAtTime`，周期继续
4. 失败 → `HandleFailureAsync`：见下节
5. **孤儿回报**：找不到在途记录（Master 刚切换、内存状态已丢）时不忽略，按 DB 当前状态决策——成功幂等落库，失败按 `TryCount` 续跑重试或进死信

## 失败与重试

```
失败
  ├─ nextAttempt = Attempt + 1
  ├─ 未超 MaxRetryCount
  │     └─ RetryAtTime = now + min(RetryBaseDelayMs * 2^(attempt-1), RetryMaxDelayMs)
  │        （1s → 2s → 4s → 8s … 上限 60s），不动 NextRunTime，周期锚点不偏移
  └─ 已达 MaxRetryCount
        ├─ DeadLetterEnabled → 写 scheduler_job_dead_letter
        └─ 推进 NextRunTime、归零 TryCount、清 RetryAtTime（否则过去时刻会被下轮立刻提前触发）
```

重试时刻写 `RetryAtTime` 而不是 `NextRunTime`，好处是重试与周期解耦：重试在内存窗口里被优先拾取，周期锚点不被重试拖偏；Master 故障转移后新 Master 从库里读到 `RetryAtTime` 也能续跑。

## 超时兜底与故障转移

| 场景 | 兜底机制 |
|---|---|
| Worker 假死、执行无回调 | `SweepTimeoutExecutionsAsync` 每轮扫 `InFlightExecutionRegistry`，超过 `ExecutionTimeoutMinutes` 无回调的按失败处理（进重试或死信） |
| Master 崩溃 | 锁 TTL 到期后其它节点抢到锁成为新 Master；旧 Master 留在途记录丢失，靠「孤儿回报按 DB 决策」+ 超时兜底收敛 |
| Master 假死但仍持锁 | 持锁节点每 TTL/3 校验锁归属，发现易主即降级；同时 TTL 到期会自然释放 |
| 分发队列饱和 | 降级为「本线程 CAS + 同步分发」，任务不丢弃，只打告警 |
| 执行队列饱和 | 受理接口返回 503，Master 侧按失败退避重试 |

## Redis key 一览

| key | 类型 | 用途 |
|---|---|---|
| `lock:findx:scheduler:master` | String（RLock） | Master 选举锁，TTL `MasterLockTtlSeconds`，自动续期 |
| `findx:scheduler:master:info` | String | Master 地址与节点信息，Worker 回调时读取 |
| `findx:scheduler:worker:index` | Hash | Worker 注册表，`field = WorkerId`，`value = WorkerNodeInfo` JSON（含 `Address` / `Capabilities` / `HeartbeatTime`） |
| `findx:scheduler:exec:{jobId}:{period}` | RLock | 执行权锁，防同一周期重复分发 |
| `findx:scheduler:result:{traceId}` | String | 结果回报幂等键，TTL 10 分钟 |

`WorkerKeyPrefix` 是历史字段，现在的实现是 Hash 注册表（`HGetAll` 一次拉全量），不再对 `prefix*` 做 `KEYS` 扫描，避免 Redis 阻塞。

## 机器间接口

| 方法 | 路径 | 说明 |
|---|---|---|
| POST | `/api/scheduler/jobs/execute` | Master → Worker，受理任务（入队即返回） |
| POST | `/api/scheduler/jobs/result` | Worker → Master，回报执行结果 |

两个接口都要求请求头带 `X-Scheduler-Token`（与 `ClusterToken` 一致），组件不做用户级鉴权，靠这个共享令牌隔离内网调用。若宿主有网关，建议再加一层来源 IP 白名单。

## 部署要点

1. **`NodeAddress` 必须是对其它节点可达的地址**，不能是 `localhost`
2. **`HandlerType` 与 Worker 侧能扫描到的 `IJob` 类型完全一致**，跨节点部署时组件包版本也要一致，否则能力匹配失败
3. **节点时钟要同步**（NTP）。到期判断、CAS 推进、退避计算都基于本机时间，时钟漂移会导致任务提前或延后
4. **单机部署**：`SchedulerEnabled = true` + `WorkerEnabled = true`，Redis 仍需要（选举锁与注册表）；若不想要 Redis，就用内存版 `Findx:Jobs:ScheduleType = Simple`
5. **多实例且作业不能并发**：保持 `EnableExecutionLock = true`
6. **秒级任务**：`LookaheadWindowSeconds` 要大于等于 Cron 的最小间隔，否则窗口取不到任务；`CatchUpWithinWindow` 打开时注意 `CatchUpMaxCount`

## 与内存版 Jobs 的选型

| 维度 | 内存版（`Findx.Jobs`，`Simple`） | 分布式版（本组件，`Distributed`） |
|---|---|---|
| 外部依赖 | 无 | Redis（选举、注册、幂等、执行锁） |
| 多实例 | 每个实例各跑一遍（会重复执行） | 选举出唯一 Master，任务只分发一次 |
| 执行位置 | 当前进程内 | 具备该作业能力的任意 Worker |
| 故障转移 | 无（进程挂了任务不再执行） | 锁 TTL 到期后自动切换 Master |
| 持久化 | 内存，重启丢失 | `scheduler_job` 表 |
| 适用场景 | 单机、开发、无 Redis 环境 | 多实例生产、需要持久化与故障转移 |

同一个 `[Job]` 类两边通用，切换只改 `Findx:Jobs:ScheduleType` 与是否配 `Findx:Scheduler` 段。

## 当前边界

| 项 | 状态 |
|---|---|
| Master / Worker 分工、选举、故障转移 | 已实现 |
| 预取窗口、misfire 策略、补触发 | 已实现 |
| CAS 抢占、执行权锁、幂等回调 | 已实现 |
| 指数退避重试、死信表 | 已实现 |
| 在途超时兜底、孤儿结果恢复 | 已实现 |
| 权重选点 | **未实现**：`WorkerNodeInfo.Weight` 与 `DispatchStrategy` 字段已预留，`RoundRobin` 不读权重 |
| 作业 CRUD REST 接口 | **未实现**：只有 `execute` / `result` 两个机器间接口，定义维护走 `ScheduledJobStorage` |
| 作业分片 / 广播（同一周期多节点执行） | **未实现**：一个周期只投递给一个 Worker |
| 失败告警通道 | **未实现**：只在日志里告警，需要接站内信/企微请自行监听日志或改 `JobResultHandler` |
| 依赖关系（DAG 编排） | **未实现**：作业之间不能配置前置依赖 |
