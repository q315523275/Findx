# 分布式调度最小可运行示例（服务端 + 执行端）

两个独立的 `net10.0` API 项目，用来跑通 `Findx.Extensions.Scheduler` 的完整闭环：

| 项目 | 目录 | 角色 | 数据库 |
| --- | --- | --- | --- |
| `Findx.Samples.SchedulerServer` | `samples/SchedulerServer` | 调度端（Master） | 需要，数据源 `jobs` |
| `Findx.Samples.SchedulerWorker` | `samples/SchedulerWorker` | 执行端（Worker） | **不需要，也没有连接串** |

两个进程通过 HTTP 通信：执行端心跳 / 回传结果 / 上报日志（`ISchedulerMasterChannel`），
调度端投递 `execute` / `kill`。执行端不持有数据库连接是这套示例刻意保留的约束。

## 1. 准备数据库

```bash
# 建库（库名与 appsettings.json 里的 Initial Catalog 保持一致）
mysql -h127.0.0.1 -uroot -p -e "CREATE DATABASE IF NOT EXISTS findx_scheduler_demo DEFAULT CHARSET utf8mb4"

# 建表 + 唯一键 + 索引（唯一键不会被 UseAutoSyncStructure 自动创建，必须执行本脚本）
mysql -h127.0.0.1 -uroot -p findx_scheduler_demo < ../../docs/sql/scheduler_v2.sql
```

`scheduler_jobs` 由组件按实体自动创建（`UseAutoSyncStructure`），其余四张表以脚本为准。

如果本机账号密码不是 `root/123456`，改 `samples/SchedulerServer/appsettings.json`，或用环境变量覆盖：

```bash
export Findx__FreeSql__DataSource__jobs__ConnectionString="Data Source=127.0.0.1;Port=3306;User ID=xxx;Password=xxx;Initial Catalog=findx_scheduler_demo;Charset=utf8mb4;SslMode=none"
```

## 2. 配置要点

两个进程必须共用同一个 `Findx:Scheduler:ClusterToken`（机器间接口鉴权头 `X-Scheduler-Token`），
并且都要满足 `Findx:Jobs:Enabled=true` + `ScheduleType=Distributed`（否则与内存版 Jobs 冲突，组件启动即抛异常）。

| 配置 | 服务端 | 执行端 |
| --- | --- | --- |
| `Urls` | `http://localhost:7100` | `http://localhost:7101` |
| `SchedulerEnabled` | `true` | `false` |
| `WorkerEnabled` | `false` | `true` |
| `NodeAddress` | `http://localhost:7100` | `http://localhost:7101` |
| `MasterEndpoints` | 空 | `["http://localhost:7100"]` |
| `Findx:FreeSql` | 启用 + `jobs` 数据源 | `Enabled=false`，无数据源 |

`NodeAddress` 必须是调度端能访问到的地址；跨机部署时不要写 `localhost`。

## 3. 启动

```bash
# 先起服务端（第一次启动会播种三条示例任务定义）
dotnet run --project samples/SchedulerServer

# 再起执行端（它会在 10 秒内被心跳登记，管理面 workers 里出现该节点及其任务清单）
dotnet run --project samples/SchedulerWorker
```

## 4. 验证

```bash
TOKEN=findx-demo-token
BASE=http://localhost:7100

# 节点与能力清单：应看到 scheduler-worker-1 与三个 Job 全名
curl -s -H "X-Scheduler-Token: $TOKEN" $BASE/api/scheduler/admin/workers
curl -s -H "X-Scheduler-Token: $TOKEN" $BASE/api/scheduler/admin/capabilities

# 任务定义（三条 demo-*）
curl -s -H "X-Scheduler-Token: $TOKEN" $BASE/api/scheduler/admin/jobs

# 当前主节点
curl -s -H "X-Scheduler-Token: $TOKEN" $BASE/api/scheduler/admin/master

# 执行记录（默认不含过程日志）
curl -s -H "X-Scheduler-Token: $TOKEN" "$BASE/api/scheduler/admin/executions?jobId=1&limit=10"

# 单次执行详情（含结果消息 + 日志尾部摘要）
curl -s -H "X-Scheduler-Token: $TOKEN" "$BASE/api/scheduler/admin/executions/{traceId}"

# 过程日志（按主键游标分页）
curl -s -H "X-Scheduler-Token: $TOKEN" "$BASE/api/scheduler/admin/executions/{traceId}/logs?afterId=0&limit=100"

# 立即触发一次（管理面入参是 SchedulerJobDto，主键字段名是 id）
curl -s -X POST -H "X-Scheduler-Token: $TOKEN" -H "Content-Type: application/json" \
     -d '{"id":<jobId>}' $BASE/api/scheduler/admin/jobs/trigger

# 人工终止某次执行（traceId 从 executions 或 trigger 的返回里取）
curl -s -X POST -H "X-Scheduler-Token: $TOKEN" -H "Content-Type: application/json" \
     -d '{"traceId":"<traceId>"}' $BASE/api/scheduler/admin/executions/kill

# 死信放行（demo-flaky 进入 Dead 后恢复调度）
curl -s -X POST -H "X-Scheduler-Token: $TOKEN" -H "Content-Type: application/json" \
     -d '{"id":<jobId>}' $BASE/api/scheduler/admin/jobs/release
```

> 主键说明：`scheduler_*` 表的 `id` 列由 FreeSql 自动建出且**没有 AUTO_INCREMENT**，
> 组件所有插入路径都用框架 `IKeyGenerator<long>`（默认雪花）显式赋值，所以看到的 Id 是 19 位长整型，不是 1、2、3。

### 预期现象

- `demo-hello`：每 10 秒一次 `Succeeded`，`executions` 与 `execution_log` 都有记录，日志里能看到 3 步进度。
- `demo-flaky`：失败 → `RetryAtTime` 退避重试（MaxRetryCount=2）→ 任务被置 `Dead` 隔离；
  管理面 `POST jobs/release` 放行后又恢复调度。
- `demo-slow`：每 5 分钟执行 40 秒；执行中 `POST executions/kill` 后，执行端记 `Cancelled`，
  调度端账本同步终结为 `Cancelled`，不会因为后续回调被翻回重试。

## 5. 换成 RPC 时改哪里

执行端只认 `ISchedulerMasterChannel`（心跳 / 下线 / 结果 / 日志四条上行）。新增一个
`GrpcSchedulerMasterChannel` 实现，把 `SchedulerModule` 里 `WorkerEnabled` 段落的这一行注册替换掉即可：

```csharp
services.AddSingleton<ISchedulerMasterChannel, HttpSchedulerMasterChannel>();
```

执行队列、日志采集、结果上报均不需要改动。调度端接收面目前是 HTTP Controller，
若要上 gRPC 服务端，需要额外为 `/beat`、`/offline`、`/result`、`/logs` 四个入口补一份服务端适配。
