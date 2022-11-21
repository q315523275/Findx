using Findx.Extensions;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Findx.Email
{
    /// <summary>
    /// 邮件发送基类
    /// </summary>
    public abstract class EmailSenderBase : IEmailSender
    {
        /// <summary>
        /// 邮件配置
        /// </summary>
        protected EmailSenderOptions EmailSenderOptions { set; get; }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isBodyHtml"></param>
        /// <param name="token"></param>
        public virtual async Task SendAsync(string to, string subject, string body, bool isBodyHtml = true, CancellationToken token = default)
        {
            await SendAsync(new MailMessage { To = { to }, Subject = subject, Body = body, IsBodyHtml = isBodyHtml }, token: token);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isBodyHtml"></param>
        /// <param name="token"></param>
        public virtual async Task SendAsync(string from, string to, string subject, string body, bool isBodyHtml = true, CancellationToken token = default)
        {
            await SendAsync(new MailMessage(from, to, subject, body) { IsBodyHtml = isBodyHtml }, token: token);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="normalize"></param>
        /// <param name="token"></param>
        public virtual async Task SendAsync(MailMessage mail, bool normalize = true, CancellationToken token = default)
        {
            if (normalize)
            {
                NormalizeMail(mail);
            }
            await SendEmailAsync(mail, token);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected abstract Task SendEmailAsync(MailMessage mail, CancellationToken token = default);

        /// <summary>
        /// 规范电子邮件报文
        /// </summary>
        /// <param name="mail"></param>
        protected virtual void NormalizeMail(MailMessage mail)
        {
            if (mail.From == null || mail.From.Address.IsNullOrEmpty())
            {
                mail.From = new MailAddress(EmailSenderOptions.FromAddress, EmailSenderOptions.FromDisplayName, Encoding.UTF8);
            }

            mail.HeadersEncoding ??= Encoding.UTF8;
            mail.SubjectEncoding ??= Encoding.UTF8;
            mail.BodyEncoding ??= Encoding.UTF8;
        }
    }
}
