# Findx.Extensions.WorkflowCore

FlowLong（飞龙）风格的树形节点驱动审批流组件。流程定义是一棵 JSON 树，节点只认自己的后继（`childNode`），没有回溯连线；驳回、回退、复审回路通过"驳回 + 指定节点"或路由分支（`Route`）实现。核心是 8 张表的 JSON 模型 + 一个执行引擎，会签、或签、顺签、并行、包容、条件、路由、触发器、子流程都在同一套驱动里处理。

> Nuget 包引用

```
Findx.Extensions.WorkflowCore
```

目标框架：`net6.0` / `net8.0`，依赖 `Findx.AspNetCore` 与 `Findx.Module.EleAdminPlus.Shared`（当前用户模型取自 `IWorkContext`）。

> 本包无独立的 DI 扩展方法

通过 `StartupModule` 机制自动装配：

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 WorkflowCoreModule
```

`WorkflowCoreModule`（`ModuleLevel.Application`，`Order = 130`）注册：

```csharp
services.Configure<WorkflowCoreOptions>(configuration.GetSection("Findx:Workflow"));

services.AddScoped<FlwStore>();                        // 8 表读写
services.AddScoped<IFlowLongEngine, FlowLongEngine>(); // 执行引擎
services.AddScoped<FlwTimeoutHandler>();               // 提醒与超时处理

services.TryAddScoped<IFlwActorProvider, DefaultFlwActorProvider>();  // 办理人解析扩展点
services.TryAddScoped<IFlwTaskNotifier, DefaultFlwTaskNotifier>();    // 通知扩展点
```

两个 `TryAdd` 都可以被宿主 `Replace` 接管，不注册就用默认实现。

## Json 配置

```json
{
  "Findx": {
    "FreeSql": {
      "Enabled": true,
      "Primary": "Master",
      "Strict": true,
      "DataSource": {
        "Master": {
          "ConnectionString": "Server=.;Database=findx;Trusted_Connection=True;",
          "DbType": "SqlServer"
        },
        "Workflow": {
          "ConnectionString": "Server=.;Database=findx_flow;Trusted_Connection=True;",
          "DbType": "SqlServer",
          "NameConvertType": "None",
          "UseAutoSyncStructure": true
        }
      }
    },
    "Workflow": {
      "TimeoutEnabled": true,
      "TimeoutAction": "Remind",
      "ScanBatchSize": 200
    },
    "Jobs": {
      "Enabled": true,
      "ScheduleType": "Simple"
    }
  }
}
```

> 8 张表的实体都标了 `[EntityExtension(DataSource = "Workflow")]`

数据源键名必须是 `Workflow`，它对应 `Findx:FreeSql:DataSource` 下的同名键。`Strict = true` 时缺失该键会直接抛异常；`Strict = false` 会静默回落到 `Primary` 库，工作流表会和业务表混在一个库里，不建议。

> 组件配置（`Findx:Workflow`）

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `TimeoutEnabled` | `false` | 是否启用任务提醒与超时扫描。**默认关闭**，开启后才会改动任务数据 |
| `TimeoutAction` | `None` | 到期处理动作：`None` / `Remind` / `AutoApprove` / `AutoReject` |
| `ScanBatchSize` | `200` | 单次扫描任务数量上限 |

> 调度配置（`Findx:Jobs`）

| 配置项 | 说明 |
|---|---|
| `Enabled` | 是否启用 `Findx.Jobs`。为 `false` 时超时作业不会被调度，工作流本身仍可正常审批 |
| `ScheduleType` | `Simple` 走内存版调度，`Distributed` 走分布式调度。**组件里的 Job 类不需要区分**，同一个 `IJob` 实现两边通用，由宿主在这决定 |

`FlwTaskTimeoutJob` 打了 `[Job(Cron = "0 * * * * ?")]`，`JobFinder` 会扫所有已加载程序集里的 `IJob` 自动注册，**不需要手工 `services.AddTransient`**。

## 八张表

表名采用下划线，C# 属性与列名采用大驼峰（`NameConvertType = None`）。

| 表 | 说明 | 关键字段 |
|---|---|---|
| `flw_process` | 流程定义 | `Code` / `Name` / `Type` / `Version` / `Status`（0 不可用 / 1 可用 / 2 历史版本）/ `ModelContent`（JSON 树）/ `InstanceUrl` / `UseScope` |
| `flw_instance` | 运行中的流程实例 | `ProcessId` / `ProcessKey` / `ProcessName` / `InstanceState` / `ParentInstanceId` / `InstanceNo` / `BusinessCode` / `Variable`（变量 JSON）/ `CurrentNodeCode` / `CurrentNodeName` / `ExpireTime` |
| `flw_instance_his` | 实例归档 | 同活动实例，另加 `Status` / `EndTime` / `Duration`（耗时） |
| `flw_instance_ext` | 实例模型快照 | `InstanceId` / `ProcessId` / `ProcessName` / `ProcessType` / `ModelContent` |
| `flw_task` | 活动任务 | `InstanceId` / `ParentTaskId` / `TaskKey` / `TaskName` / `TaskType` / `PerformType` / `Variable` / `ExpireTime` / `RemindTime` / `RemindRepeat` / `Viewed` / `AssignorId` |
| `flw_task_his` | 任务归档 | 同活动任务，另加 `CallProcessId` / `CallInstanceId` / `FinishTime` / `TaskState` / `Duration` / `Comment` / `Action` |
| `flw_task_actor` | 任务参与者 | `TaskId` / `InstanceId` / `ActorId` / `ActorName` / `ActorType`（0 用户 / 1 角色 / 2 部门）/ `Weight` / `AgentId` |
| `flw_task_actor_his` | 参与者归档 | 结构同活动表，`FlwHisTaskActorInfo` 直接继承 `FlwTaskActorInfo` |

> 三个要点

1. `flw_instance_ext` 是**发起时复制的一份模型快照**，之后修改流程定义不影响在途实例，这是"模型版本化"的关键
2. 任务办结时活动表记录**移到**历史表，活动表不再有该任务，所以"活动任务 = 待办"可以直接查 `flw_task`
3. `flw_task_actor_his` 保留办理痕迹，转办/加签都能追溯

## 流程模型

模型 JSON 结构：

```json
{
  "processKey": "leave",
  "processName": "请假审批",
  "root": {
    "nodeKey": "root",
    "nodeName": "发起人",
    "type": 0,
    "childNode": { },
    "conditionNodes": [ ],
    "parallelNodes": [ ]
  }
}
```

- `childNode`：唯一后继节点
- `conditionNodes`：条件分支的子分支列表（条件 / 路由使用）
- `parallelNodes`：并行 / 包容分支的子分支列表

### 节点类型

| 类型 | 值 | 行为 |
|---|---|---|
| `Root` | 0 | 发起节点，模型根 |
| `Approval` | 1 | 审批节点，创建任务并**阻塞**，等办理人处理 |
| `CarbonCopy` | 2 | 抄送节点，创建任务但**不阻塞**，直接走后继 |
| `Condition` | 3 | 条件分支，走第一个表达式成立的分支，必须有默认分支 |
| `Parallel` | 4 | 并行分支，不判条件，所有分支都走，汇聚时等全部到达 |
| `Inclusive` | 5 | 包容分支，只走成立的分支，汇聚时只等被选中的分支 |
| `Route` | 6 | 路由分支，按条件把流程**重定向**到 `targetNodeKey`，用于驳回返工、复审回路 |
| `Trigger` | 7 | 触发器，创建挂起任务，等外部回调或人工完成 |
| `CallProcess` | 8 | 子流程，启动子实例并挂起父实例，子流程通过后自动唤醒 |

### props

| 字段 | 类型 | 说明 |
|---|---|---|
| `assignee` | 数组 | 办理人，元素 `{ type, id, name, weight }` |
| `performType` | 数字 | 参与类型：`0` 单人 / `1` 会签 / `2` 或签 / `3` 顺签 |
| `expireDay` | 数字 | 期望完成天数，`0` 表示不限 |
| `remindDay` | 数字 | 到期前提前提醒天数，`0` 表示不提醒 |
| `remindRepeat` | 数字 | 提醒重复间隔（分钟），`0` 表示只提醒一次 |
| `expression` | 字符串 | 分支条件表达式（条件 / 包容 / 路由分支使用） |
| `targetNodeKey` | 字符串 | 路由分支成立后的目标节点 key |
| `autoApprove` | 布尔 | 无办理人时是否自动通过 |
| `triggerType` / `triggerArgs` | 字符串 | 触发器类型与参数 |
| `delayTime` | 字符串 | 延时时长，格式如 `7:d`、`30:m` |
| `callProcessKey` | 字符串 | 子流程编码，为空校验不通过 |
| `extend` | 对象 | 前端设计器保留字段，引擎不解释 |

### 办理人

| `type` | 说明 |
|---|---|
| `user` | 直接指定用户，`id` 为用户 ID |
| `creator` | 流程发起人本人 |
| `role` | 角色，`id` 为角色编码，匹配时用 `FlwCreator.RoleIds` |
| `dept` | 部门，`id` 为部门 ID，匹配时用 `FlwCreator.DeptId` |
| `superior` | 上级领导。**依赖组织架构，默认实现会跳过并告警**，需要宿主实现 `IFlwActorProvider` |

### 条件表达式

| 能力 | 写法 |
|---|---|
| 逻辑 | `&&` / `||` / `!`，可用括号改变优先级 |
| 比较 | `==` `!=` `>` `>=` `<` `<=` |
| 字符串包含 | `contains` / `not contains`，如 `reason contains '年假'` |
| 空值判断 | `empty(x)` / `notEmpty(x)` |
| 变量引用 | `days` 或 `${days}` |
| 字面量 | 数字、`true` / `false`、单双引号字符串 |

> 三条硬规则

1. 括号子表达式会递归回顶层求值，`(a || b) && c` 能正确解析
2. **裸标识符必须是变量**，变量缺失视为 `null`；字符串字面量必须加引号，否则 `missing` 这种写法会被当成布尔真
3. 数值一律按 `decimal` 比较，避免浮点误差

### 校验规则

`POST /api/flw/model/publish` 和引擎 `StartAsync` 都会走同一套校验，任一条不满足直接抛 `FlwModelException`：

- `root` 必须存在且 `type = 0`
- 所有节点 `nodeKey` 非空且**全局唯一**
- `type = 1` 的审批节点必须有 `assignee`，或显式 `autoApprove = true`
- 条件分支容器必须包含**至少一个 `expression` 为空的默认分支**
- 路由分支（带 `expression` 的）必须配置 `targetNodeKey`，且目标节点必须存在于模型中
- `type = 8` 的子流程节点必须配置 `callProcessKey`

### 完整示例

`Samples/leave-flow.json`：

```json
{
  "processKey": "leave",
  "processName": "请假审批",
  "root": {
    "nodeKey": "root", "nodeName": "发起人", "type": 0,
    "childNode": {
      "nodeKey": "leaderApprove", "nodeName": "直属主管审批", "type": 1,
      "props": {
        "assignee": [ { "type": "user", "id": "1001", "name": "张主管" } ],
        "performType": 0, "expireDay": 1, "remindDay": 1
      },
      "childNode": {
        "nodeKey": "daysJudge", "nodeName": "请假天数判断", "type": 3,
        "childNode": {
          "nodeKey": "hrCopy", "nodeName": "人事抄送", "type": 2,
          "props": { "assignee": [ { "type": "dept", "id": "2001", "name": "人事部" } ] }
        },
        "conditionNodes": [
          {
            "nodeKey": "daysSmall", "nodeName": "三天以内", "type": 3,
            "props": { "expression": "days <= 3" },
            "childNode": {
              "nodeKey": "hrRecord", "nodeName": "人事备案", "type": 1,
              "props": { "assignee": [ { "type": "role", "id": "hr", "name": "人事角色" } ] }
            }
          },
          {
            "nodeKey": "daysLarge", "nodeName": "超过三天", "type": 3, "props": {},
            "childNode": {
              "nodeKey": "gmCountersign", "nodeName": "总经理会签", "type": 1,
              "props": {
                "assignee": [
                  { "type": "user", "id": "1002", "name": "李总" },
                  { "type": "user", "id": "1003", "name": "王总" }
                ],
                "performType": 1, "expireDay": 2
              }
            }
          }
        ]
      }
    }
  }
}
```

注意：`daysLarge` 这个分支没有 `expression`，它就是默认分支，任何不满足 `days <= 3` 的实例都走这里。

## 引擎 API

```csharp
public interface IFlowLongEngine
{
    Task<FlwInstanceInfo> StartAsync(FlwStartArgs args, CancellationToken ct = default);

    Task ApproveAsync(long taskId, FlwCreator actor, string comment = null, Dictionary<string, object> variables = null, CancellationToken ct = default);
    Task RejectAsync(long taskId, FlwCreator actor, string comment = null, string targetNodeKey = null, bool endProcess = false, CancellationToken ct = default);
    Task RevokeAsync(long instanceId, FlwCreator actor, string comment = null, CancellationToken ct = default);
    Task TransferAsync(long taskId, FlwCreator actor, FlwCreator target, CancellationToken ct = default);
    Task AddSignAsync(long taskId, FlwCreator actor, List<FlwActorModel> actors, CancellationToken ct = default);
    Task JumpAsync(long taskId, FlwCreator actor, string targetNodeKey, string comment = null, CancellationToken ct = default);
    Task CompleteTriggerAsync(long taskId, FlwCreator actor, string comment = null, CancellationToken ct = default);

    Task<List<FlwTaskInfo>> GetTodoTasksAsync(FlwCreator actor, int top = 100, CancellationToken ct = default);
    Task<List<FlwHisTaskInfo>> GetDoneTasksAsync(FlwCreator actor, int top = 100, CancellationToken ct = default);
    Task<FlwInstanceInfo> GetInstanceAsync(long instanceId, CancellationToken ct = default);
    Task<FlwInstanceDetail> GetInstanceDetailAsync(long instanceId, CancellationToken ct = default);
}
```

> 发起 → 办结

```csharp
var engine = serviceProvider.GetRequiredService<IFlowLongEngine>();

//  发起：processKey 与 processId 二选一，variables 供条件分支使用
var instance = await engine.StartAsync(new FlwStartArgs
{
    ProcessKey = "leave",
    BusinessKey = "LV-2026-0001",
    Creator = new FlwCreator("1001", "张三"),
    Variables = new Dictionary<string, object> { ["days"] = 5, ["reason"] = "年假返乡" }
});

//  办结：会签 / 或签 / 顺签由节点 performType 决定，调用方只管"我同意了"
await engine.ApproveAsync(taskId, new FlwCreator("1002", "李总"), "同意", new Dictionary<string, object> { ["amount"] = 1200 });

//  驳回：不指定目标节点则回退上一审批节点；endProcess = true 直接结束为"拒绝"
await engine.RejectAsync(taskId, new FlwCreator("1002", "李总"), "金额需重新核算", targetNodeKey: null, endProcess: false);
```

`StartAsync` 的 `BusinessKey` 会落到 `flw_instance.BusinessCode`（DTO 与实体字段名不同，对接时注意）。

## REST API

控制器统一 `[Area("flw")]`，路由前缀 `/api/flw`。当前操作人从 `IWorkContext.ContextUser` / `ICurrentUser` 取，不需要前端传。

| 方法 | 路径 | 说明 |
|---|---|---|
| GET | `/api/flw/model/page` | 流程定义分页 |
| POST | `/api/flw/model/add` | 新增定义（含 `modelContent` 之外的元数据） |
| POST | `/api/flw/model/edit` | 编辑定义 |
| GET | `/api/flw/model/detail?id=` | 详情（含完整 `ModelContent`） |
| POST | `/api/flw/model/publish?id=` | 发布：校验模型 JSON，同编码旧版本置 `Status = 2` |
| POST | `/api/flw/instance/start` | 发起流程 |
| POST | `/api/flw/instance/revoke` | 撤销（仅发起人） |
| GET | `/api/flw/instance/detail?instanceId=` | 实例详情（活动任务 + 历史任务） |
| GET | `/api/flw/instance/page` | 实例分页 |
| GET | `/api/flw/task/todo?top=100` | 我的待办 |
| GET | `/api/flw/task/done?top=100` | 我的已办 |
| POST | `/api/flw/task/approve` | 同意 |
| POST | `/api/flw/task/reject` | 拒绝 / 驳回 |
| POST | `/api/flw/task/transfer` | 转办 |
| POST | `/api/flw/task/addSign` | 加签 |
| POST | `/api/flw/task/jump` | 跳转到指定节点 |
| POST | `/api/flw/task/completeTrigger` | 完成触发器 / 子流程挂起任务 |

> 发起、同意

```bash
curl -X POST http://localhost:8000/api/flw/instance/start \
  -H "Content-Type: application/json" \
  -d '{"processKey":"leave","businessKey":"LV-2026-0001","variables":{"days":5,"reason":"年假"}}'

curl -X POST http://localhost:8000/api/flw/task/approve \
  -H "Content-Type: application/json" \
  -d '{"taskId":1024,"comment":"同意","variables":{"amount":1200}}'
```

> 发布流程定义

```bash
# 先 add 或 edit 写入 ModelContent，再按 id 发布
curl -X POST "http://localhost:8000/api/flw/model/publish?id=1"
```

发布时会做完整模型校验，模型不合法直接返回失败原因（如"条件分支节点[请假天数判断]缺少默认分支"），不会写库。

## 执行语义

### 发起与推进

```
StartAsync
  ├─ 取流程定义 → 解析模型 → 校验
  ├─ 写 flw_instance + flw_instance_his + flw_instance_ext（模型快照）
  └─ ExecuteNodeAsync(root.childNode)
        Approval      → 解析办理人 → 建任务 + 参与者 → 停
        CarbonCopy    → 建任务 → 继续 childNode（不阻塞）
        Condition     → 求值选中分支 → 进分支首节点
        Parallel      → 选中全部分支 → 逐个进首节点（记 __branches__）
        Inclusive     → 选中成立的分支 → 逐个进首节点（记 __branches__）
        Route         → 命中分支的 targetNodeKey → 跳转
        Trigger       → 建挂起任务 → 停
        CallProcess   → 启动子实例 → 父实例挂 Trigger 任务 → 停
```

一次流程操作（发起、办结、驳回、跳转等）的 8 表写入共享同一个 `IUnitOfWork` 事务，中途抛异常整体回滚，不会出现"任务归档了但实例没推进"的中间态。

### 会签 / 或签 / 顺签

| `performType` | 建任务 | 推进条件 |
|---|---|---|
| `0` 单人 | 一个任务 | 该任务办结即推进 |
| `1` 会签 | 每人一个任务 | 同节点所有活动任务都办结才推进 |
| `2` 或签 | 每人一个任务 | 任一办结即推进，其余任务自动归档为完成 |
| `3` 顺签 | **只给第一人建任务**，其余排队 | 每办结一人就给下一位建任务，最后一人办结才推进 |

顺签的排队人员序列化在任务变量 `__sequence_actors__` 里，`FlwStore.SetSequenceActorsAsync / GetSequenceActors` 负责存取，所以中途转办、加签不会打乱队列顺序。

### 并行 / 包容汇聚

进入分支时把"被选中的分支根列表"写进实例变量 `__branches__{分支节点key}`；任务办到分支末端时把已完成的分支根累加进 `__join__{分支节点key}`，两个集合覆盖后才进入汇聚节点一次，避免多分支同时推进导致重复触发。

节点在解析阶段就标了 `BranchParentKey`（所属分支节点）与 `BranchRootKey`（所属分支根），引擎靠这两个标记判断"我在哪个分支里、是不是最后一人"。

### 异常路径

| 动作 | 行为 |
|---|---|
| 驳回（`endProcess = false`） | 归档同节点全部任务为 `Rejected`；从历史任务里找最近一个**不同 nodeKey** 的审批节点作为回退目标；找不到则结束实例为"拒绝" |
| 驳回（`endProcess = true`） | 直接结束实例，状态 `Rejected` |
| 撤销 | 仅发起人可撤销（比对 `CreatorId`）；全部活动任务归档为 `Revoked`，实例结束 |
| 转办 | 旧参与者写历史留痕，替换为目标用户，任务记 `Assignor` |
| 加签 | 往当前任务追加参与者；会签语义下需都办完才推进 |
| 跳转 | 当前节点任务归档为 `Jump`，直接从 `targetNodeKey` 继续 |
| 子流程 | 父实例挂 `Trigger` 任务；子流程实例通过后自动唤醒父实例继续 |
| 触发器回调 | 外部调 `CompleteTriggerAsync` 后继续 `childNode` |

### 状态枚举

任务状态（`FlwTaskState`）：`Active 0` / `Jump 1` / `Completed 2` / `Rejected 3` / `Revoked 4` / `Timeout 5` / `Terminated 6` / `RejectTerminated 7`

实例状态（`FlwInstanceState`）：`Running 0` / `Approved 1` / `Rejected 2` / `Revoked 3` / `Timeout 4` / `Terminated 5`

任务动作（`FlwTaskAction`）：`Agree` / `Reject` / `Revoke` / `Transfer` / `AddSign` / `ReduceSign` / `Jump` / `AutoComplete`

## 超时提醒与超时处理

节点 `props` 配 `expireDay` + `remindDay` + `remindRepeat`，创建任务时写入 `flw_task.ExpireTime` / `RemindTime` / `RemindRepeat`：

```json
"props": {
  "assignee": [ { "type": "user", "id": "1001", "name": "张主管" } ],
  "expireDay": 3,
  "remindDay": 1,
  "remindRepeat": 60
}
```

- `ExpireTime = 现在 + expireDay 天`
- `RemindTime = 到期前 remindDay 天`，若该时点已过则**不提醒**（避免流程一发起就弹提醒）
- `remindRepeat > 0` 且未越过到期时间时，按分钟继续提醒；否则只提醒一次

`FlwTaskTimeoutJob` 每分钟扫一次（`Cron = "0 * * * * ?"`），两类处理：

| 场景 | 处理 |
|---|---|
| `RemindTime <= now` | 调 `IFlwTaskNotifier` 通知办理人，再推进下一次提醒时间 |
| `ExpireTime <= now` | 按 `TimeoutAction`：`Remind` 只通知；`AutoApprove` / `AutoReject` 以任务上的**用户办理人**名义调引擎办结，意见写"系统超时自动通过/拒绝" |

处理完会清空 `ExpireTime` / `RemindTime` / `RemindRepeat`，避免下一轮重复触发；实例已结束但任务残留的，直接清时限不再处理。

> 安全默认

- `TimeoutEnabled` 默认 `false`，不配就只注册不动作
- 办理人只有角色/部门、没有具体用户时**跳过并告警**，不会拿错人代签
- 超时动作抛 `FlwEngineException` 时记录告警并跳过，不影响其它任务

> 用内存还是分布式调度

组件只在 `Jobs/FlwTaskTimeoutJob.cs` 里实现 `IJob`，不关心调度实现。宿主配 `Findx:Jobs:ScheduleType`：

- `Simple`：走 `Findx.Jobs` 内存版（单机、无外部依赖）
- `Distributed`：走分布式调度组件（多实例下同一作业只跑一次）

## 扩展点

### 办理人解析（`IFlwActorProvider`）

各平台的组织模型不同，"取上级""取部门负责人"没法在组件里硬编码，所以抽成接口：

```csharp
public interface IFlwActorProvider
{
    Task<List<FlwTaskActorInfo>> ResolveAsync(FlwActorResolveContext context, CancellationToken ct = default);
}

public class FlwActorResolveContext
{
    public FlwInstanceInfo Instance { get; set; }
    public FlwNodeModel Node { get; set; }
    public FlwCreator Creator { get; set; }
}
```

宿主实现示例（把 `superior` 解析成发起人的直属上级，其余类型沿用默认实现）：

```csharp
public class MyFlwActorProvider : IFlwActorProvider
{
    private readonly IOrgService _orgService;
    private readonly DefaultFlwActorProvider _default;

    public MyFlwActorProvider(IOrgService orgService, DefaultFlwActorProvider defaultProvider)
    {
        _orgService = orgService;
        _default = defaultProvider;
    }

    public async Task<List<FlwTaskActorInfo>> ResolveAsync(FlwActorResolveContext context, CancellationToken ct = default)
    {
        var assignees = context.Node.Props?.Assignee ?? [];

        //  没有 superior 时整体交给默认实现
        if (assignees.All(x => x.Type != "superior"))
            return await _default.ResolveAsync(context, ct);

        var result = new List<FlwTaskActorInfo>();
        foreach (var actor in assignees)
        {
            if (actor.Type != "superior")
            {
                //  user / creator / role / dept 的映射与默认实现一致，示例按 user 处理
                result.Add(new FlwTaskActorInfo { ActorId = actor.Id, ActorName = actor.Name, ActorType = FlwActorType.User });
                continue;
            }

            var leader = await _orgService.GetLeaderAsync(context.Creator.Id, ct);
            if (leader != null)
                result.Add(new FlwTaskActorInfo { ActorId = leader.Id, ActorName = leader.Name, ActorType = FlwActorType.User });
        }
        return result;
    }
}

//  注册（在 AddModules 之后 Replace，否则会被 TryAdd 的默认实现先占位）
services.Replace<IFlwActorProvider, MyFlwActorProvider>(ServiceLifetime.Scoped);
```

默认实现 `DefaultFlwActorProvider` 支持 `user` / `creator` / `role` / `dept`，遇到 `superior` 会 `LogWarning` 并跳过。

### 通知下发（`IFlwTaskNotifier`）

```csharp
public interface IFlwTaskNotifier
{
    Task NotifyAsync(FlwTaskNotifyContext context, CancellationToken ct = default);
}
```

`FlwTaskNotifyContext` 带 `Instance` / `Task` / `Actors` / `Expired`（`true` 表示已超时，`false` 表示临近到期提醒）。默认实现只写日志；接站内信、企微、邮件时替换即可：

```csharp
services.Replace<IFlwTaskNotifier, MyWeComNotifier>(ServiceLifetime.Scoped);
```

### 数据源

组件不自己创建 `IFreeSql`，全部走 `IRepository<T, long>`，连接由 `Findx:FreeSql` 决定。8 张表的归属由 `Findx:Workflow:DataSource` 决定，模块启动时写入实体的 `EntityExtension.DataSource`（实体类上不写死，换库只改配置）：

```jsonc
{
  "Findx": {
    "FreeSql": {
      "Primary": "BunSmart",
      "DataSource": {
        "BunSmart": {
          //  关键：把 workflow 声明为主库的别名，二者共用同一个 IFreeSql
          "DataSourceSharing": [ "system", "workflow" ]
        }
      }
    },
    "Workflow": { "DataSource": "workflow" }
  }
}
```

这样「工作流表有独立标识」和「与业务单据同一个事务」两个目标可以同时满足：

- `DataSourceSharing` 让 `freeSqlClient["workflow"]` 与 `freeSqlClient["BunSmart"]` 指向**同一个 `IFreeSql` 实例**（同一个连接池）
- 框架侧 `UnitOfWorkManager.NormalizePrimary` 会把共享同一实例的别名键归一到真实数据源键，所以两者拿到的是**同一个工作单元、同一个事务**
- 将来真要把 flw 表拆到独立库：去掉别名，改成一条真实的 `workflow` 数据源配置即可，**代码不用动**。但要接受「审批与业务单据分属两个事务」，业务侧需自行补偿

不配 `Findx:Workflow:DataSource` 时跟随主数据源，行为与单库部署完全一致。

### 主键

8 张表的 `Id` 是 `long`，一律由 `IKeyGenerator<long>`（雪花）生成，**表结构不带 `AUTO_INCREMENT`**：

```csharp
// 引擎内部：FlwStore 注入 IKeyGenerator<long>，插入前补主键
public long NewKey() => _keyGenerator.Create();

// 业务侧直接写 FlwProcessInfo 等实体时
model.SetEmptyKey();          // 等价于 Id == 0 时调 IKeyGenerator<long>.Create()
await repo.InsertAsync(model, ct);
```

注意 `IRepository.InsertAsync` **不会**自动补主键（只有 `Save` 路径会走 `SetEmptyKey`），所有插入前都要显式生成，否则落库 Id 恒为 0。实体上的 `Id` 直接继承 `EntityBase<long>` 的虚属性，**不要在子类里 `override`**——FreeSql 按声明所在类型解析主键元数据，重写会导致主键映射丢失。

## 业务单据外挂

引擎不认识业务单据，也不该认识。要让审批挂到已有单据上，只需实现一个适配器并注册进 DI：

```csharp
public class PurchaseOrderFlowAdapter : IFlwBusinessAdapter
{
    public string BusinessType => "pms.order";

    public async Task<FlwBusinessInfo> LoadAsync(long businessId, CancellationToken ct)
    {
        var order = await _orderRepo.GetAsync(businessId, ct);
        if (order == null) return null;
        return new FlwBusinessInfo
        {
            BusinessType = BusinessType, BusinessId = businessId,
            BusinessCode = order.OrderCode, Title = order.SupplierName,
            Url = $"/pms/order/detail?id={businessId}"
        };
    }

    public async Task<bool> OnBeforeStartAsync(FlwBusinessContext ctx, CancellationToken ct)
        => ctx.Business != null;   //  单据不存在 / 已被占用就返回 false，发起直接回滚

    public async Task OnInstanceStateChangedAsync(FlwBusinessContext ctx, FlwInstanceState state, CancellationToken ct)
    {
        //  与流程推进同一事务：这里落库单据状态，要么全成要么全败
        var order = await _orderRepo.WithUnitOfWork(ctx.UnitOfWork).GetAsync(ctx.Business.BusinessId, ct);
        order.ApprovalStatus = state switch
        {
            FlwInstanceState.Running => 3,     //  待审核
            FlwInstanceState.Approved => 1,    //  已审核
            FlwInstanceState.Rejected => 2,    //  已拒绝
            FlwInstanceState.Revoked => 0,     //  回到草稿
            _ => 5                             //  终止/超时 → 作废
        };
        await _orderRepo.WithUnitOfWork(ctx.UnitOfWork).UpdateAsync(order, ct);
    }
}
```

注册与绑定：

```csharp
services.AddScoped<IFlwBusinessAdapter, PurchaseOrderFlowAdapter>();   //  宿主模块 ConfigureServices
```

流程定义上把 `businessType` 设为 `pms.order`，之后按业务类型就能找到流程、找到实例、找到待办。

> 适配器找不到时按"无业务绑定"处理，流程照跑、不回调业务——**外挂的含义就是不注册也能用**

三类集成模式共用同一套契约：

| 模式 | 数据在哪 | 典型场景 |
|---|---|---|
| 外挂型 | 业务表 | 采购单、销售单、出入库单 |
| 内生型 | 表单数据表 | 请假、报销、用章（配合 `formSchema` 动态表单） |
| 混合型 | 业务表 + 补充字段 | 变更单（业务字段 + 审批意见） |

### 提交后队列与嵌套事务

引擎内部会开工作单元（`GetEntityUnitOfWorkAsync<T>(true)`）。宿主的审核接口如果已经开了工作单元，`ScopedDictionary` 会复用同一个实例，`UnitOfWorkBase._transactionStack` 让内层 `CommitAsync` 只弹栈不落库——**所以业务回写与流程推进天然同事务**。

前提是两边解析到**同一个工作单元键**。工作流表走独立标识（`workflow`）时，靠 `DataSourceSharing` 别名 + `UnitOfWorkManager.NormalizePrimary` 归一，否则会各开一个连接、各开一个事务，上面这句话就不成立。

回调分两层，不要混用：

| 扩展点 | 执行时机 | 失败影响 | 用途 |
|---|---|---|---|
| `IFlwBusinessAdapter` | 引擎事务内 | 回滚整个操作 | 单据状态回写、库存冻结等强一致动作 |
| `IFlwInstanceListener` / `IFlwTaskNotifier` | 提交后队列 | 只记日志 | 站内信、企微推送、刷新缓存 |

> 注意：宿主自建外层事务时，引擎的提交是嵌套提交，提交后队列会在引擎方法返回时执行，可能早于外层真正提交。需要严格"提交后"语义的场景请用 `IFlwBusinessAdapter` 的同步回调。

## 审批组件

面向业务页面的聚合接口，一个 `panel` 即可渲染发起态 / 审批中 / 已完结三种界面：

| 方法 | 路径 | 说明 |
|---|---|---|
| GET | `/api/flw/component/panel?businessType=&businessId=` | 状态 + 时间轴 + 可用动作 + 我的待办 |
| GET | `/api/flw/component/count` | 待办 / 已办角标 |
| POST | `/api/flw/component/submit` | 发起（`processKey` 可省，按业务类型匹配已发布流程） |
| POST | `/api/flw/component/action` | `approve / reject / transfer / addSign / jump` |
| POST | `/api/flw/component/revoke` | 撤销（仅发起人） |
| POST | `/api/flw/instance/terminate` | 强制终止（管理员/业务作废） |

`panel` 返回结构（节选）：

```json
{
  "hasProcess": true, "started": true,
  "instanceId": 1024, "instanceNo": "FLW202611150001",
  "instanceState": 0, "currentNodeName": "财务审核",
  "actions": ["approve", "reject", "transfer"],
  "myTaskId": 2048,
  "timeline": [
    { "nodeKey": "leader", "nodeName": "主管审批", "state": 2, "total": 1, "done": 1,
      "actors": [{ "actorName": "王五", "action": 0, "comment": "同意", "time": "2026-11-15 10:12:00" }] },
    { "nodeKey": "finance", "nodeName": "财务会签", "state": 1, "total": 3, "done": 2,
      "actors": [{ "actorName": "李四", "action": 0 }, { "actorName": "赵六", "action": 0 }] }
  ]
}
```

三个设计要点：

1. **时间轴是节点维度**，会签节点的多个任务按 `nodeKey` 合并，UI 直接显示 `2/3`
2. **`actions` 由后端算**，前端不做权限判断，避免两端各写一套
3. **未发起也返回合法结构**（`instanceState = -1` + 节点预览），组件不需要额外分支

`timeline[].state` 取值见 `FlwTimelineNodeState`：0 未开始 / 1 处理中 / 2 已通过 / 3 已驳回 / 4 已跳过 / 5 抄送。

## 性能与模型缓存

当前实现的模型解析时机：

| 场景 | 解析次数 |
|---|---|
| 发起流程 | 1 次（`StartAsync` 内 `FlwModelParser.Parse`） |
| 任务办结 | 1 次（从 `flw_instance_ext.ModelContent` 解析） |
| 查询待办 / 已办 | 0 次（只查表） |

FlowLong 官方额外提供了 `ProcessModelCache` + `FlowCache`（默认 `ConcurrentHashMap`）来避免重复解析 JSON，并支持 Redis 接管。本组件暂未做模型缓存：单个模型 JSON 约几 KB，解析开销在毫秒级，正常审批场景可接受。若出现高频审批压测瓶颈，可以接 `Findx.Caching` 做一个 `IFlwModelCache`，按 `instanceId` 缓存解析结果，并在发布新版本时失效。

## 当前边界

| 项 | 状态 |
|---|---|
| 会签 / 或签 / 顺签 | 已实现 |
| 并行 / 包容 / 条件 / 路由分支 | 已实现 |
| 驳回、撤销、转办、加签、跳转 | 已实现 |
| 子流程、触发器回调 | 已实现 |
| 超时提醒 / 超时自动通过 / 自动拒绝 | 已实现（Job 每分钟扫描） |
| 票签权重计票 | **未实现**：`Weight` 已存储，会签判定是"全部办完"，不是按权重过半 |
| 业务单据外挂 | 已实现：`IFlwBusinessAdapter` + `FlwBusinessAdapterRegistry`，按 `businessType` 绑定 |
| 审批组件 | 已实现：`/api/flw/component/{panel,count,submit,action,revoke}` |
| 外置表单 | **部分实现**：流程级 `formSchema` 已支持下发；节点级覆盖、表单校验、表单数据落库未做（数据交给宿主） |
| 实例级强制终止 | 已实现：`TerminateAsync` + `/api/flw/instance/terminate`；**挂起**仍未实现 |
| 任务监听器 / 事件回调 | 已实现：`IFlwBusinessAdapter`（事务内同步）+ `IFlwInstanceListener` / `IFlwTaskNotifier`（提交后队列） |
| 模型缓存 | **未实现**（见上节） |

## 接入检查清单

1. 数据源：不配 `Findx:Workflow:DataSource` 则跟随主库；要独立标识就用 `DataSourceSharing` 别名（见上节），别直接配一个真实数据源，否则审批与单据会分成两个事务
2. 首次启动开 `UseAutoSyncStructure`，8 张表建好后关掉，改由 DDL 脚本管理；`Id` 列是 `bigint` 非自增，主键由 `IKeyGenerator<long>` 生成
3. `WorkflowCoreModule` 通过 `AddModules` 自动装配，不要手工注册控制器
4. `[Area("flw")]` 的权限交给宿主的功能权限过滤器，路由前缀是 `/api/flw`
5. 需要 `superior` 取人，实现 `IFlwActorProvider` 并 `Replace`
6. 需要站内信/企微提醒，实现 `IFlwTaskNotifier` 并 `Replace`
7. 要跑超时扫描：`Findx:Jobs:Enabled = true` + `Findx:Workflow:TimeoutEnabled = true`，并按业务选 `TimeoutAction`
8. 流程定义发布前先在测试库跑一遍 `POST /api/flw/model/publish`，模式校验会把你漏配的默认分支、重复 `nodeKey`、审批人缺失直接指出来
