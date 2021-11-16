using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Linq;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    [Area("api/admin")]
    [Route("[area]/sysDictData")]
    public class SysDictDataController : CrudControllerBase<SysDictDataInfo, SysDictDataInfo, SysDictDataRequest, SysDictDataRequest, SysDictDataQuery, long, long>
    {
        protected override Expressionable<SysDictDataInfo> CreatePageWhereExpression(SysDictDataQuery request)
        {
            return ExpressionBuilder.Create<SysDictDataInfo>().AndIF(request.TypeId > 0, x => x.TypeId == request.TypeId);
        }

        protected override MultiOrderBy<SysDictDataInfo> CreatePageOrderExpression(SysDictDataQuery request)
        {
            var multiOrderBy = new MultiOrderBy<SysDictDataInfo>();
            if (typeof(SysDictDataInfo).IsAssignableTo(typeof(ISort)))
                multiOrderBy.OrderBy.Add(new OrderByParameter<SysDictDataInfo> { Expression = it => (it as ISort).Sort, SortDirection = ListSortDirection.Ascending });
            multiOrderBy.OrderBy.Add(new OrderByParameter<SysDictDataInfo> { Expression = it => it.Id, SortDirection = ListSortDirection.Ascending });
            return multiOrderBy;
        }
    }
}
