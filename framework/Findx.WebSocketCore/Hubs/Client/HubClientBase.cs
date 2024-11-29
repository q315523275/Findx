using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Data;
using Findx.Extensions;
using Findx.Utilities;

namespace Findx.WebSocketCore.Hubs.Client;

/// <summary>
///     通信客户端基类
/// </summary>
public abstract class HubClientBase: IHubClient
{
    /// <summary>
    /// 获取 网络通信连接
    /// </summary>
    protected HubConnection HubConnection { get; set; }
    
    /// <summary>
    ///     获取 通信Hub地址
    /// </summary>
    public string HubUrl { get; }
    
    /// <summary>
    ///     获取或设置 客户端版本
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    ///     获取或设置 当前网络是否通畅
    /// </summary>
    public bool IsNetConnected { get; } = true;
    
    /// <summary>
    ///     获取 通信是否正在连接
    /// </summary>
    public bool IsHubConnected => HubConnection?.State == HubConnectionState.Connected;
    
    /// <summary>
    ///     获取 通信是否正在连接
    /// </summary>
    protected bool IsHubConnecting => new[] { HubConnectionState.Connecting, HubConnectionState.Reconnecting }.Contains(HubConnection.State);
    
    /// <summary>
    ///     网络通信初始化
    /// </summary>
    public void Initialize()
    {
        Check.NotNull(HubUrl, nameof(HubUrl));
        HubConnection = new HubConnectionBuilder().WithUrl(HubUrl).Build();
        HubConnection.Closed += OnClosed;
        HubConnection.Reconnecting += OnReconnecting;
        HubConnection.Reconnected += OnReconnected;
    }

    /// <summary>
    ///     开始通信
    /// </summary>
    /// <returns></returns>
    public virtual async Task<CommonResult> StartAsync()
    {
        try
        {
            var watch = Stopwatch.StartNew();
            Console.WriteLine("开始连接通信服务器");
            await HubConnection.StartAsync();
            watch.Stop();
            Console.WriteLine($"通信服务器连接成功，耗时：{watch.Elapsed}");
            return CommonResult.Success();
        }
        catch (Exception ex)
        {
            return CommonResult.Fail("connection.start.error", ex.Message);
        }
    }

    /// <summary>
    ///     停止通信
    /// </summary>
    /// <returns></returns>
    public virtual async Task StopAsync()
    {
        if (!IsHubConnected)
        {
            return;
        }
        Console.WriteLine("断开通信服务器");
        await HubConnection.StopAsync();
    }

    /// <summary>
    ///     重启通信
    /// </summary>
    /// <returns></returns>
    public virtual async  Task<CommonResult> ReStartAsync()
    {
        await StopAsync();
        while (HubConnection.State != HubConnectionState.Disconnected)
        {
            await Task.Delay(50);
        }
        return await StartAsync();
    }

    /// <summary>
    ///     发送消息
    /// </summary>
    public virtual async Task SendAsync(RequestMessage message, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken = default)
    {
        await WaitForConnectedAsync();
        HubConnection.SendAsync(message, messageType, endOfMessage, cancellationToken);
    }
    
    
    /// <summary>
    ///     在连接关闭时触发
    /// </summary>
    /// <param name="error">错误信息</param>
    /// <returns></returns>
    protected virtual async Task OnClosed(Exception error)
    {
        var delay = RandomUtility.RandomInt(0, 5);
        await TimeSpan.FromMilliseconds(delay).CountDownAsync(span => { Console.WriteLine($"{span.TotalMilliseconds}秒后重试连接通信服务器"); return Task.CompletedTask; });
        await HubConnection.StartAsync();
        Console.WriteLine($"通信服务器连接{(HubConnection.State == HubConnectionState.Connected ? "成功" : "失败")}");
    }

    /// <summary>
    ///     在重连之后触发
    /// </summary>
    /// <returns></returns>
    protected virtual Task OnReconnected()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     在正在重连时触发
    /// </summary>
    /// <returns></returns>
    protected virtual Task OnReconnecting()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     重写以实现<see cref="HubConnection"/>通信监听
    /// </summary>
    /// <param name="connection">连接对象</param>
    protected virtual void HubListenOn(HubConnection connection)
    { }

    /// <summary>
    ///     等待通信连接通畅
    /// </summary>
    /// <returns></returns>
    protected virtual async Task WaitForConnectedAsync()
    {
        // 已连接
        if (IsHubConnected)
        {
            return;
        }

        // 等待正在连接
        while (IsHubConnecting)
        {
            await Task.Delay(200);
            if (IsHubConnected)
            {
                return;
            }
        }

        while (!IsHubConnected)
        {
            int delay;
            while (!IsNetConnected)
            {
                delay = RandomUtility.RandomInt(6, 12);
                await TimeSpan.FromMilliseconds(delay).CountDownAsync(span => { Console.WriteLine($"当前网络不通畅，延时{span.TotalMilliseconds}秒后重试"); return Task.CompletedTask; });
            }

            delay = RandomUtility.RandomInt(3, 8);
            await TimeSpan.FromMilliseconds(delay).CountDownAsync(span => { Console.WriteLine($"服务器连接失败，延时{span.TotalMilliseconds}秒后重试"); return Task.CompletedTask; });
            await StartAsync();
        }
    }
}