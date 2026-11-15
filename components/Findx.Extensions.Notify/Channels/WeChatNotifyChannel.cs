using Findx.Extensions;

namespace Findx.Extensions.Notify.Channels;

/// <summary>
///     微信公众号通道（模板消息）。微信接口不支持批量 touser，按 openid 逐条发送
/// </summary>
public class WeChatNotifyChannel : INotifyChannel
{
    private const string SendUrl = "https://api.weixin.qq.com/cgi-bin/message/template/send";

    private readonly NotifyOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AccessTokenProvider _tokenProvider = new();
    private readonly ILogger<WeChatNotifyChannel> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public WeChatNotifyChannel(IOptions<NotifyOptions> options, IHttpClientFactory httpClientFactory,
        ILogger<WeChatNotifyChannel> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    ///     通道标识
    /// </summary>
    public NotifyChannel Channel => NotifyChannel.WeChat;

    /// <summary>
    ///     发送
    /// </summary>
    public async Task<NotifyResult> SendAsync(NotifyMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null) return NotifyResult.Fail(Channel, "消息不能为空");

        var weChat = _options.WeChat;
        if (string.IsNullOrWhiteSpace(weChat.AppId) || string.IsNullOrWhiteSpace(weChat.AppSecret))
            return NotifyResult.Fail(Channel, "未配置微信参数：请设置 Findx:Notify:WeChat:AppId / AppSecret");

        var templateId = string.IsNullOrWhiteSpace(message.TemplateId) ? weChat.TemplateId : message.TemplateId;
        if (string.IsNullOrWhiteSpace(templateId))
            return NotifyResult.Fail(Channel, "未指定模板：NotifyMessage.TemplateId 或 Findx:Notify:WeChat:TemplateId 必填");

        var openIds = message.To?.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (openIds == null || openIds.Length == 0)
            return NotifyResult.Fail(Channel, "接收人不能为空（NotifyMessage.To 填 openid）");

        try
        {
            var token = await _tokenProvider.GetAsync(
                ct => WeChatTokenFetcher.FetchAsync(CreateClient(), weChat.AppId, weChat.AppSecret, ct),
                cancellationToken);

            //  逐条发送，单条失败不影响其它接收人，最后汇总
            var errors = new List<string>();
            foreach (var openId in openIds)
            {
                var body = new Dictionary<string, object>
                {
                    ["touser"] = openId,
                    ["template_id"] = templateId,
                    ["url"] = message.Url ?? string.Empty,
                    ["data"] = BuildData(message)
                };
                var result = await SendOneAsync(token, body, cancellationToken);
                if (!result.Success) errors.Add($"{openId}: {result.Message}");
            }

            return errors.Count == 0
                ? NotifyResult.Ok(Channel, $"已发送 {openIds.Length} 条")
                : NotifyResult.Fail(Channel, string.Join("；", errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "微信模板消息发送失败");
            return NotifyResult.Fail(Channel, ex.Message);
        }
    }

    /// <summary>
    ///     发送单条模板消息
    /// </summary>
    private async Task<NotifyResult> SendOneAsync(string token, Dictionary<string, object> body, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync($"{SendUrl}?access_token={Uri.EscapeDataString(token)}", body, null, cancellationToken);
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
            _logger.LogError(ex, "微信模板消息单条发送失败");
            return NotifyResult.Fail(Channel, ex.Message);
        }
    }

    /// <summary>
    ///     Data 转微信模板 data 结构：{ key: { value: "x" } }
    /// </summary>
    private static Dictionary<string, object> BuildData(NotifyMessage message)
    {
        var data = new Dictionary<string, object>();
        if (message.Data == null) return data;

        foreach (var kv in message.Data)
        {
            data[kv.Key] = new Dictionary<string, object> { ["value"] = kv.Value?.ToString() ?? string.Empty };
        }
        return data;
    }

    /// <summary>
    ///     创建带超时的 HttpClient
    /// </summary>
    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds));
        return client;
    }
}
