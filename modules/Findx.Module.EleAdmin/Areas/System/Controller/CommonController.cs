using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc;
using Findx.Data;

namespace Findx.Module.EleAdmin.Areas.System.Controller
{
    /// <summary>
    /// 登录日志
    /// </summary>
    [Area("system")]
    [Route("api/[area]/common")]
    [Authorize]
    public class CommonController: AreaApiControllerBase
    {
        //[HttpGet("")]
        //public CommonResult Application()
        //{
        //    var res = new
        //    {

        //    };

        //    return CommonResult.Success();
        //}
    }
}

