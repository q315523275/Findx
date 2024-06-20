using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     邮件服务
/// </summary>
[Route("api/mail")]
[Description("邮件服务"), Tags("邮件服务")]
public class MailController : ApiControllerBase
{
    /// <summary>
    ///     邮件发送示例接口
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="mailAddress"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    [Description("发送示例邮件")]
    [HttpGet("send")]
    public async Task<string> EmailSend([FromServices] IEmailSender sender, [Required] string mailAddress,
        [Required] string subject, [Required] string body)
    {
        await sender.SendAsync(mailAddress, subject, body, false);
        return "ok";
    }
}