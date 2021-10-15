using Findx.Data;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Findx.WebHost.Controllers
{
    public class PermissionController: Controller
    {
        /// <summary>
        /// 权限数据查询示例接口
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("/permission/list")]
        public async Task<CommonResult> PermissionList([FromServices] IPermissionStore store)
        {
            var res = await store.GetFromStoreAsync();
            return CommonResult.Success(res);
        }

        /// <summary>
        /// 接口权限校验接口
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet("/permission/verify")]
        [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
        public async Task<CommonResult> VerifyPermission([FromServices] IPermissionStore store)
        {
            var res = await store.GetFromStoreAsync();
            return CommonResult.Success(res);
        }
    }
}
