using Findx.AspNetCore.Mvc;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    [Area("api/admin")]
    [Route("[area]/sysApp")]
    public class SysAppController : CrudControllerBase<SysAppInfo, SysAppInfo, SysAppRequest, SysAppRequest, SysAppQuery, long, long>
    {
    }
}
