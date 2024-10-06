using System.ComponentModel;
using System.Security.Principal;
using System.Text.Json;
using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc;
using Findx.Caching;
using Findx.Data;
using Findx.Extensions;
using Findx.Mapping;
using Findx.Module.EleAdminPlus.WebAppApi.Dtos.Auth;
using Findx.Module.EleAdminPlus.Shared.Enums;
using Findx.Module.EleAdminPlus.Shared.Models;
using Findx.Module.EleAdminPlus.WebAppApi.Dtos.Role;
using Findx.Security;
using Findx.Security.Authentication.Jwt;
using Findx.Setting;
using Findx.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Findx.Module.EleAdminPlus.WebAppApi.Controller;

/// <summary>
///     系统-账户
/// </summary>
[Area("system")]
[Route("api/[area]/auth")]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-账户"), Description("系统-账户")]
public class AuthController : AreaApiControllerBase
{
    private readonly ICacheFactory _cacheFactory;
    private readonly ICurrentUser _currentUser;
    private readonly IKeyGenerator<long> _keyGenerator;

    private readonly bool _enabledCaptcha;
    private readonly bool _useAbpJwt;
    private readonly IOptions<JwtOptions> _options;
    private readonly IRepository<SysLoginRecordInfo, long> _loginRecordRepo;
    private readonly IRepository<SysUserInfo, long> _userRepo;
    private readonly IJwtTokenBuilder _tokenBuilder;
    private readonly IOptions<JsonOptions> _jsonOptions;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="tokenBuilder"></param>
    /// <param name="options"></param>
    /// <param name="currentUser"></param>
    /// <param name="cacheFactory"></param>
    /// <param name="userRepo"></param>
    /// <param name="loginRecordRepo"></param>
    /// <param name="settingProvider"></param>
    /// <param name="keyGenerator"></param>
    /// <param name="jsonOptions"></param>
    public AuthController(IJwtTokenBuilder tokenBuilder, IOptions<JwtOptions> options, ICurrentUser currentUser, ICacheFactory cacheFactory, IRepository<SysUserInfo, long> userRepo, IRepository<SysLoginRecordInfo, long> loginRecordRepo, ISettingProvider settingProvider, IKeyGenerator<long> keyGenerator, IOptions<JsonOptions> jsonOptions)
    {
        _tokenBuilder = tokenBuilder;
        _options = options;
        _currentUser = currentUser;
        _cacheFactory = cacheFactory;
        _userRepo = userRepo;
        _loginRecordRepo = loginRecordRepo;
        _keyGenerator = keyGenerator;
        _jsonOptions = jsonOptions;
        
        _enabledCaptcha = settingProvider.GetValue<bool>("Modules:EleAdminPlus:EnabledCaptcha");
        _useAbpJwt = settingProvider.GetValue<bool>("Modules:EleAdminPlus:UseAbpJwt");
    }

    /// <summary>
    ///     账号密码登录
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/login"), Description("登录")]
    public virtual async Task<CommonResult> LoginAsync([FromBody] LoginDto req, CancellationToken cancellationToken = default)
    {
        var cache = _cacheFactory.Create(CacheType.DefaultMemory);

        // 是否开启验证码
        var cacheKey = $"verifyCode:{req.Uuid}";
        var cacheValue = await cache.GetAsync<string>(cacheKey, cancellationToken);
        if (_enabledCaptcha && !string.Equals(cacheValue.SafeString(), req.Code, StringComparison.OrdinalIgnoreCase))
            return CommonResult.Fail("50500", "验证码错误");

        // 密码错误次数
        var errorCacheKey = $"error:{req.UserName}";
        var errorCount = await cache.GetAsync<int>(errorCacheKey, cancellationToken);
        if (errorCount >= 5)
            return CommonResult.Fail("50509", "密码错误次数超出限制");

        var accountInfo = await _userRepo.FirstAsync(it => it.UserName == req.UserName && it.TenantId == req.TenantId, cancellationToken);
        // 验证帐号是否存在
        if (accountInfo == null)
            return CommonResult.Fail("D1000", "账户不存在");

        // 系统参数
        var userAgent = HttpContext.Request.GetUserAgent();
        var loginLog = new SysLoginRecordInfo
        {
            Browser = userAgent.Browser.Name,
            Comments = "账户密码错误",
            CreatedTime = DateTime.Now,
            Device = userAgent.OS.Name,
            Id = _keyGenerator.Create(),
            Ip = HttpContext.GetClientIp(),
            LoginType = 1,
            Os = userAgent.Platform.Name,
            TenantId = req.TenantId,
            UserName = req.UserName,
            Nickname = accountInfo.Nickname
        };

        CommonResult fail = null;
        // 验证帐号密码是否正确
        if (!accountInfo.Password.Equals(EncryptUtility.Md5By32(req.Password), StringComparison.OrdinalIgnoreCase))
        {
            // 增加错误次数
            errorCount++;
            await cache.AddAsync(errorCacheKey, errorCount, TimeSpan.FromMinutes(15), cancellationToken);
            loginLog.Comments = $"账号密码错误,剩余重试次数{5 - errorCount}次";
            fail = CommonResult.Fail("D1000", loginLog.Comments);
        }

        // 验证账号是否被冻结
        if (accountInfo.Status != CommonStatus.Enable.To<int>())
        {
            loginLog.Comments = "账号已冻结";
            fail = CommonResult.Fail("D1017", loginLog.Comments);
        }

        // 清空验证码
        if (_enabledCaptcha)
            await cache.RemoveAsync(cacheKey, cancellationToken);

        // 登录失败
        if (fail != null)
        {
            loginLog.LoginType = 1;
            await _loginRecordRepo.InsertAsync(loginLog, cancellationToken);
            return fail;
        }

        // 登录成功
        loginLog.LoginType = 0;
        loginLog.Comments = "登录成功";
        await _loginRecordRepo.InsertAsync(loginLog, cancellationToken);
        
        // 清除错误次数
        await cache.RemoveAsync(errorCacheKey, cancellationToken);

        // token票据
        var payload = new Dictionary<string, string>
        {
            { ClaimTypes.UserId, accountInfo.Id.SafeString() },
            { ClaimTypes.UserIdTypeName, typeof(long).FullName },
            { ClaimTypes.UserName, accountInfo.UserName.SafeString() },
            { ClaimTypes.Nickname, accountInfo.Nickname.SafeString() },
            { ClaimTypes.TenantId, req.TenantId.ToString() },
            { ClaimTypes.OrgId, accountInfo.OrgId.SafeString() },
            { ClaimTypes.OrgName, accountInfo.OrgName.SafeString() }
        };
        // 兼容AbpJwt
        if (_useAbpJwt)
        {
            payload[System.Security.Claims.ClaimTypes.NameIdentifier] = accountInfo.Id.SafeString();
            payload[System.Security.Claims.ClaimTypes.Name] = accountInfo.UserName.SafeString();
            payload[System.Security.Claims.ClaimTypes.GivenName] = accountInfo.Nickname.SafeString();
        }
        // 角色id及编号
        var roles = JsonSerializer.Deserialize<List<RoleDto>>(accountInfo.RoleJson ?? "[]", options: _jsonOptions.Value.JsonSerializerOptions);
        payload[ClaimTypes.RoleIds] = roles.Select(x => x.Id).Distinct().JoinAsString(",");
        payload[ClaimTypes.Role] = roles.Select(x => x.Code).Distinct().JoinAsString(",");
        
        var token = await _tokenBuilder.CreateAsync(payload, _options.Value);
        return CommonResult.Success(new { access_token = "Bearer " + token.AccessToken });
    }

    /// <summary>
    ///     查看账户信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("/api/auth/user"), Authorize, Description("查看账户信息")]
    public virtual async Task<CommonResult> UserAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId.To<long>();
        var userInfo = await _userRepo.FirstAsync(x => x.Id == userId, cancellationToken);
        if (userInfo == null)
            return CommonResult.Fail("D1000", "账户不存在");

        var roleRepo = GetRepository<SysUserRoleInfo, long>();
        var menuRepo = GetRepository<SysRoleMenuInfo, long>();

        var roles = roleRepo.Select(x => x.UserId == userId && x.RoleId == x.RoleInfo.Id, x => new UserAuthRoleDto { Id = x.RoleId, RoleCode = x.RoleInfo.Code, RoleName = x.RoleInfo.Name })
                            .DistinctBy(x => x.Id);
        // ReSharper disable once PossibleMultipleEnumeration
        var roleIds = roles.Select(x => x.Id);
        // ReSharper disable once PossibleMultipleEnumeration
        var menus = roleIds.Any() ?
                // ReSharper disable once PossibleMultipleEnumeration
                menuRepo.Select(x => roleIds.Contains(x.RoleId) && x.MenuId == x.MenuInfo.Id, x => new UserAuthMenuDto { MenuId = x.MenuId }) : [];
        
        var result = userInfo.MapTo<UserAuthDto>();
        // ReSharper disable once PossibleMultipleEnumeration
        result.Roles = roles;
        result.Authorities = menus.DistinctBy(x => x.MenuId).OrderBy(x => x.Sort);

        return CommonResult.Success(result);
    }

    /// <summary>
    ///     修改密码
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("/api/auth/password"), Authorize, Description("修改账户密码")]
    public virtual async Task<CommonResult> PasswordAsync([FromBody] UpdatePasswordDto req, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId.To<long>();
        var userInfo = await _userRepo.FirstAsync(x => x.Id == userId, cancellationToken);
        if (userInfo == null)
            return CommonResult.Fail("D1000", "账户不存在");

        if (!userInfo.Password.Equals(EncryptUtility.Md5By32(req.OldPassword), StringComparison.OrdinalIgnoreCase))
            return CommonResult.Fail("D1000", "旧密码错误");

        var pwd = EncryptUtility.Md5By32(req.Password);
        await _userRepo.UpdateColumnsAsync(x => new SysUserInfo { Password = pwd }, x => x.Id == userInfo.Id, cancellationToken);

        return CommonResult.Success();
    }
    
    /// <summary>
    ///     修改账户信息
    /// </summary>
    /// <returns></returns>
    [HttpPut("/api/auth/user"), Authorize, Description("修改账户信息")]
    public virtual async Task<CommonResult> SaveUserAsync([FromBody] SaveUserDto req, CancellationToken cancellationToken)
    {
        var principal = GetRequiredService<IPrincipal>();
        var userId = _currentUser.UserId.To<long>();
        var userInfo = await _userRepo.FirstAsync(x => x.Id == userId, cancellationToken);
        if (userInfo == null)
            return CommonResult.Fail("D1000", "账户不存在");
        
        _userRepo.Attach(userInfo.Clone().As<SysUserInfo>());
        userInfo = req.MapTo(userInfo);
        userInfo.CheckUpdateAudited<SysUserInfo, long>(principal);
        await _userRepo.SaveAsync(userInfo, cancellationToken);

        return CommonResult.Success();
    }
}