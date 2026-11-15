namespace Findx.Extensions.Notify.Options;

/// <summary>
///     通知组件配置（配置节 Findx:Notify）
/// </summary>
/// <remarks>
///     各通道 Enabled 默认 false：未显式开启的通道不会被注册，调用时返回可读提示，
///     避免"漏配参数但已启用"这种半可用状态
/// </remarks>
public class NotifyOptions
{
    /// <summary>
    ///     组件总开关
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     单次发送超时（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    ///     发送日志开关：打开后每次发送写一条 notify_log（异步入队时由消费线程写，业务线程零 IO）
    /// </summary>
    public bool LogEnabled { get; set; } = true;

    /// <summary>
    ///     节点名（写入日志，多实例部署时定位发送节点；为空则不记）
    /// </summary>
    public string NodeName { get; set; }

    /// <summary>
    ///     异步发送队列容量。队列满时该次发送降级为同步执行，不丢消息
    /// </summary>
    public int QueueCapacity { get; set; } = 2048;

    /// <summary>
    ///     异步发送消费线程数
    /// </summary>
    public int DispatchWorkerCount { get; set; } = 2;

    /// <summary>
    ///     企业微信
    /// </summary>
    public WeComOptions WeCom { get; set; } = new();

    /// <summary>
    ///     钉钉
    /// </summary>
    public DingTalkOptions DingTalk { get; set; } = new();

    /// <summary>
    ///     邮件
    /// </summary>
    public EmailChannelOptions Email { get; set; } = new();

    /// <summary>
    ///     微信公众号
    /// </summary>
    public WeChatOptions WeChat { get; set; } = new();

    /// <summary>
    ///     微信小程序
    /// </summary>
    public WeChatMiniProgramOptions WeChatMiniProgram { get; set; } = new();
}

/// <summary>
///     企业微信配置。两种发送模式，WebhookUrl 优先：
///     <para>群机器人：只配 WebhookUrl，零权限申请，最常用</para>
///     <para>应用消息：配 CorpId + Secret + AgentId，可发给指定成员</para>
/// </summary>
public class WeComOptions
{
    /// <summary>
    ///     是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     群机器人 Webhook 地址（https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=xxx）
    /// </summary>
    public string WebhookUrl { get; set; }

    /// <summary>
    ///     企业 ID（应用消息模式）
    /// </summary>
    public string CorpId { get; set; }

    /// <summary>
    ///     应用 Secret（应用消息模式）
    /// </summary>
    public string Secret { get; set; }

    /// <summary>
    ///     应用 AgentId（应用消息模式）
    /// </summary>
    public int AgentId { get; set; }
}

/// <summary>
///     钉钉配置（群机器人 Webhook，可选加签）
/// </summary>
public class DingTalkOptions
{
    /// <summary>
    ///     是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     群机器人 Webhook 地址（https://oapi.dingtalk.com/robot/send?access_token=xxx）
    /// </summary>
    public string WebhookUrl { get; set; }

    /// <summary>
    ///     加签密钥（机器人安全设置选"加签"时填写，为空则不加签）
    /// </summary>
    public string Secret { get; set; }
}

/// <summary>
///     邮件通道配置。SMTP 参数复用框架 Findx:Email 配置节（EmailSenderOptions）
/// </summary>
public class EmailChannelOptions
{
    /// <summary>
    ///     是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     正文是否按 HTML 发送（false 为纯文本）
    /// </summary>
    public bool IsBodyHtml { get; set; } = true;
}

/// <summary>
///     微信公众号配置（模板消息）
/// </summary>
public class WeChatOptions
{
    /// <summary>
    ///     是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     公众号 AppId
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    ///     公众号 AppSecret
    /// </summary>
    public string AppSecret { get; set; }

    /// <summary>
    ///     默认模板编号（消息未指定 TemplateId 时使用）
    /// </summary>
    public string TemplateId { get; set; }
}

/// <summary>
///     微信小程序配置（订阅消息）。小程序与公众号是两套账号，AppId/AppSecret 需填小程序后台的
/// </summary>
public class WeChatMiniProgramOptions
{
    /// <summary>
    ///     是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     小程序 AppId
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    ///     小程序 AppSecret
    /// </summary>
    public string AppSecret { get; set; }

    /// <summary>
    ///     默认模板编号（消息未指定 TemplateId 时使用）
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    ///     默认跳转页面（如 pages/order/detail?id=1），为空则模板无跳转；消息内的 Page 优先
    /// </summary>
    public string Page { get; set; }

    /// <summary>
    ///     跳转的小程序版本：formal 正式版 / trial 体验版 / developer 开发版，默认 formal
    /// </summary>
    public string MiniProgramState { get; set; } = "formal";

    /// <summary>
    ///     进入小程序的语言：zh_CN / zh_TW / zh_HK / en_US，默认 zh_CN
    /// </summary>
    public string Lang { get; set; } = "zh_CN";

    /// <summary>
    ///     超长字段自动截断（thing 20 / phrase 5 / name 10 等，超长会被微信返回 47003），默认开启
    /// </summary>
    public bool TruncateTooLongValue { get; set; } = true;
}
