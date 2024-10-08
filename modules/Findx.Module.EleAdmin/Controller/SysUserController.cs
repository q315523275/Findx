﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Expressions;
using Findx.Extensions;
using Findx.Module.EleAdmin.Dtos;
using Findx.Module.EleAdmin.Models;
using Findx.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Controller;

/// <summary>
///     用户服务
/// </summary>
[Area("system")]
[Route("api/[area]/user")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-用户"), Description("系统-用户")]
public class SysUserController : CrudControllerBase<SysUserInfo, UserDto, UserCreateDto, UserEditDto, QueryUserRequest, Guid, Guid>
{
    private readonly IKeyGenerator<Guid> _keyGenerator;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="keyGenerator"></param>
    public SysUserController(IKeyGenerator<Guid> keyGenerator)
    {
        _keyGenerator = keyGenerator;
    }

    /// <summary>
    ///     构建查询条件
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    protected override Expression<Func<SysUserInfo, bool>> CreateWhereExpression(QueryUserRequest req)
    {
        var whereExp = PredicateBuilder.New<SysUserInfo>()
                                       .AndIf(!req.UserName.IsNullOrWhiteSpace(), x => x.UserName.Contains(req.UserName))
                                       .AndIf(!req.Nickname.IsNullOrWhiteSpace(), x => x.Nickname.Contains(req.Nickname))
                                       .AndIf(req.Sex.HasValue, x => x.Sex == req.Sex)
                                       .AndIf(req.OrgId.HasValue, x => x.OrgId == req.OrgId)
                                       .Build();
        return whereExp;
    }

    /// <summary>
    ///     分页查询
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommonResult<PageResult<List<UserDto>>>> PageAsync(QueryUserRequest request, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysUserInfo>();
        var roleRepo = GetRepository<SysUserRoleInfo>();

        var whereExpression = CreateWhereExpression(request);
        var orderByExpression = CreateOrderExpression(request);

        var res = await repo.PagedAsync<UserDto>(request.PageNo, request.PageSize, whereExpression, orderParameters: orderByExpression, cancellationToken: cancellationToken);
        var ids = res.Rows.Select(x => x.Id).Distinct();
        var roles = await roleRepo.SelectAsync(x => x.RoleInfo.Id == x.RoleId && ids.Contains(x.UserId), cancellationToken: cancellationToken);
        foreach (var item in res.Rows)
            item.Roles = roles.Where(x => x.UserId == item.Id && x.RoleInfo != null).Select(x => x.RoleInfo);

        return CommonResult.Success(res);
    }

    /// <summary>
    ///     修改状态
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
    ///     修改密码
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPut("password")]
    [Description("修改密码")]
    public CommonResult Password([FromBody] SetUserPropertyRequest req)
    {
        var repo = GetRepository<SysUserInfo>();
        var pwd = EncryptUtility.Md5By32(req.Password);
        repo.UpdateColumns(x => new SysUserInfo { Password = pwd }, x => x.Id == req.Id);
        return CommonResult.Success();
    }

    /// <summary>
    ///     判断是否存在
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("existence")]
    [Description("检查是否存在")]
    public CommonResult Existence([Required] string field, [Required] string value, Guid id)
    {
        var whereExp = PredicateBuilder.New<SysUserInfo>()
                                       .AndIf(field == "userName", x => x.UserName == value)
                                       .And(x => x.Id != id)
                                       .Build();
        var repo = GetRepository<SysUserInfo>();
        return repo.Exist(whereExp) ? CommonResult.Success() : CommonResult.Fail("404", "账号不存在");
    }

    /// <summary>
    ///     详情查询后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    protected override Task DetailAfterAsync(SysUserInfo model, UserDto dto)
    {
        var roleRepo = GetRepository<SysUserRoleInfo>();
        var roles = roleRepo.Select(x => x.RoleInfo.Id == x.RoleId && x.UserId == model.Id);
        dto.Roles = roles.Where(x => x.RoleInfo != null).Select(x => x.RoleInfo);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     插入前校验
    /// </summary>
    /// <param name="model"></param>
    /// <param name="req"></param>
    protected override async Task AddBeforeAsync(SysUserInfo model, UserCreateDto req)
    {
        if (!req.Password.IsNullOrWhiteSpace()) model.Password = EncryptUtility.Md5By32(req.Password);

        await base.AddBeforeAsync(model, req);
    }

    /// <summary>
    ///     插入后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="req"></param>
    /// <param name="result"></param>
    protected override async Task AddAfterAsync(SysUserInfo model, UserCreateDto req, int result)
    {
        if (result > 0)
        {
            var repo = GetRequiredService<IRepository<SysUserInfo>>();
            var roleRepo = GetRequiredService<IRepository<SysUserRoleInfo>>();

            var user = await repo.FirstAsync(x => x.UserName == req.UserName);
            if (user != null)
            {
                var list = req.Roles.Select(x => new SysUserRoleInfo { Id = _keyGenerator.Create(), RoleId = x.Id, UserId = user.Id });
                await roleRepo.InsertAsync(list);
            }
        }

        await base.AddAfterAsync(model, req, result);
    }

    /// <summary>
    ///     编辑后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="req"></param>
    /// <param name="result"></param>
    protected override async Task EditAfterAsync(SysUserInfo model, UserEditDto req, int result)
    {
        if (result > 0)
        {
            var roleRepo = GetRequiredService<IRepository<SysUserRoleInfo>>();

            var list = req.Roles.Select(x => new SysUserRoleInfo { Id = _keyGenerator.Create(), RoleId = x.Id, UserId = model.Id });
            await roleRepo.DeleteAsync(x => x.UserId == model.Id);
            await roleRepo.InsertAsync(list);
        }
    }
}