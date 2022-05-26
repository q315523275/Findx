using System;
using System.Threading.Tasks;
using System.Linq;
using Findx.Admin.Module.System.DTO;
using Findx.Admin.Module.System.Models;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using Findx.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Principal;

namespace Findx.Admin.Module.System.Areas.System.Controller
{
    /// <summary>
    /// 用户服务
    /// </summary>
    [Area("system")]
    [Route("api/[area]/user")]
    [Authorize]
    public class SysUserController : CrudControllerBase<SysUserInfo, UserDto, SetUserRequest, QueryUserRequest, int, int>
    {
        protected override Expressionable<SysUserInfo> CreatePageWhereExpression(QueryUserRequest request)
        {
            return base.CreatePageWhereExpression(request);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("page")]
        public override async Task<CommonResult> PageAsync([FromQuery] QueryUserRequest request)
        {
            Check.NotNull(request, nameof(request));

            var repo = GetRepository<SysUserInfo>();
            var role_repo = GetRepository<SysUserRoleInfo>();

            Check.NotNull(repo, nameof(repo));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var result = await repo.PagedAsync<UserDto>(request.PageNo, request.PageSize, whereExpression: whereExpression?.ToExpression(), orderParameters: orderByExpression.ToArray());
            var ids = result.Rows.Select(x => x.Id).Distinct();
            var roles = role_repo.Select(x => x.RoleInfo.Id == x.RoleId && ids.Contains(x.UserId));
            foreach (var item in result.Rows)
            {
                item.Roles = roles.Where(x => x.UserId == item.Id && x.RoleInfo != null).Select(x => x.RoleInfo);
            }
            return CommonResult.Success(result);
        }

        [HttpPut("status")]
        public CommonResult Status([FromBody] SetUserPropertyRequest req)
        {
            var repo = GetRepository<SysUserInfo>();
            repo.UpdateColumns(x => new SysUserInfo { Status = req.Status }, x => x.Id == req.Id);
            return CommonResult.Success();
        }

        [HttpPut("password")]
        public CommonResult Password([FromBody] SetUserPropertyRequest req)
        {
            var repo = GetRepository<SysUserInfo>();
            var pwd = Findx.Utils.Encrypt.Md5By32(req.Password);
            repo.UpdateColumns(x => new SysUserInfo { Password = pwd }, x => x.Id == req.Id);
            return CommonResult.Success();
        }

        [HttpGet("existence")]
        public CommonResult Existence([Required] string field, [Required] string value, int id)
        {
            var whereExp = ExpressionBuilder.Create<SysUserInfo>()
                                            .AndIF(field == "userName", x => x.UserName == value)
                                            .And(x => x.Id != id)
                                            .ToExpression();
            var repo = GetRepository<SysUserInfo>();
            if (repo.Exist(whereExp))
            {
                return CommonResult.Success();
            }
            else
            {
                return CommonResult.Fail("404", "账号不存在");
            }
        }

        protected override Task DetailAfterAsync(SysUserInfo model, UserDto dto)
        {
            var role_repo = GetRepository<SysUserRoleInfo>();
            var roles = role_repo.Select(x => x.RoleInfo.Id == x.RoleId && x.UserId == model.Id);
            dto.Roles = roles.Where(x => x.RoleInfo != null).Select(x => x.RoleInfo);
            return Task.CompletedTask;
        }

        protected override async Task AddBeforeAsync(SysUserInfo model, SetUserRequest req)
        {
            if (!req.Password.IsNullOrWhiteSpace())
            {
                model.Password = Findx.Utils.Encrypt.Md5By32(req.Password);
            }

            await base.AddBeforeAsync(model, req);
        }

        protected override async Task AddAfterAsync(SysUserInfo model, SetUserRequest req, int result)
        {
            if (result > 0)
            {
                var repo = HttpContext.RequestServices.GetService<IRepository<SysUserInfo>>();
                var role_repo = HttpContext.RequestServices.GetService<IRepository<SysUserRoleInfo>>();
                var principal = HttpContext.RequestServices.GetService<IPrincipal>();

                var user = repo.First(x => x.UserName == req.UserName);
                if (user != null)
                {
                    var list = req.Roles.Select(x => new SysUserRoleInfo { RoleId = x.Id, UserId = user.Id, TenantId = Findx.Data.Tenant.TenantId.Value });
                    role_repo.Insert(list);
                }
            }
            await base.AddAfterAsync(model, req, result);
        }

        protected override async Task EditBeforeAsync(SysUserInfo model, SetUserRequest req)
        {
            model.Password = null;
            await base.EditBeforeAsync(model, req);
        }

        protected override async Task EditAfterAsync(SysUserInfo model, SetUserRequest req, int result)
        {
            if (result > 0)
            {
                var role_repo = HttpContext.RequestServices.GetService<IRepository<SysUserRoleInfo>>();
                var principal = HttpContext.RequestServices.GetService<IPrincipal>();

                var list = req.Roles.Select(x => new SysUserRoleInfo { RoleId = x.Id, UserId = model.Id, TenantId = Findx.Data.Tenant.TenantId.Value });
                role_repo.Delete(x => x.UserId == model.Id);
                role_repo.Insert(list);
            }
            await base.EditAfterAsync(model, req, result);
        }
    }
}

