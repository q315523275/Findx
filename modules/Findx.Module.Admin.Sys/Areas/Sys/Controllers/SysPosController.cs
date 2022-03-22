using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Exceptions;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.Admin.Sys.DTO;
using Findx.Module.Admin.Enum;
using Findx.Module.Admin.Models;
using Findx.Security;
using Findx.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Findx.Module.Admin.Areas.Sys.Controllers
{
    /// <summary>
    /// 系统职位
    /// </summary>
    [Area("api/sys")]
    [Route("[area]/sysPos")]
    [ApiExplorerSettings(GroupName = "system")]
    [Authorize(Policy = PermissionRequirement.Policy, Roles = "admin")]
    public class SysPosController : CrudControllerBase<SysPosInfo, SysPosOutput, SysPosRequest, SysPosRequest, SysPosQuery, long, long>
    {
        protected override Expressionable<SysPosInfo> CreatePageWhereExpression(SysPosQuery request)
        {
            return ExpressionBuilder.Create<SysPosInfo>().AndIF(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
                                                         .AndIF(!request.Code.IsNullOrWhiteSpace(), x => x.Code.Contains(request.Code))
                                                         .And(x => x.Status == CommonStatusEnum.ENABLE.To<int>());
        }

        /// <summary>
        /// 构建排序规则
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override List<OrderByParameter<SysPosInfo>> CreatePageOrderExpression(SysPosQuery request)
        {
            var multiOrderBy = new List<OrderByParameter<SysPosInfo>>();
            if (typeof(SysPosInfo).IsAssignableTo(typeof(ISort)))
                multiOrderBy.Add(new OrderByParameter<SysPosInfo> { Expression = it => (it as ISort).Sort, SortDirection = ListSortDirection.Ascending });
            multiOrderBy.Add(new OrderByParameter<SysPosInfo> { Expression = it => it.Id, SortDirection = ListSortDirection.Ascending });
            return multiOrderBy;
        }

        protected override async Task AddBeforeAsync(SysPosInfo model)
        {
            var repo = GetRepository<SysPosInfo>();

            var isExist = await repo.ExistAsync(x => x.Name == model.Name || x.Code == model.Code);
            if (isExist)
                throw new FindxException("4049", "已存在同名或同编码职位");
        }

        protected override async Task EditBeforeAsync(SysPosInfo model)
        {
            var repo = GetRepository<SysPosInfo>();

            var isExist = await repo.ExistAsync(x => (x.Name == model.Name || x.Code == model.Code) && x.Id != model.Id);
            if (isExist)
                throw new FindxException("4049", "已存在同名或同编码职位");
        }

        public override async Task<CommonResult> DeleteAsync([FromBody] List<DeleteParam<long>> request)
        {
            Check.NotNull(request, nameof(request));
            if (request.Count == 0)
                return CommonResult.Fail("delete.not.count", "不存在删除数据");

            var repo = GetRepository<SysPosInfo>();
            var repo_emp_pos = GetRepository<SysEmpPosInfo>();
            var repo_emp_ext_org_pos = GetRepository<SysEmpExtOrgPosInfo>();
            var currentUser = GetService<ICurrentUser>();

            Check.NotNull(repo, nameof(repo));
            Check.NotNull(currentUser, nameof(currentUser));

            int total = 0;
            foreach (var item in request)
            {
                // 该职位下是否有员工
                var hasPosEmp = await repo_emp_pos.ExistAsync(x => x.PosId == item.Id);
                if (hasPosEmp)
                    return CommonResult.Fail("delete.no.allow", "该职位下有员工禁止删除");

                // 该附属职位下是否有员工
                var hasExtPosEmp = await repo_emp_ext_org_pos.ExistAsync(x => x.PosId == item.Id);
                if (hasExtPosEmp)
                    return CommonResult.Fail("delete.no.allow", "该职位下有员工禁止删除");

                if (repo.Delete(key: item.Id) > 0)
                    total++;
            }

            return CommonResult.Success($"共删除{total}条数据,失败{request.Count - total}条");
        }
    }
}
