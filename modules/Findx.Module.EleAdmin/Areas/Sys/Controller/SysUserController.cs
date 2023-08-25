using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Exceptions;
using Findx.Extensions;
using Findx.Linq;
using Findx.Module.EleAdmin.Dtos;
using Findx.Module.EleAdmin.Models;
using Findx.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Areas.Sys.Controller;

/// <summary>
///     用户服务
/// </summary>
[Area("system")]
[Route("api/[area]/user")]
[Authorize]
[Description("系统-用户")]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-用户")]
public class SysUserController : CrudControllerBase<SysUserInfo, UserDto, SetUserRequest, QueryUserRequest, Guid, Guid>
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
    protected override Expressionable<SysUserInfo> CreatePageWhereExpression(QueryUserRequest req)
    {
        var whereExp = ExpressionBuilder.Create<SysUserInfo>()
            .AndIF(!req.UserName.IsNullOrWhiteSpace(), x => x.UserName.Contains(req.UserName))
            .AndIF(!req.Nickname.IsNullOrWhiteSpace(), x => x.Nickname.Contains(req.Nickname))
            .AndIF(req.Sex > 0, x => x.Sex == req.Sex)
            .AndIF(req.OrgId.HasValue, x => x.OrgId == req.OrgId);
        return whereExp;
    }

    /// <summary>
    ///     构建排序
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override List<OrderByParameter<SysUserInfo>> CreatePageOrderExpression(QueryUserRequest request)
    {
        var orderExp = ExpressionBuilder.CreateOrder<SysUserInfo>();
        switch (request.SortField)
        {
            case "userName":
                orderExp.Order(it => it.UserName, request.SortDirection);
                break;
            case "nickname":
                orderExp.Order(it => it.Nickname, request.SortDirection);
                break;
            case "sex":
                orderExp.Order(it => it.Sex, request.SortDirection);
                break;
            case "phone":
                orderExp.Order(it => it.Phone, request.SortDirection);
                break;
            case "createdTime":
                orderExp.Order(it => it.CreatedTime, request.SortDirection);
                break;
            case "status":
                orderExp.Order(it => it.Status, request.SortDirection);
                break;
        }

        orderExp.OrderByDescending(it => it.Id);
        return orderExp.ToSort();
    }

    /// <summary>
    ///     分页查询
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async Task<CommonResult<PageResult<List<UserDto>>>> PageAsync(QueryUserRequest request)
    {
        var repo = GetRepository<SysUserInfo>();
        var roleRepo = GetRepository<SysUserRoleInfo>();

        var whereExpression = CreatePageWhereExpression(request);
        var orderByExpression = CreatePageOrderExpression(request);

        var res = await repo.PagedAsync<UserDto>(request.PageNo, request.PageSize, whereExpression?.ToExpression(),
            orderParameters: orderByExpression);
        var ids = res.Rows.Select(x => x.Id).Distinct();
        var roles = await roleRepo.SelectAsync(x => x.RoleInfo.Id == x.RoleId && ids.Contains(x.UserId));
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
        var pwd = Encrypt.Md5By32(req.Password);
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
        var whereExp = ExpressionBuilder.Create<SysUserInfo>()
            .AndIF(field == "userName", x => x.UserName == value)
            .And(x => x.Id != id)
            .ToExpression();
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
    protected override async Task AddBeforeAsync(SysUserInfo model, SetUserRequest req)
    {
        if (!req.Password.IsNullOrWhiteSpace()) model.Password = Encrypt.Md5By32(req.Password);

        await base.AddBeforeAsync(model, req);
    }

    /// <summary>
    ///     插入后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="req"></param>
    /// <param name="result"></param>
    protected override async Task AddAfterAsync(SysUserInfo model, SetUserRequest req, int result)
    {
        if (result > 0)
        {
            var repo = GetRequiredService<IRepository<SysUserInfo>>();
            var roleRepo = GetRequiredService<IRepository<SysUserRoleInfo>>();

            var user = await repo.FirstAsync(x => x.UserName == req.UserName);
            if (user != null)
            {
                var list = req.Roles.Select(x => new SysUserRoleInfo { Id = _keyGenerator.Create(), RoleId = x.Id, UserId = user.Id, TenantId = TenantManager.Current });
                await roleRepo.InsertAsync(list);
            }
        }

        await base.AddAfterAsync(model, req, result);
    }

    /// <summary>
    ///     编辑前
    /// </summary>
    /// <param name="model"></param>
    /// <param name="req"></param>
    protected override async Task EditBeforeAsync(SysUserInfo model, SetUserRequest req)
    {
        model.Password = null;
        await base.EditBeforeAsync(model, req);
    }

    /// <summary>
    ///     编辑后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="req"></param>
    /// <param name="result"></param>
    protected override async Task EditAfterAsync(SysUserInfo model, SetUserRequest req, int result)
    {
        if (result > 0)
        {
            var roleRepo = GetRequiredService<IRepository<SysUserRoleInfo>>();

            var list = req.Roles.Select(x => new SysUserRoleInfo { Id = _keyGenerator.Create(), RoleId = x.Id, UserId = model.Id, TenantId = TenantManager.Current });
            await roleRepo.DeleteAsync(x => x.UserId == model.Id);
            await roleRepo.InsertAsync(list);
        }
    }
}