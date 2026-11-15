using Findx.Email;

namespace Findx.Extensions.Notify.Channels;

/// <summary>
///     邮件通道。发送能力复用框架 IEmailSender（默认实现直连 SMTP，
///     或由 Findx.MailKit 模块替换），SMTP 参数取自 Findx:Email 配置节
/// </summary>
public class EmailNotifyChannel : INotifyChannel
{
    private readonly IEmailSender _emailSender;
    private readonly NotifyOptions _options;
    private readonly ILogger<EmailNotifyChannel> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public EmailNotifyChannel(IEmailSender emailSender, IOptions<NotifyOptions> options, ILogger<EmailNotifyChannel> logger)
    {
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    ///     通道标识
    /// </summary>
    public NotifyChannel Channel => NotifyChannel.Email;

    /// <summary>
    ///     发送
    /// </summary>
    public async Task<NotifyResult> SendAsync(NotifyMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null) return NotifyResult.Fail(Channel, "消息不能为空");

        var recipients = message.To?.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (recipients == null || recipients.Length == 0)
            return NotifyResult.Fail(Channel, "收件人不能为空（NotifyMessage.To 填邮箱地址）");

        try
        {
            //  MailAddressCollection 支持逗号分隔的多收件人，一次发完
            await _emailSender.SendAsync(string.Join(",", recipients),
                string.IsNullOrWhiteSpace(message.Title) ? "系统通知" : message.Title,
                message.Content ?? string.Empty,
                _options.Email.IsBodyHtml, cancellationToken);

            return NotifyResult.Ok(Channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "邮件通知发送失败");
            return NotifyResult.Fail(Channel, ex.Message);
        }
    }
}
