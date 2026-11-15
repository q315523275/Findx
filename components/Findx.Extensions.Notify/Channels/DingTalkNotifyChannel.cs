using Findx.Extensions;

namespace Findx.Extensions.Notify.Channels;

/// <summary>
///     钉钉通道（群机器人 Webhook）。安全设置选「加签」时按 HMACSHA256 计算 sign
/// </summary>
public class DingTalkNotifyChannel : INotifyChannel
{
    private readonly NotifyOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DingTalkNotifyChannel> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public DingTalkNotifyChannel(IOptions<NotifyOptions> options, IHttpClientFactory httpClientFactory,
        ILogger<DingTalkNotifyChannel> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    ///     通道标识
    /// </summary>
    public NotifyChannel Channel => NotifyChannel.DingTalk;

    /// <summary>
    ///     发送
    /// </summary>
    public async Task<NotifyResult> SendAsync(NotifyMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null) return NotifyResult.Fail(Channel, "消息不能为空");
        if (string.IsNullOrWhiteSpace(_options.DingTalk.WebhookUrl))
            return NotifyResult.Fail(Channel, "未配置钉钉参数：请设置 Findx:Notify:DingTalk:WebhookUrl");

        var url = _options.DingTalk.WebhookUrl;
        if (!string.IsNullOrWhiteSpace(_options.DingTalk.Secret))
        {
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var sign = ComputeSign(timestamp, _options.DingTalk.Secret);
            url += $"{(url.Contains('?') ? "&" : "?")}timestamp={timestamp}&sign={sign}";
        }

        var msgType = message.IsMarkdown ? "markdown" : "text";
        var body = new Dictionary<string, object>
        {
            ["msgtype"] = msgType,
            [msgType] = new Dictionary<string, object> { ["content"] = BuildContent(message) },
            ["at"] = new Dictionary<string, object>
            {
                ["atMobiles"] = message.AtMobiles ?? Array.Empty<string>(),
                ["isAtAll"] = message.AtAll
            }
        };

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds));
            var response = await client.PostAsJsonAsync(url, body, null, cancellationToken);
            var text = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return NotifyResult.Fail(Channel, $"HTTP {(int)response.StatusCode}：{text}");

            var api = text.ToObject<ApiResponse>();
            if (api == null || !api.IsOk())
                return NotifyResult.Fail(Channel, $"接口返回失败：({api?.Errcode}) {api?.Errmsg}");

            return NotifyResult.Ok(Channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "钉钉通知发送失败");
            return NotifyResult.Fail(Channel, ex.Message);
        }
    }

    /// <summary>
    ///     加签：Base64(HMACSHA256(timestamp + "\n" + secret, secret)) 后 URL 编码
    /// </summary>
    public static string ComputeSign(long timestamp, string secret)
    {
        var stringToSign = $"{timestamp}\n{secret}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
        return Uri.EscapeDataString(Convert.ToBase64String(hash));
    }

    /// <summary>
    ///     标题 + 正文拼接
    /// </summary>
    private static string BuildContent(NotifyMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Title)) return message.Content ?? string.Empty;
        return message.IsMarkdown
            ? $"### {message.Title}\n{message.Content}"
            : $"{message.Title}\n{message.Content}";
    }
}
