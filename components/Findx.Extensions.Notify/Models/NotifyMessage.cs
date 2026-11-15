namespace Findx.Extensions.Notify.Models;

/// <summary>
///     统一通知消息。字段语义按通道解释，不同通道只用其中一部分：
///     <para>企业微信：群机器人只用 Title/Content；应用消息用 To（成员账号，空则全员）</para>
///     <para>钉钉：群机器人用 Title/Content/AtMobiles/AtAll</para>
///     <para>邮件：To 为邮箱地址（可多条），Title 做主题、Content 做正文</para>
///     <para>微信：To 为 openid，用 TemplateId + Data 渲染模板</para>
/// </summary>
public class NotifyMessage
{
    /// <summary>
    ///     通道
    /// </summary>
    public NotifyChannel Channel { get; set; } = NotifyChannel.WeCom;

    /// <summary>
    ///     接收方。语义按通道：邮箱地址 / 企业微信成员账号 / 微信 openid
    /// </summary>
    public string[] To { get; set; }

    /// <summary>
    ///     标题。邮件作主题，IM 通道与正文拼接
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     正文
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    ///     是否按 markdown 排版（企业微信与钉钉支持）
    /// </summary>
    public bool IsMarkdown { get; set; }

    /// <summary>
    ///     模板编号（微信模板消息必填）
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    ///     模板变量（微信模板消息的 data，键为模板中的占位字段名）
    /// </summary>
    public Dictionary<string, object> Data { get; set; }

    /// <summary>
    ///     跳转链接（微信模板消息的 url）
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     小程序跳转页面（订阅消息的 page，如 pages/order/detail?id=1），为空则用配置默认值
    /// </summary>
    public string Page { get; set; }

    /// <summary>
    ///     业务关联键（如单据号、流程实例号），落日志表便于按业务反查通知
    /// </summary>
    public string BusinessKey { get; set; }

    /// <summary>
    ///     钉钉 @ 手机号
    /// </summary>
    public string[] AtMobiles { get; set; }

    /// <summary>
    ///     钉钉是否 @所有人
    /// </summary>
    public bool AtAll { get; set; }

    /// <summary>
    ///     快速构造一条通道消息
    /// </summary>
    public static NotifyMessage Create(NotifyChannel channel, string title, string content)
    {
        return new NotifyMessage { Channel = channel, Title = title, Content = content };
    }

    /// <summary>
    ///     复制一条消息（多通道广播时逐通道改 Channel，避免并发写同一实例）
    /// </summary>
    public NotifyMessage Clone()
    {
        return new NotifyMessage
        {
            Channel = Channel,
            To = To,
            Title = Title,
            Content = Content,
            IsMarkdown = IsMarkdown,
            TemplateId = TemplateId,
            Data = Data,
            Url = Url,
            Page = Page,
            BusinessKey = BusinessKey,
            AtMobiles = AtMobiles,
            AtAll = AtAll
        };
    }
}
