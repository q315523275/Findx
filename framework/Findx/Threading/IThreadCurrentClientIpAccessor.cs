namespace Findx.Threading;

/// <summary>
/// 线程当前客户端ip访问器
/// </summary>
public interface IThreadCurrentClientIpAccessor
{
    /// <summary>
    /// 获取客户端ip
    /// </summary>
    /// <returns></returns>
    string GetClientIp();
}