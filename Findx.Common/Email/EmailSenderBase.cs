using Findx.Extensions;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Findx.Email
{
    public abstract class EmailSenderBase : IEmailSender
    {
        protected EmailSenderOptions EmailSenderOptions { set; get; }

        public virtual async Task SendAsync(string to, string subject, string body, bool isBodyHtml = true)
        {
            await SendAsync(new MailMessage { To = { to }, Subject = subject, Body = body, IsBodyHtml = isBodyHtml });
        }

        public virtual async Task SendAsync(string from, string to, string subject, string body, bool isBodyHtml = true)
        {
            await SendAsync(new MailMessage(from, to, subject, body) { IsBodyHtml = isBodyHtml });
        }

        public virtual async Task SendAsync(MailMessage mail, bool normalize = true)
        {
            if (normalize)
            {
                NormalizeMail(mail);
            }
            await SendEmailAsync(mail);
        }

        protected abstract Task SendEmailAsync(MailMessage mail);

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
