namespace Findx.Email;

/// <summary>
///     邮件发送配置
/// </summary>
public class EmailSenderOptions : IOptions<EmailSenderOptions>
{
    /// <summary>
    ///     主机
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    ///     端口号
    /// </summary>
    public int Port { get; set; } = 25;

    /// <summary>
    ///     用户名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    ///     密码
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    ///     是否启用SSL
    /// </summary>
    public bool EnableSsl { get; set; }

    /// <summary>
    ///     是否忽略SSL证书校验（仅自签名/内网测试环境开启，默认false）
    /// </summary>
    public bool EnableSslIgnoreCertificate { get; set; }

    /// <summary>
    ///     显示名称
    /// </summary>
    public string FromDisplayName { get; set; }

    /// <summary>
    ///     发送地址
    /// </summary>
    public string FromAddress { get; set; }

    /// <summary>
    ///     value
    /// </summary>
    public EmailSenderOptions Value => this;
}