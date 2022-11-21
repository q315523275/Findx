using Findx.Extensions;
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
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="optionsMonitor"></param>
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

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="token"></param>
        protected override async Task SendEmailAsync(MailMessage mail, CancellationToken token = default)
        {
            using SmtpClient client = new SmtpClient(EmailSenderOptions.Host, EmailSenderOptions.Port)
            {
                EnableSsl = EmailSenderOptions.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(EmailSenderOptions.UserName, EmailSenderOptions.Password)
            };
            await client.SendMailAsync(mail);
            _logger.LogDebug("发送邮件到“{JoinAsString}”，标题：{MailSubject}", mail.To.JoinAsString(","), mail.Subject);
        }
    }
}
