using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json;
using Findx.AspNetCore.Mvc;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.Expressions;
using Findx.Extensions;
using Findx.Module.EleAdmin.Dtos.User;
using Findx.Module.EleAdmin.Mvc.Filters;
using Findx.Module.EleAdmin.Shared.Enum;
using Findx.Module.EleAdmin.Shared.Models;
using Findx.Module.EleAdmin.Shared.ServiceDefaults;
using Findx.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Findx.Module.EleAdmin.Controller;

/// <summary>
///     用户服务
/// </summary>
[Area("system")]
[Route("api/[area]/user")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-用户"), Description("系统-用户")]
public class SysUserController : CrudControllerBase<SysUserInfo, UserSimplifyDto, UserAddDto, UserEditDto, UserQueryDto, Guid, Guid>
{
    private readonly IKeyGenerator<Guid> _keyGenerator;
    private readonly IOptions<JsonOptions> _jsonOptions;
    private readonly IWorkContext _workContext;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="keyGenerator"></param>
    /// <param name="jsonOptions"></param>
    /// <param name="workContext"></param>
    public SysUserController(IKeyGenerator<Guid> keyGenerator, IOptions<JsonOptions> jsonOptions, IWorkContext workContext)
    {
        _keyGenerator = keyGenerator;
        _jsonOptions = jsonOptions;
        _workContext = workContext;
    }

    /// <summary>
    ///     构建数据范围表达式
    /// </summary>
    /// <param name="defaultWhere"></param>
    /// <returns></returns>
    private Expression<Func<SysUserInfo, bool>> BuildDataScopeWhereExpression(Expression<Func<SysUserInfo, bool>> defaultWhere)
    {
        var user = _workContext.GetCurrentUser();
        var exp = PredicateBuilder.New<SysUserInfo>()
                                  .AndIf(_workContext.DataScope is DataScope.Custom, x => _workContext.OrgIds.Contains(x.OrgId.Value))
                                  .AndIf(_workContext.DataScope is DataScope.Subs, x => _workContext.OrgIds.Contains(x.OrgId.Value))
                                  .AndIf(_workContext.DataScope is DataScope.Department, x => x.OrgId == user.OrgId.Value)
                                  .AndIf(_workContext.DataScope is DataScope.Oneself, x => x.Id == user.UserId);
        return defaultWhere == null ? exp.Build() : exp?.And(defaultWhere)?.Build();
    }

    /// <summary>
    ///     分页查询
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [DataScopeLimiter, IpAddressLimiter]
    public override async Task<CommonResult<PageResult<List<UserSimplifyDto>>>> PageAsync(UserQueryDto request, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysUserInfo, Guid>();
        var roleRepo = GetRepository<SysUserRoleInfo, Guid>();
        
        var whereExpression = BuildDataScopeWhereExpression(CreateWhereExpression(request));
        var orderByExpression = CreateOrderExpression(request);

        var res = await repo.PagedAsync<UserSimplifyDto>(request.PageNo, request.PageSize, whereExpression, orderParameters: orderByExpression, cancellationToken: cancellationToken);
        var ids = res.Rows.Select(x => x.Id).Distinct();
        var roles = await roleRepo.SelectAsync(x => x.RoleInfo.Id == x.RoleId && ids.Contains(x.UserId), cancellationToken: cancellationToken);
        foreach (var item in res.Rows)
            item.Roles = roles.Where(x => x.UserId == item.Id && x.RoleInfo != null).Select(x => new UserRoleSimplifyDto { Id = x.RoleInfo.Id, Code = x.RoleInfo.Code, Name = x.RoleInfo.Name });
        
        return CommonResult.Success(res);
    }

    /// <summary>
    ///     修改状态
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPut("status")]
    [Description("修改状态")]
    public CommonResult Status([FromBody] UserPropertySaveDto req)
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
    public CommonResult Password([FromBody] UserPropertySaveDto req)
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
        var filterGroup = new FilterGroup
        {
            Logic = FilterOperate.And,
            Filters = [
                new FilterCondition { Field = field, Value = value, Operator = FilterOperate.Equal },
                new FilterCondition { Field = "Id", Value = id.ToString(), Operator = FilterOperate.NotEqual }
            ]
        };
        var whereExp = LambdaExpressionParser.ParseConditions<SysUserInfo>(filterGroup);
        var repo = GetRepository<SysUserInfo, Guid>();
        return repo.Exist(whereExp) ? CommonResult.Success() : CommonResult.Fail("404", "账号不存在");
    }

    /// <summary>
    ///     插入前校验
    /// </summary>
    /// <param name="model"></param>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    protected override async Task AddBeforeAsync(SysUserInfo model, UserAddDto req, CancellationToken cancellationToken = default)
    {
        if (!req.Password.IsNullOrWhiteSpace()) 
            model.Password = EncryptUtility.Md5By32(req.Password);
        model.RoleJson = JsonSerializer.Serialize(req.Roles.Select(x => new { x.Id, x.Name, x.Code }), options: _jsonOptions.Value.JsonSerializerOptions);
        await base.AddBeforeAsync(model, req, cancellationToken);
    }
    
    /// <summary>
    ///     详情查询后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="simplifyDto"></param>
    /// <returns></returns>
    protected override Task DetailAfterAsync(SysUserInfo model, UserSimplifyDto simplifyDto)
    {
        var roleRepo = GetRepository<SysUserRoleInfo>();
        var roles = roleRepo.Select(x => x.RoleInfo.Id == x.RoleId && x.UserId == model.Id);
        simplifyDto.Roles = roles.Where(x => x.RoleInfo != null).Select(x => new UserRoleSimplifyDto { Id = x.RoleInfo.Id, Code = x.RoleInfo.Code, Name = x.RoleInfo.Name });
        
        return Task.CompletedTask;
    }

    /// <summary>
    ///     插入中
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Transactional(EntityType = typeof(SysUserInfo))]
    public override Task<CommonResult> AddAsync(UserAddDto request, CancellationToken cancellationToken = default)
    {
        return base.AddAsync(request, cancellationToken);
    }

    /// <summary>
    ///     插入后
    /// </summary>
    /// <param name="model"></param>
    /// <param name="req"></param>
    /// <param name="result"></param>
    /// <param name="cancellationToken"></param>
    protected override async Task AddAfterAsync(SysUserInfo model, UserAddDto req, int result, CancellationToken cancellationToken = default)
    {
        if (result > 0)
        {
            var userRoleRepo = UnitOfWork.GetRepository<SysUserRoleInfo, Guid>();
            var list = req.Roles.Select(x => new SysUserRoleInfo { Id = _keyGenerator.Create(), RoleId = x.Id, UserId = model.Id });
            await userRoleRepo.DeleteAsync(x => x.UserId == model.Id, cancellationToken);
            await userRoleRepo.InsertAsync(list, cancellationToken);
        }
        await base.AddAfterAsync(model, req, result, cancellationToken);
    }

    /// <summary>
    ///     编辑前
    /// </summary>
    /// <param name="model"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override Task EditBeforeAsync(SysUserInfo model, UserEditDto request, CancellationToken cancellationToken = default)
    {
        model.RoleJson = JsonSerializer.Serialize(request.Roles.Select(x => new { x.Id, x.Name, x.Code }), options: _jsonOptions.Value.JsonSerializerOptions);
        return base.EditBeforeAsync(model, request, cancellationToken);
    }

    /// <summary>
    ///     编辑执行
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Transactional(EntityType = typeof(SysUserInfo))]
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
    /// <param name="cancellationToken"></param>
    protected override async Task EditAfterAsync(SysUserInfo model, UserEditDto req, int result, CancellationToken cancellationToken = default)
    {
        if (result > 0)
        {
            var userRoleRepo = UnitOfWork.GetRepository<SysUserRoleInfo, Guid>();
            var list = req.Roles.Select(x => new SysUserRoleInfo { Id = _keyGenerator.Create(), RoleId = x.Id, UserId = model.Id });
            await userRoleRepo.DeleteAsync(x => x.UserId == model.Id, cancellationToken);
            await userRoleRepo.InsertAsync(list, cancellationToken);
        }
        await base.EditAfterAsync(model, req, result, cancellationToken);
    }
}