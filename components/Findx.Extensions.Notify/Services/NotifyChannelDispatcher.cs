using Findx.Extensions.Notify.Channels;

namespace Findx.Extensions.Notify.Services;

/// <summary>
///     通道路由与超时收敛。同步发送（NotifySender）与异步消费线程（NotifyDispatchQueue）共用，
///     避免两边各写一套路由逻辑
/// </summary>
public class NotifyChannelDispatcher
{
    private readonly NotifyOptions _options;
    private readonly Dictionary<NotifyChannel, INotifyChannel> _channels;
    private readonly ILogger<NotifyChannelDispatcher> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public NotifyChannelDispatcher(IOptions<NotifyOptions> options, IEnumerable<INotifyChannel> channels,
        ILogger<NotifyChannelDispatcher> logger)
    {
        _options = options.Value;
        _channels = new Dictionary<NotifyChannel, INotifyChannel>();
        if (channels != null)
        {
            foreach (var channel in channels) _channels[channel.Channel] = channel;
        }
        _logger = logger;
    }

    /// <summary>
    ///     发送一条消息到其自带通道
    /// </summary>
    public async Task<NotifyResult> SendAsync(NotifyMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null) return NotifyResult.Fail(NotifyChannel.WeCom, "消息不能为空");
        if (!_options.Enabled) return NotifyResult.Fail(message.Channel, "通知组件未启用（Findx:Notify:Enabled=false）");

        if (!_channels.TryGetValue(message.Channel, out var channel))
            return NotifyResult.Fail(message.Channel,
                $"通道 {message.Channel} 未启用：请配置 Findx:Notify:{message.Channel}:Enabled=true");

        //  统一超时：通道内部异常与超时都收敛成失败结果，不抛给调用方
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds)));

        try
        {
            return await channel.SendAsync(message, cts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("通道 {Channel} 发送超时（{Timeout} 秒）", message.Channel, _options.TimeoutSeconds);
            return NotifyResult.Fail(message.Channel, $"发送超时（{_options.TimeoutSeconds} 秒）");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通道 {Channel} 发送异常", message.Channel);
            return NotifyResult.Fail(message.Channel, ex.Message);
        }
    }
}
