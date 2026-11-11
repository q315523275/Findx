using Findx.Extensions.WorkflowCore.Enums;
using Findx.Extensions.WorkflowCore.Models;
using Findx.Extensions.WorkflowCore.Engine;
using Findx.Extensions.WorkflowCore.Options;
using Findx.Extensions.WorkflowCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Findx.Extensions.WorkflowCore.Services;

/// <summary>
///     任务提醒与超时处理
///     <para>由 FlwTaskTimeoutJob 周期调用；提醒渠道走 IFlwTaskNotifier，超时动作由配置决定</para>
/// </summary>
public class FlwTimeoutHandler
{
    private readonly FlwStore _store;
    private readonly IFlowLongEngine _engine;
    private readonly IFlwTaskNotifier _notifier;
    private readonly WorkflowCoreOptions _options;
    private readonly ILogger<FlwTimeoutHandler> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public FlwTimeoutHandler(FlwStore store, IFlowLongEngine engine, IFlwTaskNotifier notifier,
        IOptions<WorkflowCoreOptions> options, ILogger<FlwTimeoutHandler> logger)
    {
        _store = store;
        _engine = engine;
        _notifier = notifier;
        _options = options.Value ?? new WorkflowCoreOptions();
        _logger = logger;
    }

    /// <summary>
    ///     执行一次扫描
    /// </summary>
    public async Task<FlwTimeoutResult> HandleAsync(CancellationToken cancellationToken = default)
    {
        var result = new FlwTimeoutResult();
        if (!_options.TimeoutEnabled) return result;

        var now = DateTime.Now;

        //  1. 临近到期提醒
        var remindTasks = await _store.QueryRemindTasksAsync(now, _options.ScanBatchSize, cancellationToken);
        foreach (var task in remindTasks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instance = await _store.GetInstanceAsync(task.InstanceId, cancellationToken);
            if (instance == null || instance.InstanceState != FlwInstanceState.Running) continue;

            var actors = await _store.GetActorsAsync(task.Id, cancellationToken);
            await _notifier.NotifyAsync(new FlwTaskNotifyContext { Instance = instance, Task = task, Actors = actors, Expired = false }, cancellationToken);

            await _store.UpdateRemindAsync(task, ComputeNextRemind(now, task), cancellationToken);
            result.Reminded++;
        }

        //  2. 到期处理
        if (_options.TimeoutAction == FlwTimeoutAction.None) return result;

        var expiredTasks = await _store.QueryExpiredTasksAsync(now, _options.ScanBatchSize, cancellationToken);
        foreach (var task in expiredTasks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instance = await _store.GetInstanceAsync(task.InstanceId, cancellationToken);
            if (instance == null || instance.InstanceState != FlwInstanceState.Running)
            {
                //  实例已结束但任务残留，清掉时限避免反复扫到
                await _store.ClearTimeoutAsync(task, cancellationToken);
                continue;
            }

            var actors = await _store.GetActorsAsync(task.Id, cancellationToken);
            try
            {
                switch (_options.TimeoutAction)
                {
                    case FlwTimeoutAction.Remind:
                        await _notifier.NotifyAsync(new FlwTaskNotifyContext { Instance = instance, Task = task, Actors = actors, Expired = true }, cancellationToken);
                        break;

                    case FlwTimeoutAction.AutoApprove:
                    case FlwTimeoutAction.AutoReject:
                        var assignee = PickAssignableActor(actors);
                        if (assignee == null)
                        {
                            //  角色/部门办理人没有具体用户可代签，跳过并保留时限，等宿主 provider 或人工处理
                            _logger.LogWarning("工作流任务[{TaskId}]超时但未找到可代签的用户办理人，跳过自动处理", task.Id);
                            result.Skipped++;
                            continue;
                        }
                        if (_options.TimeoutAction == FlwTimeoutAction.AutoApprove)
                            await _engine.ApproveAsync(task.Id, assignee, "系统超时自动通过", null, cancellationToken);
                        else
                            await _engine.RejectAsync(task.Id, assignee, "系统超时自动拒绝", null, true, cancellationToken);
                        break;
                }

                await _store.ClearTimeoutAsync(task, cancellationToken);
                result.TimeoutHandled++;
            }
            catch (FlwEngineException exception)
            {
                _logger.LogWarning(exception, "工作流任务[{TaskId}]超时处理失败", task.Id);
                result.Skipped++;
            }
        }

        return result;
    }

    /// <summary>
    ///     下一次提醒时间：配置了重复间隔且未越过到期时间时继续提醒，否则停止
    /// </summary>
    private static DateTime? ComputeNextRemind(DateTime now, Entity.FlwTaskInfo task)
    {
        if (task.RemindRepeat <= 0 || task.ExpireTime == null) return null;

        var next = now.AddMinutes(task.RemindRepeat);
        return next < task.ExpireTime.Value ? next : null;
    }

    /// <summary>
    ///     挑一个可代签的用户办理人（角色/部门无法确定具体人）
    /// </summary>
    private static FlwCreator PickAssignableActor(List<Entity.FlwTaskActorInfo> actors)
    {
        var actor = actors?.FirstOrDefault(x => x.ActorType == FlwActorType.User && !string.IsNullOrWhiteSpace(x.ActorId));
        return actor == null ? null : new FlwCreator(actor.ActorId, actor.ActorName);
    }
}

/// <summary>
///     扫描结果
/// </summary>
public class FlwTimeoutResult
{
    /// <summary>
    ///     提醒数量
    /// </summary>
    public int Reminded { get; set; }

    /// <summary>
    ///     超时处理数量
    /// </summary>
    public int TimeoutHandled { get; set; }

    /// <summary>
    ///     跳过数量（无用户办理人、处理失败）
    /// </summary>
    public int Skipped { get; set; }

    /// <summary>
    ///     摘要
    /// </summary>
    public override string ToString()
    {
        return $"提醒 {Reminded}，超时处理 {TimeoutHandled}，跳过 {Skipped}";
    }
}
