using System.Security.Principal;
using Findx.AspNetCore.Extensions;
using Findx.Caching;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Mapping;
using Findx.Module.EleAdminPlus.Shared.Dtos.Auth;
using Findx.Module.EleAdminPlus.Shared.Enums;
using Findx.Module.EleAdminPlus.Shared.Models;
using Findx.Module.EleAdminPlus.Shared.ServiceDefaults;
using Findx.Module.EleAdminPlus.Shared.Vos.Auth;
using Findx.NewId;
using Findx.Security;
using Findx.Security.Authentication.Jwt;
using Findx.Setting;
using Findx.Storage;
using Findx.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Findx.Module.EleAdminPlus.ServiceDefaults;

/// <summary>
///     认证服务实现
/// </summary>
public class AuthService : IAuthService, IScopeDependency
{
    private readonly IRepository<SysLoginRecordInfo, long> _loginRecordRepo;
    private readonly IRepository<SysUserInfo, long> _userRepo;
    private readonly IKeyGenerator<long> _keyGenerator;
    
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    private readonly IJwtTokenBuilder _tokenBuilder;
    private readonly IOptions<JwtOptions> _options;
    private readonly bool _enabledCaptcha;

    private readonly ICacheFactory _cacheFactory;
    private readonly string _defaultCacheType;
    
    /// <summary>
    ///     Ctor
    /// </summary>
    public AuthService(
        IJwtTokenBuilder tokenBuilder, 
        IOptions<JwtOptions> options, 
        ICacheFactory cacheFactory, 
        IRepository<SysUserInfo, long> userRepo, 
        IRepository<SysLoginRecordInfo, long> loginRecordRepo, 
        ISettingProvider settingProvider, 
        IKeyGenerator<long> keyGenerator,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider)
    {
        _tokenBuilder = tokenBuilder;
        _options = options;
        _cacheFactory = cacheFactory;
        _userRepo = userRepo;
        _loginRecordRepo = loginRecordRepo;
        _keyGenerator = keyGenerator;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
        
        _enabledCaptcha = settingProvider.GetValue<bool>("Modules:EleAdminPlus:EnabledCaptcha");
        _defaultCacheType = settingProvider.GetValue<string>("Modules:EleAdminPlus:CacheType") ?? CacheType.DefaultMemory;
    }
    
    /// <summary>
    ///     账号登录
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<CommonResult> LoginAsync(LoginRequestDto req, CancellationToken cancellationToken = default)
    {
        // 1. 验证码校验
        var captchaValidation = await ValidateCaptchaAsync(req, cancellationToken);
        if (!captchaValidation.IsSuccess()) 
            return captchaValidation;

        // 2. 密码错误次数检查
        var passwordValidation = await ValidatePasswordAttemptAsync(req.UserName, cancellationToken);
        if (!passwordValidation.IsSuccess()) 
            return passwordValidation;

        // 3. 用户身份验证
        var userValidation = await ValidateUserCredentialsAsync(req, cancellationToken);
        if (!userValidation.IsSuccess()) 
            return userValidation;

        // 4. 登录成功处理
        return await HandleSuccessfulLoginAsync(req, userValidation.Data, cancellationToken);
    }
    
    /// <summary>
    ///     验证码验证
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<CommonResult> ValidateCaptchaAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (!_enabledCaptcha) return CommonResult.Success();

        if (request.Code.IsNullOrWhiteSpace() || request.Uuid.IsNullOrWhiteSpace()) return CommonResult.Fail("D1000", "验证码不能为空");
        
        var cache = _cacheFactory.Create(_defaultCacheType);
        var cacheKey = $"verifyCode:{request.Uuid}";
        var cacheValue = await cache.GetAsync<string>(cacheKey, cancellationToken);
    
        return !string.Equals(cacheValue.SafeString(), request.Code, StringComparison.OrdinalIgnoreCase) ? CommonResult.Fail("50500", "验证码错误") : CommonResult.Success();
    }
    
    /// <summary>
    ///     密码尝试次数检查
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<CommonResult> ValidatePasswordAttemptAsync(string userName, CancellationToken cancellationToken)
    {
        var cache = _cacheFactory.Create(_defaultCacheType);
        var errorCacheKey = $"error:{userName}";
        var errorCount = await cache.GetAsync<int>(errorCacheKey, cancellationToken);
    
        return errorCount >= 5 ? CommonResult.Fail("50509", "密码错误次数超出限制") : CommonResult.Success();
    }
    
    /// <summary>
    ///     用户凭证验证
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<CommonResult<SysUserInfo>> ValidateUserCredentialsAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var accountInfo = await _userRepo.FirstAsync(it => it.UserName == request.UserName && it.TenantId == request.TenantId, cancellationToken);
    
        //  验证帐号是否存在
        if (accountInfo == null) return CommonResult.Fail<SysUserInfo>("D1000", "账户不存在");

        //  验证帐号密码是否正确
        if (!accountInfo.Password.Equals(EncryptUtility.Md5By32(request.Password), StringComparison.OrdinalIgnoreCase))
        {
            await RecordFailedLoginAttemptAsync(request, accountInfo, "账号密码错误", cancellationToken);
            return CommonResult.Fail<SysUserInfo>("D1000", "账号密码错误");
        }

        //  验证账号是否被冻结
        if (accountInfo.Status != CommonStatus.Enable.To<int>())
        {
            await RecordFailedLoginAttemptAsync(request, accountInfo, "账号已冻结", cancellationToken);
            return CommonResult.Fail<SysUserInfo>("D1017", "账号已冻结");
        }

        return CommonResult.Success(accountInfo);
    }
    
    /// <summary>
    ///     失败登录记录
    /// </summary>
    /// <param name="request"></param>
    /// <param name="accountInfo"></param>
    /// <param name="comments"></param>
    /// <param name="cancellationToken"></param>
    protected virtual async Task RecordFailedLoginAttemptAsync(LoginRequestDto request, SysUserInfo accountInfo, string comments, CancellationToken cancellationToken)
    {
        var userAgent = _httpContextAccessor.HttpContext?.Request.GetUserAgent();
        var loginLog = new SysLoginRecordInfo
        {
            Browser = userAgent?.Browser.Name,
            Comments = comments,
            CreatedTime = DateTime.Now,
            Device = userAgent?.Os.Name,
            Id = _keyGenerator.Create(),
            Ip = _httpContextAccessor.HttpContext?.GetClientIp(),
            LoginType = 1,
            Os = userAgent?.Platform.Name,
            TenantId = request.TenantId,
            UserName = request.UserName,
            Nickname = accountInfo.Nickname
        };

        await _loginRecordRepo.InsertAsync(loginLog, cancellationToken);
    
        //  更新密码错误次数
        var cache = _cacheFactory.Create(_defaultCacheType);
        var errorCacheKey = $"error:{request.UserName}";
        var errorCount = await cache.GetAsync<int>(errorCacheKey, cancellationToken);
        errorCount++;
        await cache.AddAsync(errorCacheKey, errorCount, TimeSpan.FromMinutes(15), cancellationToken);
    
        //  清空验证码
        if (_enabledCaptcha)
        {
            var cacheKey = $"verifyCode:{request.Uuid}";
            await cache.RemoveAsync(cacheKey, cancellationToken);
        }
    }
    
    /// <summary>
    ///     成功登录处理
    /// </summary>
    /// <param name="request"></param>
    /// <param name="accountInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<CommonResult> HandleSuccessfulLoginAsync(LoginRequestDto request, SysUserInfo accountInfo, CancellationToken cancellationToken)
    {
        //  记录成功登录
        await RecordSuccessfulLoginAsync(request, accountInfo, cancellationToken);
    
        //  清除错误计数和验证码
        await ClearLoginArtifactsAsync(request, cancellationToken);
    
        //  生成Token
        return await GenerateAuthTokenAsync(accountInfo, request.TenantId, cancellationToken);
    }
    
    /// <summary>
    ///     记录成功登录
    /// </summary>
    /// <param name="request"></param>
    /// <param name="accountInfo"></param>
    /// <param name="cancellationToken"></param>
    private async Task RecordSuccessfulLoginAsync(LoginRequestDto request, SysUserInfo accountInfo, CancellationToken cancellationToken)
    {
        var userAgent = _httpContextAccessor.HttpContext?.Request.GetUserAgent();
        var loginLog = new SysLoginRecordInfo
        {
            Browser = userAgent?.Browser.Name,
            Comments = "登录成功",
            CreatedTime = DateTime.Now,
            Device = userAgent?.Os.Name,
            Id = _keyGenerator.Create(),
            Ip = _httpContextAccessor.HttpContext?.GetClientIp(),
            LoginType = 0, // 0表示成功
            Os = userAgent?.Platform.Name,
            TenantId = request.TenantId,
            UserName = request.UserName,
            Nickname = accountInfo.Nickname
        };

        await _loginRecordRepo.InsertAsync(loginLog, cancellationToken);
    }

    /// <summary>
    ///     清除登录相关缓存数据
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    private async Task ClearLoginArtifactsAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var cache = _cacheFactory.Create(_defaultCacheType);
    
        // 清除密码错误次数
        var errorCacheKey = $"error:{request.UserName}";
        await cache.RemoveAsync(errorCacheKey, cancellationToken);
    
        // 清除验证码
        if (_enabledCaptcha)
        {
            var cacheKey = $"verifyCode:{request.Uuid}";
            await cache.RemoveAsync(cacheKey, cancellationToken);
        }
    }
    
    /// <summary>
    ///     生成认证Token
    /// </summary>
    /// <param name="accountInfo"></param>
    /// <param name="tenantId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<CommonResult> GenerateAuthTokenAsync(SysUserInfo accountInfo, string tenantId, CancellationToken cancellationToken)
    {
        //  token票据
        var payload = new Dictionary<string, string>
        {
            { ClaimTypes.UserId, accountInfo.Id.SafeString() },
            { ClaimTypes.UserIdTypeName, typeof(long).FullName },
            { ClaimTypes.UserName, accountInfo.UserName.SafeString() },
            { ClaimTypes.Nickname, accountInfo.Nickname.SafeString() },
            { ClaimTypes.TenantId, tenantId },
            { ClaimTypes.OrgId, accountInfo.OrgId.SafeString() },
            { ClaimTypes.OrgName, accountInfo.OrgName.SafeString() }
        };
    
        //  角色id及编号
        var roleRepo = _serviceProvider.GetRequiredService<IRepository<SysUserRoleInfo, long>>();
        var roles = await roleRepo.SelectAsync(x => x.UserId == accountInfo.Id && x.RoleId == x.RoleInfo.Id, x => new UserAuthRoleSimplifyDto { Id = x.RoleId, RoleCode = x.RoleInfo.Code, RoleName = x.RoleInfo.Name }, cancellationToken: cancellationToken);
    
        payload[ClaimTypes.RoleIds] = roles.Select(x => x.Id).Distinct().JoinAsString(",");
        payload[ClaimTypes.Role] = roles.Select(x => x.RoleCode).Distinct().JoinAsString(",");
    
        //  请求token
        var token = await _tokenBuilder.CreateAsync(payload, _options.Value);

        //  返回内容
        var rs = new
        {
            AccessToken = "Bearer " + token.AccessToken,
            AccessTokenUtcExpires = token.AccessTokenUtcExpires
        };
        return CommonResult.Success(rs);
    }
    
    /// <summary>
    ///     退出登录
    /// </summary>
    /// <returns></returns>
    public virtual Task<CommonResult> LogoutAsync(long userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CommonResult.Success());
    }
    
    /// <summary>
    ///     查看账户信息
    /// </summary>
    /// <returns></returns>
    public virtual async Task<CommonResult<UserAuthSimplifyDto>> GetUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var userInfo = await _userRepo.FirstAsync(x => x.Id == userId, cancellationToken);
        if (userInfo == null) return CommonResult.Fail<UserAuthSimplifyDto>("D1000", "账户不存在");

        var roleRepo = _serviceProvider.GetRequiredService<IRepository<SysUserRoleInfo, long>>();
        var roles = await roleRepo.SelectAsync(x => x.UserId == userId && x.RoleId == x.RoleInfo.Id, x => new UserAuthRoleSimplifyDto { Id = x.RoleId, RoleCode = x.RoleInfo.Code, RoleName = x.RoleInfo.Name }, cancellationToken: cancellationToken);
        var result = userInfo.MapTo<UserAuthSimplifyDto>();
        result.Roles = roles.DistinctBy(x => x.Id);

        return CommonResult.Success(result);
    }
    
    /// <summary>
    ///     查看账户菜单
    /// </summary>
    /// <returns></returns>
    public virtual async Task<CommonResult<IEnumerable<UserAuthMenuSimplifyDto>>> GetUserMenusAsync(long userId, string code, CancellationToken cancellationToken = default)
    {
        //  实体仓储
        var roleRepo = _serviceProvider.GetRequiredService<IRepository<SysUserRoleInfo, long>>();
        var menuRepo = _serviceProvider.GetRequiredService<IRepository<SysRoleMenuInfo, long>>();

        //  合并菜单查询
        var roleIds = await roleRepo.SelectAsync(x => x.UserId == userId && x.RoleId == x.RoleInfo.Id, x => x.RoleId, cancellationToken: cancellationToken);
        var menus = new List<UserAuthMenuSimplifyDto>();
        if (roleIds.Any())
            menus = await menuRepo.SelectAsync(x => roleIds.Contains(x.RoleId) && x.MenuId == x.MenuInfo.Id, x => new UserAuthMenuSimplifyDto { MenuId = x.MenuId }, cancellationToken: cancellationToken);
        
        //  去重排序
        var res = menus.DistinctBy(x => x.MenuId).OrderBy(x => x.Sort).AsEnumerable();

        return CommonResult.Success(res);
    }

    /// <summary>
    ///     修改密码
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<CommonResult> UpdatePasswordAsync(long userId, UpdatePasswordDto request, CancellationToken cancellationToken = default)
    {
        var principal = _serviceProvider.GetService<IPrincipal>();
        
        var userInfo = await _userRepo.FirstAsync(x => x.Id == userId, cancellationToken);
        if (userInfo == null)
            return CommonResult.Fail("D1000", "账户不存在");

        if (!userInfo.Password.Equals(EncryptUtility.Md5By32(request.OldPassword), StringComparison.OrdinalIgnoreCase))
            return CommonResult.Fail("D1000", "旧密码错误");
        
        userInfo.Password = EncryptUtility.Md5By32(request.Password);
        userInfo.CheckUpdateAudited<SysUserInfo, long>(principal);
        await _userRepo.SaveAsync(userInfo, cancellationToken);

        return CommonResult.Success();
    }
    
    /// <summary>
    ///     修改账户信息
    /// </summary>
    /// <returns></returns>
    public virtual async Task<CommonResult> UpdateUserAsync(long userId, UpdateUserDto request, CancellationToken cancellationToken = default)
    {
        var principal = _serviceProvider.GetService<IPrincipal>();
        var userInfo = await _userRepo.FirstAsync(x => x.Id == userId, cancellationToken);
        if (userInfo == null) return CommonResult.Fail("D1000", "账户不存在");
        
        userInfo = request.MapTo(userInfo);
        userInfo.CheckUpdateAudited<SysUserInfo, long>(principal);
        await _userRepo.SaveAsync(userInfo, cancellationToken);

        return CommonResult.Success();
    }
    
    /// <summary>
    ///     修改账户头像
    /// </summary>
    /// <returns></returns>
    public virtual async Task<CommonResult> UpdateUserAvatarAsync(long userId, UpdateUserAvatarDto request, CancellationToken cancellationToken = default)
    {
        var principal = _serviceProvider.GetService<IPrincipal>();
        var userInfo = await _userRepo.FirstAsync(x => x.Id == userId, cancellationToken);
        if (userInfo == null) return CommonResult.Fail("D1000", "账户不存在");
        
        // 头像处理,覆盖模式
        
        //  服务接口
        var fileStorage = _serviceProvider.GetRequiredService<IFileStorage>();
        var settingProvider = _serviceProvider.GetRequiredService<ISettingProvider>();
        var applicationContext = _serviceProvider.GetRequiredService<IApplicationContext>();
        //  图片参数处理
        var folderHost = settingProvider.GetValue<string>("Findx:Storage:Folder:Host") ?? $"//{HostUtility.ResolveHostAddress(HostUtility.ResolveHostName())}:{applicationContext.Port}";
        var pathDir = Path.Combine("storage", "avatar");
        var fileExt = "jpeg";
        var imageBase64 = request.Avatar;
        if (request.Avatar.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            fileExt = request.Avatar.Replace("data:image/", string.Empty).Split(";")[0];
            imageBase64 = request.Avatar.Replace($"data:image/{fileExt};base64,", string.Empty);
        }
        var saveName = $"{_keyGenerator.Create()}.{fileExt}"; // 文件名
        var path = Path.Combine(pathDir, saveName);
        var fullPath = Path.Combine(applicationContext.RootPath, path);
        //  图片压缩
        var imageByte = Convert.FromBase64String(imageBase64);
        //  物理存储
        await fileStorage.SaveFileAsync(fullPath, imageByte, cancellationToken);
        //  删除原始头像
        if (!string.IsNullOrWhiteSpace(userInfo.Avatar))
        {
            var oldPath = Path.Combine(applicationContext.RootPath, userInfo.Avatar.RemovePreFix(folderHost).NormalizePath());
            await fileStorage.DeleteFileAsync(oldPath, cancellationToken);
        }
        //  落库
        userInfo.Avatar = Path.Combine(folderHost, path).NormalizePath();
        userInfo.CheckUpdateAudited<SysUserInfo, long>(principal);
        await _userRepo.SaveAsync(userInfo, cancellationToken);

        return CommonResult.Success();
    }
}