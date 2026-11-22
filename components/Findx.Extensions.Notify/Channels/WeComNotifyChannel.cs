using Findx.Extensions;

namespace Findx.Extensions.Notify.Channels;

/// <summary>
///     企业微信通道。两种模式，WebhookUrl 优先（群机器人不需要申请任何权限）：
///     <para>群机器人：POST https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=xxx</para>
///     <para>应用消息：先取 access_token，再 POST /cgi-bin/message/send</para>
/// </summary>
public class WeComNotifyChannel : INotifyChannel
{
    private const string TokenUrl = "https://qyapi.weixin.qq.com/cgi-bin/gettoken";
    private const string SendUrl = "https://qyapi.weixin.qq.com/cgi-bin/message/send";

    private readonly NotifyOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AccessTokenProvider _tokenProvider = new();
    private readonly ILogger<WeComNotifyChannel> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="httpClientFactory"></param>
    /// <param name="logger"></param>
    public WeComNotifyChannel(IOptions<NotifyOptions> options, IHttpClientFactory httpClientFactory, ILogger<WeComNotifyChannel> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    ///     通道标识
    /// </summary>
    public NotifyChannel Channel => NotifyChannel.WeCom;

    /// <summary>
    ///     发送消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<NotifyResult> SendAsync(NotifyMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null) return NotifyResult.Fail(Channel, "消息不能为空");

        //  群机器人优先：零权限申请，最常用
        if (!string.IsNullOrWhiteSpace(_options.WeCom.WebhookUrl))
            return await SendByWebhookAsync(message, cancellationToken);

        //  应用消息：需要 corpId + secret + agentId，可发给指定成员
        if (!string.IsNullOrWhiteSpace(_options.WeCom.CorpId) && !string.IsNullOrWhiteSpace(_options.WeCom.Secret) && _options.WeCom.AgentId > 0)
            return await SendByAgentAsync(message, cancellationToken);

        return NotifyResult.Fail(Channel, "未配置企业微信参数：请设置 Findx:Notify:WeCom:WebhookUrl（群机器人），或 CorpId/Secret/AgentId（应用消息）");
    }

    /// <summary>
    ///     群机器人发送
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private Task<NotifyResult> SendByWebhookAsync(NotifyMessage message, CancellationToken cancellationToken)
    {
        var msgType = message.IsMarkdown ? "markdown" : "text";
        var body = new Dictionary<string, object>
        {
            ["msgtype"] = msgType,
            [msgType] = new Dictionary<string, object> { ["content"] = BuildContent(message) }
        };
        return SendCoreAsync(_options.WeCom.WebhookUrl, body, cancellationToken);
    }

    /// <summary>
    ///     应用消息发送
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<NotifyResult> SendByAgentAsync(NotifyMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _tokenProvider.GetAsync(async ct =>
            {
                using var client = CreateClient();
                var url = $"{TokenUrl}?corpid={Uri.EscapeDataString(_options.WeCom.CorpId)}&corpsecret={Uri.EscapeDataString(_options.WeCom.Secret)}";
                var response = await client.GetAsync(url, ct);
                var text = await response.Content.ReadAsStringAsync(ct);
                var api = text.ToObject<ApiResponse>();
                if (api == null || !api.IsOk() || string.IsNullOrWhiteSpace(api.AccessToken))
                    throw new InvalidOperationException($"企业微信获取令牌失败：{(api == null ? "响应解析为空" : $"({api.Errcode}) {api.Errmsg}")}");
                return (api.AccessToken, api.ExpiresIn <= 0 ? 7200 : api.ExpiresIn);
            }, cancellationToken);

            var msgType = message.IsMarkdown ? "markdown" : "text";
            var body = new Dictionary<string, object>
            {
                ["touser"] = message.To?.Length > 0 ? string.Join("|", message.To) : "@all",
                ["msgtype"] = msgType,
                [msgType] = new Dictionary<string, object> { ["content"] = BuildContent(message) },
                ["agentid"] = _options.WeCom.AgentId,
                ["safe"] = 0
            };
            return await SendCoreAsync($"{SendUrl}?access_token={Uri.EscapeDataString(token)}", body, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "企业微信应用消息发送失败");
            return NotifyResult.Fail(Channel, ex.Message);
        }
    }

    /// <summary>
    ///     POST JSON 并解析 errcode
    /// </summary>
    /// <param name="url"></param>
    /// <param name="body"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<NotifyResult> SendCoreAsync(string url, Dictionary<string, object> body, CancellationToken cancellationToken)
    {
        try
        {
            using var client = CreateClient();
            var response = await client.PostAsJsonAsync(url, body, null, cancellationToken);
            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                return NotifyResult.Fail(Channel, $"HTTP {(int)response.StatusCode}：{text}");

            var api = text.ToObject<ApiResponse>();
            if (api == null || !api.IsOk())
                return NotifyResult.Fail(Channel, $"接口返回失败：({api?.Errcode}) {api?.Errmsg}");

            return NotifyResult.Ok(Channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "企业微信通知发送失败");
            return NotifyResult.Fail(Channel, ex.Message);
        }
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

    /// <summary>
    ///     标题 + 正文拼接（markdown 用三级标题）
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private static string BuildContent(NotifyMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Title)) return message.Content ?? string.Empty;
        return message.IsMarkdown ? $"### {message.Title}\n{message.Content}" : $"{message.Title}\n{message.Content}";
    }
}
