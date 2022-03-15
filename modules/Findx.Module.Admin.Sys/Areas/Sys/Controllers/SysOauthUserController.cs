using Findx.AspNetCore.Mvc;
using Findx.Module.Admin.DTO;
using Findx.Module.Admin.Models;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    [Area("api/sys")]
    [Route("[area]/sysOauthUser")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysOauthUserController : CrudControllerBase<SysOauthUserInfo, SysOauthUserInfo, SysOauthUserInfo, SysOauthUserInfo, SysQuery, long, long>
    {
    }
}
