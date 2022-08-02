using Findx.Email;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Findx.WebHost.Controllers
{
    [Description("邮件")]
    public class MailController : Controller
    {
        /// <summary>
        /// 邮件发送示例接口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mailAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        [Description("发送示例邮件")]
        [HttpGet("/email/send")]
        public async Task<string> EmailSend([FromServices] IEmailSender sender, [Required] string mailAddress, [Required] string subject, [Required] string body)
        {
            await sender.SendAsync(mailAddress, subject, body, isBodyHtml: false);
            return "ok";
        }
    }
}
