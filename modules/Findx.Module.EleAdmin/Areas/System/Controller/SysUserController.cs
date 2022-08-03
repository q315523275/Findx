﻿using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using Findx.Linq;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Findx.Module.EleAdmin.DTO;
using Findx.Module.EleAdmin.Models;
using Findx.Security;
using System.ComponentModel;

namespace Findx.Module.EleAdmin.Areas.System.Controller
{
    /// <summary>
    /// 用户服务
    /// </summary>
    [Area("system")]
    [Route("api/[area]/user")]
    [Authorize]
    [Description("系统-用户")]
    public class SysUserController : CrudControllerBase<SysUserInfo, UserDto, SetUserRequest, QueryUserRequest, Guid, Guid>
    {
        private readonly ICurrentUser _currentUser;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="currentUser"></param>
        public SysUserController(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        /// <summary>
        /// 构建查询条件
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        protected override Expressionable<SysUserInfo> CreatePageWhereExpression(QueryUserRequest req)
        {
            var whereExp = ExpressionBuilder.Create<SysUserInfo>()
                                            .AndIF(!req.UserName.IsNullOrWhiteSpace(), x => x.UserName.Contains(req.UserName))
                                            .AndIF(!req.Nickname.IsNullOrWhiteSpace(), x => x.Nickname.Contains(req.Nickname))
                                            .AndIF(req.Sex > 0, x => x.Sex == req.Sex);
            return whereExp;
        }

        /// <summary>
        /// 构建排序
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override List<OrderByParameter<SysUserInfo>> CreatePageOrderExpression(QueryUserRequest request)
        {
            var multiOrderBy = new List<OrderByParameter<SysUserInfo>>();

            switch (request.SortField)
            {
                case "userName":
                    multiOrderBy.Add(new OrderByParameter<SysUserInfo> { Expression = it => it.UserName, SortDirection = request.SortDirection });
                    break;
                case "nickname":
                    multiOrderBy.Add(new OrderByParameter<SysUserInfo> { Expression = it => it.Nickname, SortDirection = request.SortDirection });
                    break;
                case "sex":
                    multiOrderBy.Add(new OrderByParameter<SysUserInfo> { Expression = it => it.Sex, SortDirection = request.SortDirection });
                    break;
                case "phone":
                    multiOrderBy.Add(new OrderByParameter<SysUserInfo> { Expression = it => it.Phone, SortDirection = request.SortDirection });
                    break;
                case "createdTime":
                    multiOrderBy.Add(new OrderByParameter<SysUserInfo> { Expression = it => it.CreatedTime, SortDirection = request.SortDirection });
                    break;
                case "status":
                    multiOrderBy.Add(new OrderByParameter<SysUserInfo> { Expression = it => it.Status, SortDirection = request.SortDirection });
                    break;
            }
            multiOrderBy.Add(new OrderByParameter<SysUserInfo> { Expression = it => it.Id, SortDirection = ListSortDirection.Descending });
            return multiOrderBy;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("page")]
        public override async Task<CommonResult> PageAsync([FromQuery] QueryUserRequest request)
        {
            var repo = GetRepository<SysUserInfo>();
            var roleRepo = GetRepository<SysUserRoleInfo>();

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
        [Description("修改状态")]
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
        [Description("修改密码")]
        public CommonResult Password([FromBody] SetUserPropertyRequest req)
        {
            var userId = _currentUser.UserId.To<Guid>();
            var repo = GetRepository<SysUserInfo>();
            var pwd = Findx.Utils.Encrypt.Md5By32(req.Password);
            repo.UpdateColumns(x => new SysUserInfo { Password = pwd }, x => x.Id == userId);
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
        [Description("检查是否存在")]
        public CommonResult Existence([Required] string field, [Required] string value, Guid id)
        {
            var whereExp = ExpressionBuilder.Create<SysUserInfo>()
                                            .AndIF(field == "userName", x => x.UserName == value)
                                            .And(x => x.Id != id)
                                            .ToExpression();
            var repo = GetRepository<SysUserInfo>();
            return repo.Exist(whereExp) ? CommonResult.Success() : CommonResult.Fail("404", "账号不存在");
        }

        /// <summary>
        /// 详情查询后
        /// </summary>
        /// <param name="model"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected override Task DetailAfterAsync(SysUserInfo model, UserDto dto)
        {
            var roleRepo = GetRepository<SysUserRoleInfo>();
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

                var user = await repo.FirstAsync(x => x.UserName == req.UserName);
                if (user != null)
                {
                    var list = req.Roles.Select(x => new SysUserRoleInfo { RoleId = x.Id, UserId = user.Id, TenantId = Tenant.TenantId.Value });
                    await roleRepo.InsertAsync(list);
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
                var roleRepo = HttpContext.RequestServices.GetRequiredService<IRepository<SysUserRoleInfo>>();

                var list = req.Roles.Select(x => new SysUserRoleInfo { RoleId = x.Id, UserId = model.Id, TenantId = Findx.Data.Tenant.TenantId.Value });
                await roleRepo.DeleteAsync(x => x.UserId == model.Id);
                await roleRepo.InsertAsync(list);
            }
        }
    }
}
