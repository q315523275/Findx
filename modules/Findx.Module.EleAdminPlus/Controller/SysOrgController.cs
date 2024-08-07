using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Exceptions;
using Findx.Module.EleAdminPlus.Dtos;
using Findx.Module.EleAdminPlus.Dtos.Org;
using Findx.Module.EleAdminPlus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.Controller;

/// <summary>
///     机构服务
/// </summary>
[Area("system")]
[Route("api/[area]/org")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-机构"), Description("系统-机构")]
public class SysOrgController : CrudControllerBase<SysOrgInfo, OrgDto, OrgSaveDto, OrgPageQueryDto, long, long>
{
    /// <summary>
    ///     删除前校验
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    protected override async Task DeleteBeforeAsync(List<long> req)
    {
        var repo = GetRepository<SysOrgInfo, long>();
        var isExist = await repo.ExistAsync(x => req.Contains(x.ParentId));
        if (isExist) throw new FindxException("500", "请先删除下属机构,再删除选中机构");
    }
}