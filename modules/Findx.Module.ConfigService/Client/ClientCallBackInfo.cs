namespace Findx.Module.ConfigService.Client;

/// <summary>
///     客户端回调详情
/// </summary>
/// <typeparam name="T"></typeparam>
public class ClientCallBackInfo<T>
{
    /// <summary>
    ///     任务完成源
    /// </summary>
    public TaskCompletionSource<T> Task { set; get; }

    /// <summary>
    ///     客户端请求编号
    /// </summary>
    public string ReqId { set; get; }

    /// <summary>
    ///     客户端Ip
    /// </summary>
    public string ClientIp { set; get; }

    /// <summary>
    ///     取消令牌
    /// </summary>
    public CancellationTokenSource CancellationTokenSource { set; get; }
    
    /// <summary>
    ///     注册的回调
    /// </summary>
    public CancellationTokenRegistration CancellationTokenRegistration { set; get; }
}