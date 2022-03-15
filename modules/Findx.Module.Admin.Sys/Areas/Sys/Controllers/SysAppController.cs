using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Exceptions;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.Admin.DTO;
using Findx.Module.Admin.Enum;
using Findx.Module.Admin.Models;
using Findx.Security;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    [Area("api/sys")]
    [Route("[area]/sysApp")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysAppController : CrudControllerBase<SysAppInfo, SysAppInfo, SysAppRequest, SysAppRequest, SysAppQuery, long, long>
    {
        protected override Expressionable<SysAppInfo> CreatePageWhereExpression(SysAppQuery request)
        {
            return ExpressionBuilder.Create<SysAppInfo>().AndIF(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
                                                         .AndIF(!request.Code.IsNullOrWhiteSpace(), x => x.Code.Contains(request.Code));
        }

        protected override async Task AddBeforeAsync(SysAppInfo model)
        {
            var repo = GetRepository<SysAppInfo>();

            var isExist = await repo.ExistAsync(x => x.Name == model.Name || x.Code == model.Code);
            if (isExist)
                throw new FindxException("500", "已存在同名或同编码应用");

            if (model.Active == YesOrNotEnum.Y.ToString())
            {
                isExist = await repo.ExistAsync(u => u.Active == model.Active);
                if (isExist)
                    throw new FindxException("500", "默认激活系统只能有一个");
            }
        }

        protected override async Task EditBeforeAsync(SysAppInfo model)
        {
            var repo = GetRepository<SysAppInfo>();

            var isExist = await repo.ExistAsync(x => (x.Name == model.Name || x.Code == model.Code) && x.Id != model.Id);
            if (isExist)
                throw new FindxException("500", "已存在同名或同编码应用");

            if (model.Active == YesOrNotEnum.Y.ToString())
            {
                isExist = await repo.ExistAsync(u => u.Active == model.Active && u.Id != model.Id);
                if (isExist)
                    throw new FindxException("500", "默认激活系统只能有一个");
            }
        }

        public override async Task<CommonResult> DeleteAsync([FromBody] List<DeleteParam<long>> request)
        {
            Check.NotNull(request, nameof(request));
            if (request.Count == 0)
                return CommonResult.Fail("delete.not.count", "不存在删除数据");

            var repo = GetRepository<SysAppInfo>();
            var repo_menu = GetRepository<SysMenuInfo>();
            var currentUser = GetService<ICurrentUser>();

            Check.NotNull(repo, nameof(repo));
            Check.NotNull(currentUser, nameof(currentUser));

            int total = 0;
            foreach (var item in request)
            {
                var info = await repo.GetAsync(item.Id);

                if (await repo_menu.ExistAsync(u => u.Application == info.Code))
                    return CommonResult.Fail("delete.not.count", "该应用下有菜单禁止删除");

                if (repo.Delete(key: item.Id) > 0)
                    total++;
            }
            return CommonResult.Success($"共删除{total}条数据,失败{request.Count - total}条");
        }

        /// <summary>
        /// 设置默认
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("setAsDefault")]
        public CommonResult SetAsDefault([FromBody] SysAppRequest request)
        {
            Check.NotNull(request, nameof(request));
            var repo = GetRepository<SysAppInfo>();
            Check.NotNull(repo, nameof(repo));

            var updateColums = new List<Expression<Func<SysAppInfo, bool>>> { x => x.Active == YesOrNotEnum.N.ToString() };
            repo.UpdateColumns(updateColums, x => x.Status == CommonStatusEnum.ENABLE.To<int>());
            repo.UpdateColumns(x => new SysAppInfo { Active = YesOrNotEnum.Y.ToString() }, x => x.Id == request.Id);

            return CommonResult.Success();
        }
    }
}
