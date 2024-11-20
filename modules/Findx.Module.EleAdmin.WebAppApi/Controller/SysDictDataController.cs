using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Expressions;
using Findx.Extensions;
using Findx.Module.EleAdmin.Dtos.Dict;
using Findx.Module.EleAdmin.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Controller;

/// <summary>
///     字典值服务
/// </summary>
[Area("system")]
[Route("api/[area]/dictData")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-字典值"), Description("系统-字典值")]
public class SysDictDataController : CrudControllerBase<SysDictDataInfo, DictDataSaveDto, DictDataQueryDto, Guid, Guid>
{
    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override Expression<Func<SysDictDataInfo, bool>> CreateWhereExpression(DictDataQueryDto request)
    {
        var typeId = request.TypeId;
        if (!request.TypeCode.IsNullOrWhiteSpace())
        {
            var repo = GetRepository<SysDictTypeInfo>();
            var model = repo.First(x => x.Code == request.TypeCode);
            typeId = model?.Id ?? Guid.Empty;
        }

        var whereExp = PredicateBuilder.New<SysDictDataInfo>()
            .And(x => x.TypeId == x.TypeInfo.Id)
            .AndIf(typeId != Guid.Empty, x => x.TypeId == typeId)
            .AndIf(!request.Keywords.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Keywords))
            .Build();
        return whereExp;
    }

    /// <summary>
    ///      列表查询
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommonResult<List<SysDictDataInfo>>> ListAsync([FromQuery] DictDataQueryDto dto, CancellationToken cancellationToken = default)
    {
        dto.PageSize = 9999;
        return await base.ListAsync(dto, cancellationToken);
    }
}