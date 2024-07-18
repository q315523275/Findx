using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Exceptions;
using Findx.Module.EleAdminPlus.Dtos.Dict;
using Findx.Module.EleAdminPlus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.Controller;

/// <summary>
///     字典类型服务
/// </summary>
[Area("system")]
[Route("api/[area]/dictType")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-字典"), Description("系统-字典")]
public class SysDictTypeController : CrudControllerBase<SysDictTypeInfo, DictTypeDto, DictTypeSaveDto, QueryDictTypeDto, long, long>
{
    /// <summary>
    ///     删除前校验
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    protected override async Task DeleteBeforeAsync(List<long> req)
    {
        var repo = GetRepository<SysDictDataInfo, long>();
        var isExist = await repo.ExistAsync(x => req.Contains(x.TypeId));
        if (isExist) throw new FindxException("500", "请先删除字典数据,再删除字典类型");
    }
}