using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Findx.Email;
using Findx.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Findx.MailKit;

/// <summary>
///     MailKit邮件发送者
/// </summary>
public class MailKitEmailSender : EmailSenderBase
{
    private readonly ILogger<MailKitEmailSender> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="optionsMonitor"></param>
    public MailKitEmailSender(ILogger<MailKitEmailSender> logger, IOptionsMonitor<EmailSenderOptions> optionsMonitor)
    {
        _logger = logger;
        EmailSenderOptions = optionsMonitor.CurrentValue;
        optionsMonitor.OnChange(ConfigurationOnChange);
    }

    /// <summary>
    ///     配置变更
    /// </summary>
    /// <param name="changeOptions"></param>
    private void ConfigurationOnChange(EmailSenderOptions changeOptions)
    {
        EmailSenderOptions = changeOptions;
    }

    /// <summary>
    ///     发送
    /// </summary>
    /// <param name="mail"></param>
    /// <param name="token"></param>
    protected override async Task SendEmailAsync(MailMessage mail, CancellationToken token = default)
    {
        using var message = mail.ToMimeMessage(); // 中文支持 MimeMessage.CreateFromMailMessage(mail);
        message.MessageId = MimeUtils.GenerateMessageId();

        using var client = new SmtpClient();
        
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        client.MessageSent += (s, e) => { _logger.LogDebug($"发送邮件到“{e.Message.To.JoinAsString(",")}”，标题：{e.Message.Subject}，结果：{e.Response}"); };

        await client.ConnectAsync(EmailSenderOptions.Host, EmailSenderOptions.Port, EmailSenderOptions.EnableSsl, token);
        await client.AuthenticateAsync(EmailSenderOptions.UserName, EmailSenderOptions.Password, token);
        await client.SendAsync(message, token);
        await client.DisconnectAsync(true, token);
    }

    /// <summary>
    ///     批量发送
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="cancellationToken"></param>
    protected override async Task SendEmailAsync(IEnumerable<MailMessage> messages, CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient();
        
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        client.MessageSent += (s, e) => { _logger.LogDebug($"发送邮件到“{e.Message.To.JoinAsString(",")}”，标题：{e.Message.Subject}，结果：{e.Response}"); };
        
        await client.ConnectAsync(EmailSenderOptions.Host, EmailSenderOptions.Port, EmailSenderOptions.EnableSsl, cancellationToken);
        await client.AuthenticateAsync(EmailSenderOptions.UserName, EmailSenderOptions.Password, cancellationToken);

        foreach (var message in messages)
        {
            using var mailMessage = message.ToMimeMessage(); // 中文支持 MimeMessage.CreateFromMailMessage(message);
            mailMessage.MessageId = MimeUtils.GenerateMessageId();
            await client.SendAsync(mailMessage, cancellationToken);
        }
        
        await client.DisconnectAsync(true, cancellationToken);
    }
}