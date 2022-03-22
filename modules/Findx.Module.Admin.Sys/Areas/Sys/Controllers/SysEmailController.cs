using System;
using System.Threading.Tasks;
using Findx.Data;
using Findx.Email;
using Findx.Module.Admin.Sys.DTO;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
	[Area("api/sys")]
	[Route("[area]/email")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
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

