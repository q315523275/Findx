using Findx.Module.ConfigService.Dtos;

namespace Findx.Module.ConfigService.Client;

/// <summary>
/// 客户端回调
/// </summary>
public interface IClientCallBack: IDisposable
{
    /// <summary>
    /// 创建新的回调任务
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="reqId"></param>
    /// <param name="clientIp"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    Task<ConfigDataChangeDto> NewCallBackTaskAsync(string appId, string reqId, string clientIp, int timeout);

    /// <summary>
    /// 执行回调
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="content"></param>
    void CallBack(string appId, ConfigDataChangeDto content);
}