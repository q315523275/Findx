using System.Threading.Tasks;
using Findx.Data;

namespace Findx.WebSocketCore.Hubs.Client;

/// <summary>
///     通信客户端
/// </summary>
public interface IHubClient
{
    /// <summary>
    ///     获取 通信Hub地址
    /// </summary>
    string HubUrl { get; }

    /// <summary>
    ///     获取或设置 客户端版本
    /// </summary>
    string Version { get; set; }

    /// <summary>
    ///     获取 当前网络是否通畅
    /// </summary>
    bool IsNetConnected { get; }

    /// <summary>
    ///     获取 当前通信是否已连接
    /// </summary>
    bool IsHubConnected { get; }

    /// <summary>
    ///     网络通信初始化
    /// </summary>
    void Initialize();
    
    /// <summary>
    ///     开始通信
    /// </summary>
    /// <returns></returns>
    Task<CommonResult> StartAsync();

    /// <summary>
    ///     停止通信
    /// </summary>
    /// <returns></returns>
    Task StopAsync();

    /// <summary>
    ///     重启通信
    /// </summary>
    /// <returns></returns>
    Task<CommonResult> ReStartAsync();

    /// <summary>
    ///     向服务器发送数据
    /// </summary>
    /// <param name="methodName">Hub方法名</param>
    /// <param name="args">调用参数</param>
    /// <returns></returns>
    Task SendToHubAsync(string methodName, params object[] args);
}