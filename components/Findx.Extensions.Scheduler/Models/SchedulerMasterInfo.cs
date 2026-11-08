namespace Findx.Extensions.Scheduler.Models;

/// <summary>
///     Master 节点信息（写入 Redis MasterInfoKey，Worker 回调地址解析用）
/// </summary>
public class SchedulerMasterInfo
{
    /// <summary>
    ///     Master 对外回调地址
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    ///     节点显示名
    /// </summary>
    public string NodeName { get; set; }

    /// <summary>
    ///     成为 Master 的时间
    /// </summary>
    public DateTime StartedAt { get; set; }
}
