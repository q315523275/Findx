using System;
using System.Threading.Tasks;
using Findx.Admin.Module.System.DTO;
using Findx.Admin.Module.System.Models;
using Findx.AspNetCore.Mvc;
using Findx.Linq;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Findx.Admin.Module.System.Areas.System.Controller
{
	/// <summary>
    /// 字典值服务
    /// </summary>
	[Area("system")]
	[Route("api/[area]/dictData")]
    [Authorize]
    public class SysDictDataController : CrudControllerBase<SysDictDataInfo, SetDictDataRequest, QueryDictDataRequest, int, int>
	{
        protected override Expressionable<SysDictDataInfo> CreatePageWhereExpression(QueryDictDataRequest request)
        {
            var dictId = request.DictId;
            if (!request.DictCode.IsNullOrWhiteSpace())
            {
                var repo = GetRepository<SysDictTypeInfo>();
                var model = repo.First(x => x.Code == request.DictCode);
                dictId = model?.Id ?? 0;
            }

            var whereExp = ExpressionBuilder.Create<SysDictDataInfo>()
                                            .AndIF(dictId > 0, x => x.DictId == dictId)
                                            .AndIF(!request.Keywords.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Keywords));

            return whereExp;
        }
    }
}

