using Findx.AspNetCore.Mvc;
using Findx.Module.Admin.DTO;
using Findx.Module.Admin.Models;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 系统区域
    /// </summary>
    [Area("api/sys")]
    [Route("[area]/sysArea")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysAreaController : QueryControllerBase<SysAreaInfo, SysAreaInfo, SysQuery, long>
    {
    }
}
