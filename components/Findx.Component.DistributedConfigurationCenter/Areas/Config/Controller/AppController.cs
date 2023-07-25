using System;
using System.Collections.Generic;
using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Component.DistributedConfigurationCenter.Dtos;
using Findx.Component.DistributedConfigurationCenter.Models;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Component.DistributedConfigurationCenter.Areas.Config.Controller;

/// <summary>
/// 配置服务App
/// </summary>
[Area("findx")]
[Route("api/config/app")]
[Description("配置服务App")]
[ApiExplorerSettings(GroupName = "config"), Tags("配置服务-App")]
public class AppController: CrudControllerBase<AppInfo, AppInfo, CreateAppDto, UpdateAppDto, QueryAppDto, Guid, Guid>
{
    /// <summary>
    /// 创建查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override Expressionable<AppInfo> CreatePageWhereExpression(QueryAppDto request)
    {
        var whereExp = ExpressionBuilder.Create<AppInfo>()
                                        .AndIF(!request.AppId.IsNullOrWhiteSpace(), x => request.AppId == x.AppId)
                                        .AndIF(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name));

        return whereExp;
    }

    /// <summary>
    /// 创建排序条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override List<OrderByParameter<AppInfo>> CreatePageOrderExpression(QueryAppDto request)
    {
        return ExpressionBuilder.CreateOrder<AppInfo>().OrderByDescending(x => x.Id).ToSort();
    }
}