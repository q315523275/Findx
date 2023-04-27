using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc;
using Findx.Caching;
using Findx.Data;
using Findx.Extensions;
using Findx.Security;
using Findx.Security.Authentication.Jwt;
using Findx.Setting;
using Findx.Mapping;
using Findx.Module.EleAdmin.Enum;
using Findx.Module.EleAdmin.Models;
using System.ComponentModel;
using Findx.Module.EleAdmin.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Findx.Module.EleAdmin.Areas.System.Controller;

/// <summary>
/// 系统-账户
/// </summary>
[Area("system")]
[Route("api/[area]/auth")]
[Description("系统-账户")]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-账户")]
public class AuthController : AreaApiControllerBase
{
    private readonly IOptions<JwtOptions> _options;

    private readonly IJwtTokenBuilder _tokenBuilder;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheProvider _cacheProvider;

    private readonly IRepository<SysUserInfo> _repo;
    private readonly IRepository<SysLoginRecordInfo> _recordRepo;

    private readonly bool _enabledCaptcha;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="tokenBuilder"></param>
    /// <param name="options"></param>
    /// <param name="currentUser"></param>
    /// <param name="cacheProvider"></param>
    /// <param name="repo"></param>
    /// <param name="recordRepo"></param>
    /// <param name="settingProvider"></param>
    public AuthController(IJwtTokenBuilder tokenBuilder, IOptions<JwtOptions> options, ICurrentUser currentUser,
        ICacheProvider cacheProvider, IRepository<SysUserInfo> repo, IRepository<SysLoginRecordInfo> recordRepo,
        ISettingProvider settingProvider)
    {
        _tokenBuilder = tokenBuilder;
        _options = options;
        _currentUser = currentUser;
        _cacheProvider = cacheProvider;
        _repo = repo;
        _recordRepo = recordRepo;
        _enabledCaptcha = settingProvider.GetValue<bool>("Modules:EleAdmin:EnabledCaptcha");
    }

    /// <summary>
    /// 账号密码登录
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Description("登录")]
    [HttpPost("/api/login")]
    public async Task<CommonResult> Login([FromBody] LoginRequest req)
    {
        var cache = _cacheProvider.Get();
            
        // 是否开启验证码
        var cacheKey = $"verifyCode:{req.Uuid}";
        if (_enabledCaptcha && cache.Get<string>(cacheKey) != req.Code.ToLower())
            return CommonResult.Fail("50500", "验证码错误");
            
        // 密码错误次数
        var errorCacheKey = $"error:{req.UserName}";
        var errorCount = cache.Get<int>(errorCacheKey);
        if (errorCount >= 5)
            return CommonResult.Fail("50509", "密码错误次数超出限制");

        var accountInfo = await _repo.FirstAsync(it => it.UserName == req.UserName && it.TenantId == req.TenantId);
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
            Id = Utils.SequentialGuid.Instance.Create(_repo.GetDbType()),
            Ip = HttpContext.GetClientIp(),
            LoginType = 1,
            Os = userAgent.Platform.Name,
            TenantId = req.TenantId,
            UserName = req.UserName,
            Nickname = accountInfo.Nickname
        };

        CommonResult fail = null;

        // 验证帐号密码是否正确
        if (accountInfo.Password != Utils.Encrypt.Md5By32(req.Password))
        {
            // 增加错误次数
            errorCount++;
            await cache.AddAsync(errorCacheKey, errorCount, TimeSpan.FromMinutes(15));
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
            await cache.RemoveAsync(cacheKey);

        // 登录失败
        if (fail != null)
        {
            loginLog.LoginType = 1;
            await _recordRepo.InsertAsync(loginLog);
            return fail;
        }

        // 登录成功
        loginLog.LoginType = 0;
        loginLog.Comments = "登录成功";
        await _recordRepo.InsertAsync(loginLog);
        await cache.RemoveAsync(errorCacheKey);

        // token票据
        var payload = new Dictionary<string, string>
        {
            { ClaimTypes.UserId, accountInfo.Id.SafeString() },
            { ClaimTypes.UserName, accountInfo.UserName.SafeString() },
            { ClaimTypes.Nickname, accountInfo.Nickname.SafeString() },
            { ClaimTypes.TenantId, req.TenantId.ToString() },
            { ClaimTypes.UserIdTypeName, typeof(Guid).FullName },
            { "org_id", accountInfo.OrgId.SafeString() },
            { "org_name", accountInfo.OrgName.SafeString() }
        };
        var token = await _tokenBuilder.CreateAsync(payload, _options.Value);

        return CommonResult.Success(new { access_token = "Bearer " + token.AccessToken });
    }

    /// <summary>
    /// 查看账户信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("/api/auth/user")]
    [Description("查看账户信息")]
    [Authorize]
    public new CommonResult User()
    {
        var userId = _currentUser.UserId.To<Guid>();
        var userInfo = _repo.First(x => x.Id == userId);
        if (userInfo == null)
            return CommonResult.Fail("D1000", "账户不存在");

        var roleRepo = GetRepository<SysUserRoleInfo>();
        var menuRepo = GetRepository<SysRoleMenuInfo>();
        var appRepo = GetRepository<SysAppInfo>();

        var roles = roleRepo.Select(x => x.UserId == userId && x.RoleId == x.RoleInfo.Id, selectExpression: x => new RoleDto { Id = x.RoleId, RoleCode = x.RoleInfo.Code, RoleName = x.RoleInfo.Name }).DistinctBy(x => x.Id);
        // ReSharper disable once PossibleMultipleEnumeration
        var roleIds = roles.Select(x => x.Id);
        // ReSharper disable once PossibleMultipleEnumeration
        var menus = roleIds.Any()
            ?
            // ReSharper disable once PossibleMultipleEnumeration
            menuRepo.Select(x => roleIds.Contains(x.RoleId) && x.MenuId == x.MenuInfo.Id, selectExpression: x => new MenuDto { MenuId = x.MenuId })
            : new List<MenuDto>();

        var appCodes = menus.Select(x => x.ApplicationCode).Distinct();
        var appList = appRepo.Select(x => appCodes.Contains(x.Code), x => new AppDto());

        var result = userInfo.MapTo<UserAuthDto>();
        // ReSharper disable once PossibleMultipleEnumeration
        result.Roles = roles;
        result.Authorities = menus.DistinctBy(x => x.MenuId).OrderBy(x => x.Sort);
        result.Apps = appList;

        return CommonResult.Success(result);
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPut("/api/auth/password")]
    [Authorize]
    [Description("修改账户密码")]
    public CommonResult Password([FromBody] UpdatePasswordRequest req)
    {
        var userId = _currentUser.UserId.To<Guid>();
        // var tenantId = _currentUser.TenantId.To<Guid>();
        var userInfo = _repo.First(x => x.Id == userId);
        if (userInfo == null)
            return CommonResult.Fail("D1000", "账户不存在");

        if (userInfo.Password != Utils.Encrypt.Md5By32(req.OldPassword))
            return CommonResult.Fail("D1000", "旧密码错误");

        var pwd = Utils.Encrypt.Md5By32(req.Password);
        _repo.UpdateColumns(x => new SysUserInfo { Password = pwd }, x => x.Id == userInfo.Id);

        return CommonResult.Success();
    }
}