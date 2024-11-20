using Findx.Common;

namespace Findx.WebSocketCore.Hubs.Client;

/// <summary>
///     通信连接构建
/// </summary>
public class HubConnectionBuilder
{
    /// <summary>
    ///     配置Url地址
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public HubConnectionBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    /// <summary>
    ///     配置自动重连
    /// </summary>
    /// <returns></returns>
    public HubConnectionBuilder WithAutomaticReconnection()
    {
        _automaticReconnection = true;
        return this;
    }

    /// <summary>
    ///     配置序列化工具
    /// </summary>
    /// <returns></returns>
    public HubConnectionBuilder WithSerializer(IWebSocketSerializer serializer)
    {
        _webSocketSerializer = serializer;
        return this;
    }
    
    
    private string _url;
    private bool _automaticReconnection;
    private IWebSocketSerializer _webSocketSerializer;
    
    /// <summary>
    ///     
    /// </summary>
    /// <returns></returns>
    public HubConnection Build()
    {
        Check.NotNullOrWhiteSpace(_url, nameof(_url));
        return new HubConnection(_url, _automaticReconnection, _webSocketSerializer);
    }
}