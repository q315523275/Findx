using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Exceptions;
using Findx.Linq;
using Findx.Module.Admin.Cms.DTO;
using Findx.Module.Cms.Models;
using Findx.Security;
using Findx.Security.Authorization;
using Findx.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Findx.Module.Admin.Cms.Areas.Cms.Controller
{
	/// <summary>
    /// 信息类别
    /// </summary>
	[Area("api/cms")]
	[Route("[area]/articleCategory")]
	[ApiExplorerSettings(GroupName = "cms")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class ArticleCategoryController : CrudControllerBase<ArticleCategoryInfo, ArticleCategoryOutput, ArticleCategoryRequest, ArticleCategoryRequest, ArticleCategoryQuery, long, long>
	{

        /// <summary>
        /// 构建查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Expressionable<ArticleCategoryInfo> CreatePageWhereExpression(ArticleCategoryQuery req)
        {
            return ExpressionBuilder.Create<ArticleCategoryInfo>().AndIF(req.Status.HasValue, x => x.Status == req.Status.Value)
                                                              .AndIF(req.ChannelId.HasValue, x => x.ChannelId == req.ChannelId.Value);
        }

        /// <summary>
        /// 树形数据列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public override async Task<CommonResult> ListAsync([FromQuery] ArticleCategoryQuery request)
        {
            Check.NotNull(request, nameof(request));

            var repo = GetRepository<ArticleCategoryInfo>();

            Check.NotNull(repo, nameof(repo));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var list = await repo.SelectAsync<ArticleCategoryOutput>(whereExpression: whereExpression?.ToExpression(), orderParameters: orderByExpression.ToArray());

            return CommonResult.Success(new TreeBuilder<ArticleCategoryOutput, long>().Build(list, 0));
        }

        /// <summary>
        /// 获取信息类别菜单树，用于新增、编辑时选择上级节点
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("tree")]
        public async Task<CommonResult> GetCategoryTree([FromQuery] ArticleCategoryQuery req, [FromServices] IRepository<ArticleCategoryInfo> repo)
        {
            var whereExpression = CreatePageWhereExpression(req);

            var menus = await repo.SelectAsync(whereExpression.ToExpression()
                                        , selectExpression: x => new ArticleCategoryTreeNode
                                        {
                                            Id = x.Id,
                                            ParentId = x.Pid,
                                            Value = x.Id.ToString(),
                                            Title = x.Title,
                                            Weight = x.Sort
                                        }
                                        , orderParameters: CreatePageOrderExpression(req).ToArray());
            return CommonResult.Success(new TreeBuilder<ArticleCategoryTreeNode, long>().Build(menus, 0));
        }

        /// <summary>
        /// 创建Pids格式
        /// 如果pid是0顶级节点，pids就是 [0];
        /// 如果pid不是顶级节点，pids就是 pid菜单的 pids + [pid] + ,
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private async Task<string> CreateNewPids(long pid)
        {
            if (pid == 0L)
            {
                return "[0],";
            }
            else
            {
                var model = await GetRepository<ArticleCategoryInfo>().FirstAsync(u => u.Id == pid);
                return model.Pids + "[" + pid + "],";
            }
        }

        protected override async Task AddBeforeAsync(ArticleCategoryInfo model)
        {
            var repo = GetRepository<ArticleCategoryInfo>();
            var currentUser = GetService<ICurrentUser>();

            var isExist = await repo.ExistAsync(u => u.CallIndex == model.CallIndex);
            if (isExist)
                throw new FindxException("D2002", "已有相同唯一编码相同");

            model.Pids = await CreateNewPids(model.Pid);
        }

        protected override async Task EditBeforeAsync(ArticleCategoryInfo model)
        {
            model.Pids = await CreateNewPids(model.Pid);
        }

        protected override Task DeleteBeforeAsync(List<DeleteParam<long>> req)
        {
            return base.DeleteBeforeAsync(req);
        }
    }
}

