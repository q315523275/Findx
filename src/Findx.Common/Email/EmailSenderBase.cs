using Findx.Extensions;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Email
{
    public abstract class EmailSenderBase : IEmailSender
    {
        protected EmailSenderOptions EmailSenderOptions { set; get; }

        public virtual async Task SendAsync(string to, string subject, string body, bool isBodyHtml = true, CancellationToken token = default)
        {
            await SendAsync(new MailMessage { To = { to }, Subject = subject, Body = body, IsBodyHtml = isBodyHtml }, token: token);
        }

        public virtual async Task SendAsync(string from, string to, string subject, string body, bool isBodyHtml = true, CancellationToken token = default)
        {
            await SendAsync(new MailMessage(from, to, subject, body) { IsBodyHtml = isBodyHtml }, token: token);
        }

        public virtual async Task SendAsync(MailMessage mail, bool normalize = true, CancellationToken token = default)
        {
            if (normalize)
            {
                NormalizeMail(mail);
            }
            await SendEmailAsync(mail, token);
        }

        protected abstract Task SendEmailAsync(MailMessage mail, CancellationToken token = default);

        protected virtual void NormalizeMail(MailMessage mail)
        {
            if (mail.From == null || mail.From.Address.IsNullOrEmpty())
            {
                mail.From = new MailAddress(EmailSenderOptions?.FromAddress, EmailSenderOptions?.FromDisplayName, Encoding.UTF8);
            }

            if (mail.HeadersEncoding == null)
            {
                mail.HeadersEncoding = Encoding.UTF8;
            }

            if (mail.SubjectEncoding == null)
            {
                mail.SubjectEncoding = Encoding.UTF8;
            }

            if (mail.BodyEncoding == null)
            {
                mail.BodyEncoding = Encoding.UTF8;
            }
        }
    }
}
