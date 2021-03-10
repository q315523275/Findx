using Findx.DependencyInjection;
using Findx.Email;
using Findx.Extensions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Findx.MailKit
{
    [Dependency(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton, ReplaceServices = true)]
    public class MailKitEmailSender : EmailSenderBase
    {
        private readonly ILogger<MailKitEmailSender> _logger;
        public MailKitEmailSender(ILogger<MailKitEmailSender> logger, IOptionsMonitor<EmailSenderOptions> optionsMonitor)
        {
            _logger = logger;
            EmailSenderOptions = optionsMonitor.CurrentValue;
            optionsMonitor.OnChange(ConfigurationOnChange);
        }
        private void ConfigurationOnChange(EmailSenderOptions changeOptions)
        {
            EmailSenderOptions = changeOptions;
        }

        protected override async Task SendEmailAsync(System.Net.Mail.MailMessage mail)
        {
            using (var client = new SmtpClient())
            {
                client.Connect(EmailSenderOptions.Host, EmailSenderOptions.Port, EmailSenderOptions.EnableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable);
                client.Authenticate(EmailSenderOptions.UserName, EmailSenderOptions.Password);

                var message = mail.ToMimeMessage();
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogDebug($"发送邮件到“{mail.To.JoinAsString(",")}”，标题：{mail.Subject}");
            }
        }
    }
}
