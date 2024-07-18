using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.EleAdminPlus.Dtos.Dict;
using Findx.Module.EleAdminPlus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.Controller;

/// <summary>
///     字典值服务
/// </summary>
[Area("system")]
[Route("api/[area]/dictData")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-字典值"), Description("系统-字典值")]
public class SysDictDataController : CrudControllerBase<SysDictDataInfo, DictDataDto, DictDataSaveDto, QueryDictDataDto, long, long>
{
    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    protected override Expression<Func<SysDictDataInfo, bool>> CreatePageWhereExpression(QueryDictDataDto dto)
    {
        var typeId = dto.TypeId;
        if (!dto.TypeCode.IsNullOrWhiteSpace())
        {
            var repo = GetRepository<SysDictTypeInfo, long>();
            var model = repo.First(x => x.Code == dto.TypeCode);
            typeId = model?.Id ?? 0;
        }

        var whereExp = PredicateBuilder.New<SysDictDataInfo>()
                                       .And(x => x.TypeId == x.TypeInfo.Id)
                                       .AndIf(typeId > -1, x => x.TypeId == typeId)
                                       .AndIf(!dto.Keywords.IsNullOrWhiteSpace(), x => x.Name.Contains(dto.Keywords))
                                       .Build();
        return whereExp;
    }

    /// <summary>
    ///      列表查询
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommonResult<List<DictDataDto>>> ListAsync([FromQuery] QueryDictDataDto dto, CancellationToken cancellationToken = default)
    {
        dto.PageSize = 9999;
        return await base.ListAsync(dto, cancellationToken);
    }
}