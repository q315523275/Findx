using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;
using Findx.Mapping;
using Findx.Module.EleAdminPlus.Dtos.User;
using Findx.Module.EleAdminPlus.Models;
using Findx.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.Controller;

/// <summary>
///     用户服务
/// </summary>
[Area("system")]
[Route("api/[area]/user")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-用户"), Description("系统-用户")]
public class SysUserController : CrudControllerBase<SysUserInfo, UserDto, UserCreateDto, UserEditDto, UserPageQueryDto, long, long>
{
    private readonly IKeyGenerator<long> _keyGenerator;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="keyGenerator"></param>
    public SysUserController(IKeyGenerator<long> keyGenerator)
    {
        _keyGenerator = keyGenerator;
    }

    /// <summary>
    ///     分页查询
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommonResult<PageResult<List<UserDto>>>> PageAsync(UserPageQueryDto request, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysUserInfo, long>();
        var roleRepo = GetRepository<SysUserRoleInfo, long>();

        var whereExpression = CreatePageWhereExpression(request);
        var orderByExpression = CreatePageOrderExpression(request);

        var res = await repo.PagedAsync<UserDto>(request.PageNo, request.PageSize, whereExpression, orderParameters: orderByExpression, cancellationToken: cancellationToken);
        var ids = res.Rows.Select(x => x.Id).Distinct();
        var roles = await roleRepo.SelectAsync(x => x.RoleInfo.Id == x.RoleId && ids.Contains(x.UserId), selectExpression: x => new { RoleId = x.RoleInfo.Id, RoleCode = x.RoleInfo.Code, RoleName = x.RoleInfo.Name, x.UserId }, cancellationToken: cancellationToken);
        var roleDict = roles.GroupBy(x => x.UserId).ToDictionary(x => x.Key, x => x);
        foreach (var item in res.Rows)
            item.Roles = roleDict.TryGetValue(item.Id, out var userRoles) ? userRoles.Select(x => new UserRoleSimplifyDto { Id = x.RoleId, Code = x.RoleCode, Name = x.RoleName }) : Array.Empty<UserRoleSimplifyDto>();
        
        return CommonResult.Success(res);
    }

    /// <summary>
    ///     用户列表
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommonResult<List<UserDto>>> ListAsync(UserPageQueryDto request, CancellationToken cancellationToken = new())
    {
        var repo = GetRepository<SysUserInfo, long>();
        var roleRepo = GetRepository<SysUserRoleInfo, long>();

        var whereExpression = CreatePageWhereExpression(request);
        var orderByExpression = CreatePageOrderExpression(request);

        var res = await repo.TopAsync<UserDto>(request.PageSize, whereExpression, orderParameters: orderByExpression, cancellationToken: cancellationToken);
        var ids = res.Select(x => x.Id).Distinct();
        var roles = await roleRepo.SelectAsync(x => x.RoleInfo.Id == x.RoleId && ids.Contains(x.UserId), selectExpression: x => new { RoleId = x.RoleInfo.Id, RoleCode = x.RoleInfo.Code, RoleName = x.RoleInfo.Name, x.UserId }, cancellationToken: cancellationToken);
        var roleDict = roles.GroupBy(x => x.UserId).ToDictionary(x => x.Key, x => x);
        foreach (var item in res)
            item.Roles = roleDict.TryGetValue(item.Id, out var userRoles) ? userRoles.Select(x => new UserRoleSimplifyDto { Id = x.RoleId, Code = x.RoleCode, Name = x.RoleName }) : Array.Empty<UserRoleSimplifyDto>();
        
        return CommonResult.Success(res);
    }

    /// <summary>
    ///     修改状态
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPut("status"), Description("修改状态")]
    public CommonResult Status([FromBody] UserPropertySaveDto req)
    {
        var repo = GetRepository<SysUserInfo, long>();
        repo.UpdateColumns(x => new SysUserInfo { Status = req.Status }, x => x.Id == req.Id);
        return CommonResult.Success();
    }

    /// <summary>
    ///     修改密码
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPut("password"), Description("修改密码")]
    public CommonResult Password([FromBody] UserPropertySaveDto req)
    {
        var repo = GetRepository<SysUserInfo, long>();
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
    [HttpGet("existence"), Description("检查是否存在")]
    public CommonResult Existence([Required] string field, [Required] string value, long id)
    {
        var filterGroup = new FilterGroup
        {
            Logic = FilterOperate.And,
            Filters = [
                new FilterCondition { Field = field.ToPascalCase(), Value = value, Operator = FilterOperate.Equal },
                new FilterCondition { Field = "Id", Value = id.ToString(), Operator = FilterOperate.NotEqual }
            ]
        };
        var whereExp = LambdaExpressionParser.ParseConditions<SysUserInfo>(filterGroup);
        var repo = GetRepository<SysUserInfo, long>();
        return repo.Exist(whereExp) ? CommonResult.Success() : CommonResult.Fail("404", "账号不存在");
    }

    /// <summary>
    ///     详情查询后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    protected override async Task DetailAfterAsync(SysUserInfo model, UserDto dto)
    {
        var roleRepo = GetRepository<SysUserRoleInfo, long>();
        var roles = await roleRepo.SelectAsync(x => x.RoleInfo.Id == x.RoleId && x.UserId == model.Id, x => x.RoleInfo);
        dto.Roles = roles.MapTo<List<UserRoleSimplifyDto>>();
    }

    /// <summary>
    ///     插入前校验
    /// </summary>
    /// <param name="model"></param>
    /// <param name="req"></param>
    protected override async Task AddBeforeAsync(SysUserInfo model, UserCreateDto req)
    {
        if (!req.Password.IsNullOrWhiteSpace())
            model.Password = EncryptUtility.Md5By32(req.Password);

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
            var repo = GetRepository<SysUserInfo, long>();
            var roleRepo = GetRepository<SysUserRoleInfo, long>();

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
    ///     编辑
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Transactional(DataSource = "system")]
    public override Task<CommonResult> EditAsync(UserEditDto request, CancellationToken cancellationToken = default)
    {
        return base.EditAsync(request, cancellationToken);
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
            var userRoleRepo = GetRepository<SysUserRoleInfo, long>();
            var list = req.Roles.Select(x => new SysUserRoleInfo { Id = _keyGenerator.Create(), RoleId = x.Id, UserId = model.Id });
            await userRoleRepo.DeleteAsync(x => x.UserId == model.Id);
            await userRoleRepo.InsertAsync(list);
        }
    }
}