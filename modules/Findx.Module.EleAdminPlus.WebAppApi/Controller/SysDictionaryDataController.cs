using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Findx.Expressions;
using Findx.Module.EleAdminPlus.Shared.Models;
using Findx.Module.EleAdminPlus.WebAppApi.Dtos.Dictionary;
using Findx.Module.EleAdminPlus.WebAppApi.Vos.Dictionary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.WebAppApi.Controller;

/// <summary>
///     系统-字典数据
/// </summary>
[Area("system")]
[Route("api/[area]/dictionary-data")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-字典数据"), Description("系统-字典数据")]
public class SysDictionaryDataController : CrudControllerBase<SysDictionaryDataInfo, DictionaryDataSimplifyVo, DictionaryDataSaveDto, DictionaryDataPageQueryDto, long, long>
{
    private readonly IRepository<SysDictionaryInfo, long> _dictRepo;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="dictRepo"></param>
    public SysDictionaryDataController(IRepository<SysDictionaryInfo, long> dictRepo)
    {
        _dictRepo = dictRepo;
    }

    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    protected override Expression<Func<SysDictionaryDataInfo, bool>> CreateWhereExpression(DictionaryDataPageQueryDto dto)
    {
        var dictId = dto.DictId;
        if (!dto.DictCode.IsNullOrWhiteSpace())
        {
            var model = _dictRepo.First(x => x.Code == dto.DictCode);
            dictId = model?.Id ?? 0;
        }

        var whereExp = PredicateBuilder.New<SysDictionaryDataInfo>()
                                       .AndIf(dictId > 0, x => x.DictId == dictId)
                                       .AndIf(!dto.Keywords.IsNullOrWhiteSpace(), x => x.Name.Contains(dto.Keywords))
                                       .Build();
        return whereExp;
    }

    /// <summary>
    ///     分页查询
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommonResult<PageResult<List<DictionaryDataSimplifyVo>>>> PageAsync(DictionaryDataPageQueryDto req, CancellationToken cancellationToken = default)
    {
        var rs = await base.PageAsync(req, cancellationToken);
        var dictIds = rs.Data.Rows.Select(x => x.DictId).Distinct().ToList();
        var dictList = await _dictRepo.SelectAsync(x => dictIds.Contains(x.Id), x => new { x.Id, x.Code, x.Name }, cancellationToken: cancellationToken);
        var dict = dictList.ToDictionary(x => x.Id);
        foreach (var item in rs.Data.Rows)
        {
            if (dict.TryGetValue(item.DictId, out var model))
            {
                item.DictCode = model.Code;
                item.DictName = model.Name;
            }
        }
        return rs;
    }

    /// <summary>
    ///      列表查询
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommonResult<List<DictionaryDataSimplifyVo>>> ListAsync([FromQuery] DictionaryDataPageQueryDto dto, CancellationToken cancellationToken = default)
    {
        dto.PageSize = 10000;
        var rs = await base.ListAsync(dto, cancellationToken);
        var dictIds = rs.Data.Select(x => x.DictId).Distinct().ToList();
        var dictList = await _dictRepo.SelectAsync(x => dictIds.Contains(x.Id), x => new { x.Id, x.Code, x.Name }, cancellationToken: cancellationToken);
        var dict = dictList.ToDictionary(x => x.Id);
        foreach (var item in rs.Data)
        {
            if (dict.TryGetValue(item.DictId, out var model))
            {
                item.DictCode = model.Code;
                item.DictName = model.Name;
            }
        }
        return rs;
    }
}
