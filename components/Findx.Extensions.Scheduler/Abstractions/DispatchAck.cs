namespace Findx.Extensions.Scheduler.Abstractions;

/// <summary>
///     分发结果：区分"未受理"与"已受理"，两者进入不同的后续路径。
///     <para>未受理（拒绝/不可达）才算分发失败；已受理后即使执行出错也由回调/超时兜底处理，
///     不能重复计入失败预算。</para>
/// </summary>
public enum DispatchAck
{
    /// <summary>
    ///     Worker 已受理，等待回调
    /// </summary>
    Accepted = 0,

    /// <summary>
    ///     Worker 明确拒绝（队列满 / 参数非法）
    /// </summary>
    Rejected = 1,

    /// <summary>
    ///     Worker 不可达（连接失败 / HTTP 超时）
    /// </summary>
    Unreachable = 2,

    /// <summary>
    ///     没有可执行该任务的节点
    /// </summary>
    NoWorker = 3,
}
