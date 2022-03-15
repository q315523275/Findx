using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Linq;
using Findx.Module.Admin.DTO;
using Findx.Module.Admin.Models;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    [Area("api/sys")]
    [Route("[area]/sysDictData")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysDictDataController : CrudControllerBase<SysDictDataInfo, SysDictDataInfo, SysDictDataRequest, SysDictDataRequest, SysDictDataQuery, long, long>
    {

        /// <summary>
        /// 构建查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Expressionable<SysDictDataInfo> CreatePageWhereExpression(SysDictDataQuery request)
        {
            return ExpressionBuilder.Create<SysDictDataInfo>().AndIF(request.TypeId > 0, x => x.TypeId == request.TypeId)
                                                              .AndIF(!request.Value.IsNullOrWhiteSpace(), x => x.Value.Contains(request.Value))
                                                              .AndIF(!request.Code.IsNullOrWhiteSpace(), x => x.Code.Contains(request.Code));
        }

        /// <summary>
        /// 构建排序规则
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override List<OrderByParameter<SysDictDataInfo>> CreatePageOrderExpression(SysDictDataQuery request)
        {
            var multiOrderBy = new List<OrderByParameter<SysDictDataInfo>>();
            if (typeof(SysDictDataInfo).IsAssignableTo(typeof(ISort)))
                multiOrderBy.Add(new OrderByParameter<SysDictDataInfo> { Expression = it => (it as ISort).Sort, SortDirection = ListSortDirection.Ascending });
            multiOrderBy.Add(new OrderByParameter<SysDictDataInfo> { Expression = it => it.Id, SortDirection = ListSortDirection.Ascending });
            return multiOrderBy;
        }
    }
}
