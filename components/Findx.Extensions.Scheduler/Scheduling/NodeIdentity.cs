namespace Findx.Extensions.Scheduler.Scheduling;

/// <summary>
///     节点标识工具：WorkerId 与 Master 租约持有者标识的统一推导规则。
/// </summary>
public static class NodeIdentity
{
    /// <summary>
    ///     由节点名推导稳定标识（去除 key 非法字符；空值回退机器名）
    /// </summary>
    /// <param name="nodeName"></param>
    /// <returns></returns>
    public static string Resolve(string nodeName)
    {
        var id = string.IsNullOrWhiteSpace(nodeName) ? Environment.MachineName : nodeName.Trim();
        foreach (var ch in new[] { ' ', ':', '*', '?', '\n', '\r', '\t' }) id = id.Replace(ch, '-');
        return id;
    }
}
