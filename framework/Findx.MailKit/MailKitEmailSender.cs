using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Findx.Email;
using Findx.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        //  发送前一次性快照配置，避免热更新中途割裂
        var options = EmailSenderOptions;

        using var message = mail.ToMimeMessage(); // 中文支持 MimeMessage.CreateFromMailMessage(mail);
        message.MessageId = MimeUtils.GenerateMessageId();

        using var client = new SmtpClient();

        client.ServerCertificateValidationCallback = (s, c, h, e) => !options.EnableSslIgnoreCertificate || e == SslPolicyErrors.None;
        client.MessageSent += (s, e) => { _logger.LogDebug($"发送邮件到“{e.Message.To.JoinAsString(",")}”，标题：{e.Message.Subject}，结果：{e.Response}"); };

        await client.ConnectAsync(options.Host, options.Port, options.EnableSsl, token);
        await client.AuthenticateAsync(options.UserName, options.Password, token);
        try
        {
            await client.SendAsync(message, token);
        }
        finally
        {
            await client.DisconnectAsync(true, token);
        }
    }

    /// <summary>
    ///     批量发送
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="cancellationToken"></param>
    protected override async Task SendEmailAsync(IEnumerable<MailMessage> messages, CancellationToken cancellationToken = default)
    {
        //  发送前一次性快照配置，整批使用同一快照
        var options = EmailSenderOptions;

        using var client = new SmtpClient();

        client.ServerCertificateValidationCallback = (s, c, h, e) => !options.EnableSslIgnoreCertificate || e == SslPolicyErrors.None;
        client.MessageSent += (s, e) => { _logger.LogDebug($"发送邮件到“{e.Message.To.JoinAsString(",")}”，标题：{e.Message.Subject}，结果：{e.Response}"); };

        await client.ConnectAsync(options.Host, options.Port, options.EnableSsl, cancellationToken);
        await client.AuthenticateAsync(options.UserName, options.Password, cancellationToken);
        try
        {
            foreach (var message in messages)
            {
                try
                {
                    using var mailMessage = message.ToMimeMessage(); // 中文支持 MimeMessage.CreateFromMailMessage(message);
                    mailMessage.MessageId = MimeUtils.GenerateMessageId();
                    await client.SendAsync(mailMessage, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "批量发送邮件失败，跳过该收件人继续：{Subject}", message.Subject);
                }
            }
        }
        finally
        {
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}