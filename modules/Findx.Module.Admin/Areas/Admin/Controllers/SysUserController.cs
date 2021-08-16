using Findx.AspNetCore.Mvc;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 系统用户控制器
    /// </summary>

    [Area("admin")]
    [Route("[area]/[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "admin")]
    public class SysUserController : CrudControllerBase<SysUserInfo, SysUserInfo, SysUserRequest, SysUserUpdate, SysUserQuery, long>
    {
    }
}
