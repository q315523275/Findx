using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Exceptions;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Internals;
using Findx.Module.Admin.Models;
using Findx.Security;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 系统职位
    /// </summary>
    [Area("api/admin")]
    [Route("[area]/sysPos")]
    public class SysPosController : CrudControllerBase<SysPosInfo, SysPosOutput, SysPosRequest, SysPosRequest, SysPosQuery, long, long>
    {
        protected override Expressionable<SysPosInfo> CreatePageWhereExpression(SysPosQuery request)
        {
            return ExpressionBuilder.Create<SysPosInfo>().AndIF(!request.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(request.Name))
                                                         .AndIF(!request.Code.IsNullOrWhiteSpace(), x => x.Code.Contains(request.Code))
                                                         .And(x => x.Status == CommonStatusEnum.ENABLE.To<int>());
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
