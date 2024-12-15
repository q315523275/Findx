using Findx.Extensions.ConfigurationServer.Dtos;

namespace Findx.Extensions.ConfigurationServer.Client;

/// <summary>
///     客户端回调
/// </summary>
public interface IClientCallBack : IDisposable
{
    /// <summary>
    ///     创建新的回调任务
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="traceIdentifier"></param>
    /// <param name="clientIpAddress"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    Task<ConfigDataChangeDto> NewCallBackTaskAsync(string appId, string traceIdentifier, string clientIpAddress, int timeout);

    /// <summary>
    ///     执行回调任务
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="content"></param>
    void CallBack(string appId, ConfigDataChangeDto content);

    /// <summary>
    ///     客户端信息集合
    /// </summary>
    /// <returns></returns>
    IDictionary<string, List<ClientCallBackInfo<ConfigDataChangeDto>>> Metrics();
}