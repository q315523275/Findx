using Findx.Extensions;

namespace Findx.Extensions.Notify.Channels;

/// <summary>
///     微信小程序订阅消息通道。
///     <para>订阅消息不支持批量 touser，按 openid 逐条发送</para>
///     <para>用户必须先在小程序内调用 wx.requestSubscribeMessage 授权；一次性订阅每次授权只能下发一条，发完即 43101</para>
/// </summary>
public class WeChatMiniProgramNotifyChannel : INotifyChannel
{
    private const string SendUrl = "https://api.weixin.qq.com/cgi-bin/message/subscribe/send";

    private readonly NotifyOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WeChatMiniProgramNotifyChannel> _logger;

    //  小程序与公众号是两套账号、令牌互不通用，各自一份缓存
    private readonly AccessTokenProvider _tokenProvider = new();

    /// <summary>
    ///     Ctor
    /// </summary>
    public WeChatMiniProgramNotifyChannel(IOptions<NotifyOptions> options, IHttpClientFactory httpClientFactory,
        ILogger<WeChatMiniProgramNotifyChannel> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    ///     通道标识
    /// </summary>
    public NotifyChannel Channel => NotifyChannel.WeChatMiniProgram;

    /// <summary>
    ///     发送
    /// </summary>
    public async Task<NotifyResult> SendAsync(NotifyMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null) return NotifyResult.Fail(Channel, "消息不能为空");

        var mini = _options.WeChatMiniProgram;
        if (string.IsNullOrWhiteSpace(mini.AppId) || string.IsNullOrWhiteSpace(mini.AppSecret))
            return NotifyResult.Fail(Channel, "未配置小程序参数：请设置 Findx:Notify:WeChatMiniProgram:AppId / AppSecret");

        var templateId = string.IsNullOrWhiteSpace(message.TemplateId) ? mini.TemplateId : message.TemplateId;
        if (string.IsNullOrWhiteSpace(templateId))
            return NotifyResult.Fail(Channel,
                "未指定模板：NotifyMessage.TemplateId 或 Findx:Notify:WeChatMiniProgram:TemplateId 必填");

        var openIds = message.To?.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (openIds == null || openIds.Length == 0)
            return NotifyResult.Fail(Channel, "接收人不能为空（NotifyMessage.To 填小程序 openid）");

        try
        {
            var token = await _tokenProvider.GetAsync(
                ct => WeChatTokenFetcher.FetchAsync(CreateClient(), mini.AppId, mini.AppSecret, ct),
                cancellationToken);

            var state = string.IsNullOrWhiteSpace(mini.MiniProgramState) ? "formal" : mini.MiniProgramState;
            var lang = string.IsNullOrWhiteSpace(mini.Lang) ? "zh_CN" : mini.Lang;
            var page = string.IsNullOrWhiteSpace(message.Page) ? mini.Page : message.Page;
            var data = BuildData(message, mini.TruncateTooLongValue);

            //  逐条发送，单个接收人失败不影响其它人，最后汇总
            var errors = new List<string>();
            foreach (var openId in openIds)
            {
                var body = new Dictionary<string, object>
                {
                    ["touser"] = openId,
                    ["template_id"] = templateId,
                    ["data"] = data,
                    ["miniprogram_state"] = state,
                    ["lang"] = lang
                };
                //  page 不填则模板无跳转，不能传空串
                if (!string.IsNullOrWhiteSpace(page)) body["page"] = page;

                var result = await SendOneAsync(token, body, cancellationToken);
                if (!result.Success) errors.Add($"{openId}: {result.Message}");
            }

            return errors.Count == 0
                ? NotifyResult.Ok(Channel, $"已发送 {openIds.Length} 条订阅消息")
                : NotifyResult.Fail(Channel, string.Join("；", errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "小程序订阅消息发送失败");
            return NotifyResult.Fail(Channel, ex.Message);
        }
    }

    /// <summary>
    ///     发送单条订阅消息
    /// </summary>
    private async Task<NotifyResult> SendOneAsync(string token, Dictionary<string, object> body,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync($"{SendUrl}?access_token={Uri.EscapeDataString(token)}",
                body, null, cancellationToken);
            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                return NotifyResult.Fail(Channel, $"HTTP {(int)response.StatusCode}：{text}");

            var api = text.ToObject<ApiResponse>();
            if (api == null || !api.IsOk())
                return NotifyResult.Fail(Channel, DescribeError(api));

            return NotifyResult.Ok(Channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "小程序订阅消息单条发送失败");
            return NotifyResult.Fail(Channel, ex.Message);
        }
    }

    /// <summary>
    ///     错误码转人话：订阅消息的失败原因多数是订阅额度或参数问题，直接给排查方向
    /// </summary>
    private static string DescribeError(ApiResponse api)
    {
        var code = api?.Errcode ?? 0;
        var detail = $"({code}) {api?.Errmsg}";
        return code switch
            {
                40001 => $"access_token 无效：{detail}",
                40003 => $"openid 不合法（须属于本小程序）：{detail}",
                40037 => $"template_id 不存在或已删除：{detail}",
                41030 => $"跳转 page 不存在（须与已发布版本页面一致）：{detail}",
                43101 => $"用户未订阅该模板，或一次性订阅的额度已用完（每次授权只能下发一条）：{detail}",
                43107 => $"订阅消息能力被封禁：{detail}",
                43108 => $"同一用户并发下发，请降低频率：{detail}",
                47003 => $"模板参数不合法（字段长度/格式/字段名须与模板一致）：{detail}",
                _ => $"接口返回失败：{detail}"
            };
    }

    /// <summary>
    ///     Data 转订阅消息 data 结构：{ key: { value: "x" } }，按字段类型截断超长值
    /// </summary>
    private Dictionary<string, object> BuildData(NotifyMessage message, bool truncate)
    {
        var data = new Dictionary<string, object>();
        if (message.Data == null) return data;

        foreach (var kv in message.Data)
        {
            var value = kv.Value?.ToString() ?? string.Empty;
            if (truncate)
            {
                value = MiniProgramDataRules.Truncate(kv.Key, value, out var truncated);
                if (truncated)
                    _logger.LogWarning("小程序订阅消息字段 {Key} 超出长度限制，已截断为 {Limit} 字符", kv.Key,
                        MiniProgramDataRules.GetLimit(kv.Key));
            }

            data[kv.Key] = new Dictionary<string, object> { ["value"] = value };
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
