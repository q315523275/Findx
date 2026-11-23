using Microsoft.Extensions.Logging;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     提交后回调队列：事务提交成功后统一执行，单个回调失败只记日志、不影响其它回调
///     <para>注意：外层已有工作单元（宿主自建事务）时，引擎的提交是嵌套提交（只弹栈不落库），
///     此时队列会在引擎方法返回时执行，可能早于宿主外层提交。需要严格"提交后"语义的场景，
///     业务回写请用 IFlwBusinessAdapter 的同步回调，不要依赖本队列</para>
/// </summary>
internal sealed class FlwPostCommitQueue
{
    private readonly List<Func<CancellationToken, Task>> _actions = [];

    /// <summary>
    ///     入队
    /// </summary>
    public void Add(Func<CancellationToken, Task> action)
    {
        if (action != null) _actions.Add(action);
    }

    /// <summary>
    ///     执行并清空
    /// </summary>
    public async Task FlushAsync(ILogger logger, CancellationToken cancellationToken = default)
    {
        if (_actions.Count == 0) return;
        var actions = _actions.ToList();
        _actions.Clear();
        foreach (var action in actions)
        {
            try
            {
                await action(cancellationToken);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "工作流提交后回调执行失败");
            }
        }
    }
}
