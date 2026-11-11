using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Models;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     引擎 - 查询
/// </summary>
public partial class FlowLongEngine
{
    /// <inheritdoc />
    public Task<List<FlwTaskInfo>> GetTodoTasksAsync(FlwCreator actor, int top = 100, CancellationToken cancellationToken = default)
    {
        return _store.QueryTodoAsync(actor, top, cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<FlwHisTaskInfo>> GetDoneTasksAsync(FlwCreator actor, int top = 100, CancellationToken cancellationToken = default)
    {
        return _store.QueryDoneAsync(actor, top, cancellationToken);
    }

    /// <inheritdoc />
    public Task<FlwInstanceInfo> GetInstanceAsync(long instanceId, CancellationToken cancellationToken = default)
    {
        return _store.GetInstanceAsync(instanceId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FlwInstanceDetail> GetInstanceDetailAsync(long instanceId, CancellationToken cancellationToken = default)
    {
        var instance = await _store.GetInstanceAsync(instanceId, cancellationToken);
        if (instance == null) throw new FlwEngineException($"流程实例不存在：{instanceId}");

        return new FlwInstanceDetail
        {
            Instance = instance,
            ActiveTasks = await _store.GetActiveTasksAsync(instanceId, cancellationToken),
            HisTasks = await _store.GetHisTasksAsync(instanceId, cancellationToken)
        };
    }
}
