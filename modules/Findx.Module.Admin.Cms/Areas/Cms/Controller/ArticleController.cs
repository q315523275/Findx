using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Linq;
using Findx.Extensions;
using Findx.Module.Admin.Cms.DTO;
using Findx.Module.Cms.Models;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Findx.Module.Admin.Cms.Areas.Cms.Controller
{
	/// <summary>
    /// 信息
    /// </summary>
	[Area("api/cms")]
	[Route("[area]/article")]
	[ApiExplorerSettings(GroupName = "cms")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class ArticleController : CrudControllerBase<ArticleInfo, ArticleDTO, ArticleDetailDTO, ArticleRequest, ArticleRequest, ArticleQuery, long, long>
	{
        protected override Expressionable<ArticleInfo> CreatePageWhereExpression(ArticleQuery request)
        {
            var repo = GetRepository<ArticleCategoryInfo>();
            var cateIdList = new List<long>();
            if (!request.CategoryId.IsNullOrWhiteSpace())
            {
                var pid = request.CategoryId.To<long>();
                cateIdList = repo.Select(x => x.Id == pid || x.Pids.Contains(request.CategoryId), x => x.Id);
            }
            return ExpressionBuilder.Create<ArticleInfo>().AndIF(cateIdList.Count > 0, x => cateIdList.Contains(x.CategoryId))
                                                          .AndIF(!request.SearchValue.IsNullOrWhiteSpace(), x => x.Title.Contains(request.SearchValue))
                                                          .AndIF(request.SearchStatus > -1, x => x.Status == request.SearchStatus);
        }


        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repo"></param>
        /// <returns></returns>
        [HttpPost("changeStatus")]
        public CommonResult ChangeStatus([FromBody] ArticleRequest request, [FromServices] IRepository<ArticleInfo> repo)
        {
            var updateColums = new List<Expression<Func<ArticleInfo, bool>>>
            {
                x => x.Status == request.Status
            };

            repo.UpdateColumns(updateColums, x => x.Id == request.Id);

            return CommonResult.Success();
        }

        protected override async Task DetailAfterAsync(ArticleInfo model, ArticleDetailDTO dto)
        {
            var repo = GetRepository<ArticleInfo>();
            await repo.UpdateColumnsAsync(x => new ArticleInfo { Click = x.Click + 1 }, x => x.Id == model.Id);
        }
    }
}

