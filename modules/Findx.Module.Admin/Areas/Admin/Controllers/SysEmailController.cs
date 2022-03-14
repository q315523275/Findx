using System;
using System.Threading.Tasks;
using Findx.Data;
using Findx.Email;
using Findx.Module.Admin.Areas.Admin.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
	[Area("api/admin")]
	[Route("[area]/email")]
	public class SysEmailController : ControllerBase
	{
        private readonly IEmailSender _sender;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sender"></param>
        public SysEmailController(IEmailSender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("sendEmail")]
		public async Task<CommonResult> SendEmail([FromBody] SendEmailRequest req)
        {
            await _sender.SendAsync(req.To, req.Title, req.Content, isBodyHtml: false);

            return CommonResult.Success();
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("sendEmailHtml")]
        public async Task<CommonResult> SendEmailHtml([FromBody] SendEmailRequest req)
        {
            await _sender.SendAsync(req.To, req.Title, req.Content);

            return CommonResult.Success();
        }
    }
}

