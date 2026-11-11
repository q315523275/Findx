using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Models;

namespace Findx.Extensions.WorkflowCore.Storage;

/// <summary>
///     工作流存储（提醒与超时时间读写）
/// </summary>
public partial class FlwStore
{
    /// <summary>
    ///     计算首次提醒时间：到期前 RemindDay 天，若该时点已过则不提醒
    /// </summary>
    public static DateTime? ComputeRemindTime(DateTime now, FlwNodeProps props)
    {
        if (props == null || props.ExpireDay <= 0 || props.RemindDay <= 0) return null;

        var remind = now.AddDays(props.ExpireDay - props.RemindDay);
        return remind > now ? remind : null;
    }

    /// <summary>
    ///     查询到提醒时点的活动任务
    /// </summary>
    public async Task<List<FlwTaskInfo>> QueryRemindTasksAsync(DateTime now, int top = 200, CancellationToken cancellationToken = default)
    {
        var tasks = await Repo<FlwTaskInfo>().SelectAsync(x => x.RemindTime != null && x.RemindTime <= now, cancellationToken: cancellationToken);
        return tasks.OrderBy(x => x.RemindTime).Take(top).ToList();
    }

    /// <summary>
    ///     查询已到期（超时）的活动任务
    /// </summary>
    public async Task<List<FlwTaskInfo>> QueryExpiredTasksAsync(DateTime now, int top = 200, CancellationToken cancellationToken = default)
    {
        var tasks = await Repo<FlwTaskInfo>().SelectAsync(x => x.ExpireTime != null && x.ExpireTime <= now, cancellationToken: cancellationToken);
        return tasks.OrderBy(x => x.ExpireTime).Take(top).ToList();
    }

    /// <summary>
    ///     更新下次提醒时间（null 表示不再提醒）
    /// </summary>
    public async Task UpdateRemindAsync(FlwTaskInfo task, DateTime? nextRemindTime, CancellationToken cancellationToken = default)
    {
        task.RemindTime = nextRemindTime;
        task.LastUpdatedTime = DateTime.Now;
        await Repo<FlwTaskInfo>().UpdateAsync(task, cancellationToken: cancellationToken);
    }

    /// <summary>
    ///     清除任务的时限约束（提醒与到期时间已消费，避免重复触发）
    /// </summary>
    public async Task ClearTimeoutAsync(FlwTaskInfo task, CancellationToken cancellationToken = default)
    {
        task.RemindTime = null;
        task.ExpireTime = null;
        task.RemindRepeat = 0;
        task.LastUpdatedTime = DateTime.Now;
        await Repo<FlwTaskInfo>().UpdateAsync(task, cancellationToken: cancellationToken);
    }
}
