using Findx.AspNetCore.Mvc;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 系统职位
    /// </summary>
    [Area("api/admin")]
    [Route("[area]/sysPos")]
    public class SysPosController : CrudControllerBase<SysPosInfo, SysPosInfo, SysPosRequest, SysPosRequest, SysPosQuery, long, long>
    {
    }
}
