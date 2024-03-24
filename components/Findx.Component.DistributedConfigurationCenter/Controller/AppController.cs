using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Component.DistributedConfigurationCenter.Dtos;
using Findx.Component.DistributedConfigurationCenter.Models;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Component.DistributedConfigurationCenter.Controller;

/// <summary>
/// 配置服务App
/// </summary>
[Area("findx")]
[Route("api/config/app")]
[Authorize]
[Description("配置服务App")]
[ApiExplorerSettings(GroupName = "config"), Tags("配置服务-App")]
public class AppController: CrudControllerBase<AppInfo, AppInfo, CreateAppDto, UpdateAppDto, QueryAppDto, long, long>
{
    /// <summary>
    /// 创建查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override Expression<Func<AppInfo, bool>> CreatePageWhereExpression(QueryAppDto request)
    {
        var whereExp = PredicateBuilder.New<AppInfo>()
                                       .AndIf(!request.AppId.IsNullOrWhiteSpace(), x => request.AppId == x.AppId)
                                       .AndIf(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
                                       .Build();
        return whereExp;
    }

    /// <summary>
    /// 创建排序条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override List<OrderByParameter<AppInfo>> CreatePageOrderExpression(QueryAppDto request)
    {
        return DataSortBuilder.New<AppInfo>().OrderByDescending(x => x.Id).Build();
    }
}