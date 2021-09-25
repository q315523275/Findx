using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Security;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;
using Findx.Module.Admin.Models;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    [Area("api/admin")]
    [Route("[area]/account")]
    public class AccountController : AreaApiControllerBase
    {
        /// <summary>
        /// 查询登录用户信息
        /// </summary>
        /// <param name="fsql"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        [HttpGet("getLoginUser")]
        public CommonResult GetLoginUser([FromServices] IFreeSql fsql, [FromServices] IPrincipal principal)
        {
            var userId = principal.Identity.GetUserId<long>();

            var userInfo = fsql.Select<SysUserInfo>().First(it => it.Id == userId);

            if (userInfo == null)
                return CommonResult.Fail("401", "登录信息失效,请重新登录");


            return CommonResult.Success(null);
        }
    }
