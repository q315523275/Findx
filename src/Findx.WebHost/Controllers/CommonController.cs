using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Findx.WebHost.Controllers
{
    [Area("sample")]
    public class CommonController : AreaApiControllerBase
    {
        [HttpGet]
        [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
        public CommonResult<object> Common([FromServices] IPermissionStore store)
        {
            return CommonResult.Success<object>(store.GetFromStoreAsync().Result);
        }

        [HttpGet]
        public object Monitor()
        {
            return DateTime.Now;
        }
    }
}
