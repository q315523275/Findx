using Findx.AspNetCore.Mvc;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    [Area("api/admin")]
    [Route("[area]/sysConfig")]
    public class SysConfigController : CrudControllerBase<SysConfigInfo, SysConfigInfo, SysConfigRequest, SysConfigRequest, SysConfigQuery, long, long>
    {
    }
}
