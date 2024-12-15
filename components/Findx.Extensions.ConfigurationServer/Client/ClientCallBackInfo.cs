using System.Text.Json.Serialization;

namespace Findx.Extensions.ConfigurationServer.Client;

/// <summary>
///     客户端回调详情
/// </summary>
/// <typeparam name="T"></typeparam>
public class ClientCallBackInfo<T>
{
    /// <summary>
    ///     任务完成源
    /// </summary>
    [JsonIgnore]
    public TaskCompletionSource<T> Task { set; get; }

    /// <summary>
    ///     客户端请求编号
    /// </summary>
    public string TraceIdentifier { set; get; }

    /// <summary>
    ///     客户端Ip
    /// </summary>
    public string ClientIpAddress { set; get; }

    /// <summary>
    ///     取消令牌
    /// </summary>
    [JsonIgnore]
    public CancellationTokenSource CancellationTokenSource { set; get; }
    
    /// <summary>
    ///     注册的回调
    /// </summary>
    [JsonIgnore]
    public CancellationTokenRegistration CancellationTokenRegistration { set; get; }
}