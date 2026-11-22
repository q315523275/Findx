namespace Findx.Extensions.Scheduler.Abstractions;

/// <summary>
///     分发结果到可读信息的映射（日志与死信原因共用）
/// </summary>
public static class DispatchAckExtensions
{
    /// <summary>
    ///     转为失败原因描述
    /// </summary>
    /// <param name="ack"></param>
    /// <returns></returns>
    public static string ToMessage(this DispatchAck ack)
    {
        return ack switch
        {
            DispatchAck.NoWorker => "分发失败：无可执行节点",
            DispatchAck.Rejected => "分发失败：执行端拒绝受理",
            DispatchAck.Unreachable => "分发失败：执行端不可达",
            _ => string.Empty
        };
    }
}
