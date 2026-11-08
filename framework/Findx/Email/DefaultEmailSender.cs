using System.Net.Mail;
using System.Threading.Tasks;
using Findx.Extensions;

namespace Findx.Email;

/// <summary>
///     默认邮件发送者
/// </summary>
public class DefaultEmailSender : EmailSenderBase, IDisposable
{
    private readonly ILogger<DefaultEmailSender> _logger;
    private readonly IDisposable _optionsChangeRegistration;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="optionsMonitor"></param>
    public DefaultEmailSender(ILogger<DefaultEmailSender> logger, IOptionsMonitor<EmailSenderOptions> optionsMonitor)
    {
        _logger = logger;
        EmailSenderOptions = optionsMonitor.CurrentValue;
        _optionsChangeRegistration = optionsMonitor.OnChange(ConfigurationOnChange);
    }

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
        //  发送前快照配置，避免 OnChange 并发替换引用导致 Host/凭据新旧混用
        var options = EmailSenderOptions;
        using var client = new SmtpClient(options.Host, options.Port);
        client.EnableSsl = options.EnableSsl;
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(options.UserName, options.Password);
        await client.SendMailAsync(mail, token);
        _logger.LogDebug("发送邮件到“{JoinAsString}”，标题：{MailSubject}", mail.To.JoinAsString(","), mail.Subject);
    }

    /// <summary>
    ///     发送
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="token"></param>
    protected override async Task SendEmailAsync(IEnumerable<MailMessage> messages, CancellationToken token = default)
    {
        //  发送前快照配置，避免 OnChange 并发替换引用导致 Host/凭据新旧混用
        var options = EmailSenderOptions;
        using var client = new SmtpClient(options.Host, options.Port);
        client.EnableSsl = options.EnableSsl;
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(options.UserName, options.Password);
        foreach (var message in messages)
        {
            await client.SendMailAsync(message, token);
        }
    }

    /// <summary>
    ///     释放
    /// </summary>
    public void Dispose()
    {
        _optionsChangeRegistration?.Dispose();
    }
}