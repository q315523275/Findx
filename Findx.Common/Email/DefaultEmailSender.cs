using Findx.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Findx.Email
{
    /// <summary>
    /// 默认邮件发送者
    /// </summary>
    public class DefaultEmailSender : EmailSenderBase
    {
        private readonly ILogger<DefaultEmailSender> _logger;
        public DefaultEmailSender(ILogger<DefaultEmailSender> logger, IOptionsMonitor<EmailSenderOptions> optionsMonitor)
        {
            _logger = logger;
            EmailSenderOptions = optionsMonitor.CurrentValue;
            optionsMonitor.OnChange(ConfigurationOnChange);
        }
        private void ConfigurationOnChange(EmailSenderOptions changeOptions)
        {
            EmailSenderOptions = changeOptions;
        }
        protected override async Task SendEmailAsync(MailMessage mail)
        {
            using SmtpClient client = new SmtpClient(EmailSenderOptions.Host, EmailSenderOptions.Port)
            {
                UseDefaultCredentials = true,
                EnableSsl = EmailSenderOptions.EnableSsl,
                Credentials = new NetworkCredential(EmailSenderOptions.UserName, EmailSenderOptions.Password)
            };
            await client.SendMailAsync(mail);
            _logger.LogDebug($"发送邮件到“{mail.To.JoinAsString(",")}”，标题：{mail.Subject}");
        }
    }
}
