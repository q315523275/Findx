﻿using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Exceptions;
using Findx.Module.EleAdmin.Dtos;
using Findx.Module.EleAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Areas.System.Controller;

/// <summary>
/// 字典类型服务
/// </summary>
[Area("system")]
[Route("api/[area]/dictType")]
[Authorize]
[Description("系统-字典")]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-字典")]
public class SysDictTypeController : CrudControllerBase<SysDictTypeInfo, SetDictTypeRequest, QueryDictTypeRequest, Guid, Guid>
{
    /// <summary>
    /// 删除前校验
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    protected override async Task DeleteBeforeAsync(List<Guid> req)
    {
        var repo = GetRepository<SysDictDataInfo>();
        var isExist = await repo.ExistAsync(x => req.Contains(x.TypeId));
        if (isExist)
           throw new FindxException("500", "请先删除字典数据,再删除字典类型");
    }
}