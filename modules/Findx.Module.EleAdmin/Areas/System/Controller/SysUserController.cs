using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using Findx.Linq;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Principal;
using Findx.Module.EleAdmin.DTO;
using Findx.Module.EleAdmin.Models;

namespace Findx.Module.EleAdmin.Areas.System.Controller
{
    /// <summary>
    /// 用户服务
    /// </summary>
    [Area("system")]
    [Route("api/[area]/user")]
    [Authorize]
    public class SysUserController : CrudControllerBase<SysUserInfo, UserDto, SetUserRequest, QueryUserRequest, Guid, Guid>
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("page")]
        public override async Task<CommonResult> PageAsync([FromQuery] QueryUserRequest request)
        {
            Check.NotNull(request, nameof(request));

            var repo = GetRepository<SysUserInfo>();
            var roleRepo = GetRepository<SysUserRoleInfo>();

            Check.NotNull(repo, nameof(repo));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var result = await repo.PagedAsync<UserDto>(request.PageNo, request.PageSize, whereExpression: whereExpression?.ToExpression(), orderParameters: orderByExpression.ToArray());
            var ids = result.Rows.Select(x => x.Id).Distinct();
            var roles = roleRepo.Select(x => x.RoleInfo.Id == x.RoleId && ids.Contains(x.UserId));
            foreach (var item in result.Rows)
            {
                item.Roles = roles.Where(x => x.UserId == item.Id && x.RoleInfo != null).Select(x => x.RoleInfo);
            }
            return CommonResult.Success(result);
        }

        /// <summary>
        /// 修改状态
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("status")]
        public CommonResult Status([FromBody] SetUserPropertyRequest req)
        {
            var repo = GetRepository<SysUserInfo>();
            repo.UpdateColumns(x => new SysUserInfo { Status = req.Status }, x => x.Id == req.Id);
            return CommonResult.Success();
        }
        
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("password")]
        public CommonResult Password([FromBody] SetUserPropertyRequest req)
        {
            var repo = GetRepository<SysUserInfo>();
            var pwd = Findx.Utils.Encrypt.Md5By32(req.Password);
            repo.UpdateColumns(x => new SysUserInfo { Password = pwd }, x => x.Id == req.Id);
            return CommonResult.Success();
        }

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("existence")]
        public CommonResult Existence([Required] string field, [Required] string value, Guid id)
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
            var roleRepo = GetRepository<SysUserRoleInfo>() ?? throw new ArgumentNullException("GetRepository<SysUserRoleInfo>()");
            var roles = roleRepo.Select(x => x.RoleInfo.Id == x.RoleId && x.UserId == model.Id);
            dto.Roles = roles.Where(x => x.RoleInfo != null).Select(x => x.RoleInfo);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 插入前
        /// </summary>
        /// <param name="model"></param>
        /// <param name="req"></param>
        protected override async Task AddBeforeAsync(SysUserInfo model, SetUserRequest req)
        {
            if (!req.Password.IsNullOrWhiteSpace())
            {
                model.Password = Findx.Utils.Encrypt.Md5By32(req.Password);
            }

            await base.AddBeforeAsync(model, req);
        }

        /// <summary>
        /// 插入后
        /// </summary>
        /// <param name="model"></param>
        /// <param name="req"></param>
        /// <param name="result"></param>
        protected override async Task AddAfterAsync(SysUserInfo model, SetUserRequest req, int result)
        {
            if (result > 0)
            {
                var repo = HttpContext.RequestServices.GetRequiredService<IRepository<SysUserInfo>>();
                var roleRepo = HttpContext.RequestServices.GetRequiredService<IRepository<SysUserRoleInfo>>();
                var principal = HttpContext.RequestServices.GetRequiredService<IPrincipal>();

                var user = repo.First(x => x.UserName == req.UserName);
                if (user != null)
                {
                    var list = req.Roles.Select(x => new SysUserRoleInfo { RoleId = x.Id, UserId = user.Id, TenantId = Findx.Data.Tenant.TenantId.Value });
                    roleRepo.Insert(list);
                }
            }
            await base.AddAfterAsync(model, req, result);
        }

        /// <summary>
        /// 编辑前
        /// </summary>
        /// <param name="model"></param>
        /// <param name="req"></param>
        protected override async Task EditBeforeAsync(SysUserInfo model, SetUserRequest req)
        {
            model.Password = null;
            await base.EditBeforeAsync(model, req);
        }

        /// <summary>
        /// 编辑后
        /// </summary>
        /// <param name="model"></param>
        /// <param name="req"></param>
        /// <param name="result"></param>
        protected override async Task EditAfterAsync(SysUserInfo model, SetUserRequest req, int result)
        {
            if (result > 0)
            {
                var roleRepo = HttpContext.RequestServices.GetService<IRepository<SysUserRoleInfo>>();
                var principal = HttpContext.RequestServices.GetService<IPrincipal>();

                var list = req.Roles.Select(x => new SysUserRoleInfo { RoleId = x.Id, UserId = model.Id, TenantId = Findx.Data.Tenant.TenantId.Value });
                roleRepo.Delete(x => x.UserId == model.Id);
                roleRepo.Insert(list);
            }
            await base.EditAfterAsync(model, req, result);
        }
    }
}

