using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Exceptions;
using Findx.Module.EleAdminPlus.Shared.Models;
using Findx.Module.EleAdminPlus.WebAppApi.Dtos.Dictionary;
using Findx.Module.EleAdminPlus.WebAppApi.Vos.Dictionary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.WebAppApi.Controller;

/// <summary>
///     系统-字典
/// </summary>
[Area("system")]
[Route("api/[area]/dictionary")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-字典"), Description("系统-字典")]
public class SysDictionaryController : CrudControllerBase<SysDictionaryInfo, DictionarySimplifyVo, DictionarySaveDto, DictionaryPageQueryDto, long, long>
{
    /// <summary>
    ///      列表查询
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommonResult<List<DictionarySimplifyVo>>> ListAsync([FromQuery] DictionaryPageQueryDto dto, CancellationToken cancellationToken = default)
    {
        dto.PageSize = 10000;
        return await base.ListAsync(dto, cancellationToken);
    }
    
    /// <summary>
    ///     删除前校验
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task DeleteBeforeAsync(List<long> req, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysDictionaryDataInfo, long>();
        var isExist = await repo.ExistAsync(x => req.Contains(x.DictId), cancellationToken);
        if (isExist) throw new FindxException("500", "请先删除字典数据,再删除字典类型");
    }
}