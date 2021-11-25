using Findx.AspNetCore.Mvc;
using Findx.Module.Admin.Areas.Admin.DTO;
using Findx.Module.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Findx.Extensions;
using Findx.Data;
using System.Security.Principal;
using Findx.Module.Admin.Internals;
using Findx.Security;
using System.Collections.Generic;
using Findx.Mapping;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Findx.Module.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 系统用户
    /// </summary>

    [Area("api/admin")]
    [Route("[area]/sysUser")]
    public class SysUserController : CrudControllerBase<SysUserInfo, SysUserOutput, SysUserCreateRequest, SysUserUpdateRequest, SysUserQuery, long, long>
    {
        private readonly IFreeSql fsql;
        private readonly IPrincipal principal;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="fsql"></param>
        /// <param name="principal"></param>
        public SysUserController(IFreeSql fsql, IPrincipal principal)
        {
            this.fsql = fsql;
            this.principal = principal;
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repositor"></param>
        /// <returns></returns>
        [HttpGet("page")]
        public override async Task<CommonResult> PageAsync([FromQuery] SysUserQuery request)
        {
            Check.NotNull(request, nameof(request));

            var userId = principal.Identity.GetUserId<long>();

            var userInfo = fsql.Select<SysUserInfo>(userId).First();
            if (userInfo == null)
                return CommonResult.Fail("401", "登录信息失效,请重新登录");

            var pid = request.SysEmpParam != null && !request.SysEmpParam.OrgId.IsNullOrWhiteSpace() ? request.SysEmpParam.OrgId.To<long>() : 0;
            var key = $"[{request.SysEmpParam?.OrgId}]";

            var userList = await fsql.Select<SysUserInfo, SysEmpInfo, SysOrgInfo>()
                                     .InnerJoin((u, e, o) => u.Id == e.Id)
                                     .InnerJoin((u, e, o) => e.OrgId == o.Id)
                                     .WhereIf(!request.SearchValue.IsNullOrWhiteSpace(), (u, e, o) => u.Account.Contains(request.SearchValue) || u.Name.Contains(request.SearchValue) || u.Phone.Contains(request.SearchValue))
                                     .WhereIf(pid > 0, (u, e, o) => e.OrgId == pid || o.Pids.Contains(key))
                                     .WhereIf(request.SearchStatus > -1, (u, e, o) => u.Status == request.SearchStatus)
                                     .WhereIf(!userInfo.IsSuperAdmin(), (u, e, o) => u.AdminType != 1)
                                     .Count(out var totalRows)
                                     .Page(request.PageNo, request.PageSize)
                                     .ToListAsync<SysUserOutput>();

            return CommonResult.Success(new PageResult<List<SysUserOutput>>(request.PageNo, request.PageSize, (int)totalRows, userList));
        }

        /// <summary>
        /// 详情结果转换
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected override SysUserOutput ToDto(SysUserInfo result)
        {
            var userOutput = result.MapTo<SysUserOutput>();
            Check.NotNull(userOutput, nameof(userOutput));

            var sysEmpInfo = fsql.Select<SysEmpInfo>().Where(it => it.Id == userOutput.Id).First<SysEmpOutput>();
            if (sysEmpInfo == null)
                return userOutput;

            userOutput.SysEmpInfo = sysEmpInfo;
            userOutput.SysEmpInfo.ExtOrgPos = fsql.Select<SysEmpExtOrgPosInfo>().Where(it => it.EmpId == userOutput.Id).ToList<SysEmpExtOrgPosOutput>();
            userOutput.SysEmpInfo.Positions = fsql.Select<SysEmpPosInfo>().Where(it => it.EmpId == userOutput.Id).ToList<SysEmpPosOutput>();

            return userOutput;
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repo"></param>
        /// <returns></returns>
        [HttpPost("changeStatus")]
        public CommonResult ChangeStatus([FromBody] SysUserStatusUpdateRequest request, [FromServices] IRepository<SysUserInfo> repo)
        {
            // 用户信息
            var userInfo = fsql.Select<SysUserInfo>(request.Id).First();
            if (userInfo == null)
                return CommonResult.Fail("404", "用户不存在");

            if (!Enum.IsDefined(typeof(CommonStatusEnum), request.Status))
                return CommonResult.Fail("404", "字典状态错误");

            if (userInfo.IsSuperAdmin())
                return CommonResult.Fail("403", "无权限");

            var updateColums = new List<Expression<Func<SysUserInfo, bool>>>
            {
                x => x.Status == request.Status
            };

            repo.UpdateColumns(updateColums, x => x.Id == request.Id);

            return CommonResult.Success();
        }

        /// <summary>
        /// 获取用户选择器
        /// </summary>
        /// <returns></returns>
        [HttpGet("selector")]
        public async Task<CommonResult> Selector([FromQuery] SysUserQuery request)
        {

            var userId = principal.Identity.GetUserId<long>();

            var userInfo = fsql.Select<SysUserInfo>(userId).First();
            if (userInfo == null)
                return CommonResult.Fail("401", "登录信息失效,请重新登录");

            var userList = await fsql.Select<SysUserInfo>()
                                     .WhereIf(!request.SearchValue.IsNullOrWhiteSpace(), x => x.Name.Contains(request.SearchValue))
                                     .Where(x => x.AdminType != 1)
                                     .ToListAsync(x => new { x.Id, x.Name });

            return CommonResult.Success(userList);
        }
    }
}
