﻿using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.EleAdmin.Dtos;
using Findx.Module.EleAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Areas.Sys.Controller;

/// <summary>
///     字典值服务
/// </summary>
[Area("system")]
[Route("api/[area]/dictData")]
[Authorize]
[Description("系统-字典值")]
[ApiExplorerSettings(GroupName = "eleAdmin")]
[Tags("系统-字典值")]
public class
    SysDictDataController : CrudControllerBase<SysDictDataInfo, SetDictDataRequest, QueryDictDataRequest, Guid, Guid>
{
    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override Expression<Func<SysDictDataInfo, bool>> CreatePageWhereExpression(QueryDictDataRequest request)
    {
        var typeId = request.TypeId;
        if (!request.TypeCode.IsNullOrWhiteSpace())
        {
            var repo = GetRepository<SysDictTypeInfo>();
            var model = repo.First(x => x.Code == request.TypeCode);
            typeId = model?.Id ?? Guid.Empty;
        }

        var whereExp = PredicateBuilder.New<SysDictDataInfo>()
                                       .AndIf(typeId != Guid.Empty, x => x.TypeId == typeId)
                                       .AndIf(!request.Keywords.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Keywords))
                                       .Build();
        return whereExp;
    }
}