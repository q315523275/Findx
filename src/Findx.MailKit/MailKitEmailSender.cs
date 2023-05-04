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

namespace Findx.MailKit
{
    public class MailKitEmailSender : EmailSenderBase
    {
        private readonly ILogger<MailKitEmailSender> _logger;

        public MailKitEmailSender(ILogger<MailKitEmailSender> logger,
            IOptionsMonitor<EmailSenderOptions> optionsMonitor)
        {
            _logger = logger;
            EmailSenderOptions = optionsMonitor.CurrentValue;
            optionsMonitor.OnChange(ConfigurationOnChange);
        }

        private void ConfigurationOnChange(EmailSenderOptions changeOptions)
        {
            EmailSenderOptions = changeOptions;
        }

        protected override async Task SendEmailAsync(MailMessage mail, CancellationToken token = default)
        {
            // var message = mail.ToMimeMessage();
            var message = MimeMessage.CreateFromMailMessage(mail);
            message.MessageId = MimeUtils.GenerateMessageId();

            using var client = new SmtpClient();
            client.MessageSent += (s, e) =>
            {
                _logger.LogDebug($"发送邮件到“{mail.To.JoinAsString(",")}”，标题：{mail.Subject}，结果：{e.Response}");
            };
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(EmailSenderOptions.Host, EmailSenderOptions.Port, EmailSenderOptions.EnableSsl,
                token);
            await client.AuthenticateAsync(EmailSenderOptions.UserName, EmailSenderOptions.Password, token);
            await client.SendAsync(message, token);
            await client.DisconnectAsync(true, token);
        }
    }
}