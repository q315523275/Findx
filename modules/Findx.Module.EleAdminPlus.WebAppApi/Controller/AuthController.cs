using System.ComponentModel;
using System.Security.Principal;
using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Module.EleAdminPlus.Shared.Dtos.Auth;
using Findx.Module.EleAdminPlus.Shared.ServiceDefaults;
using Findx.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.WebAppApi.Controller;

/// <summary>
///     系统-账户
/// </summary>
[Area("system")]
[Route("api/[area]/auth")]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-账户"), Description("系统-账户")]
public class AuthController : AreaApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWorkContext _workContext;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="authService"></param>
    /// <param name="workContext"></param>
    public AuthController(IAuthService authService, IWorkContext workContext)
    {
        _authService = authService;
        _workContext = workContext;
    }
    
    /// <summary>
    ///     账号登录
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/login"), Description("登录")]
    public virtual async Task<CommonResult> LoginAsync([FromBody] LoginRequestDto req, CancellationToken cancellationToken = default)
    {
        return await _authService.LoginAsync(req, cancellationToken);
    }

    /// <summary>
    ///     退出登录
    /// </summary>
    /// <returns></returns>
    [HttpGet("/api/auth/logout"), Authorize, Description("退出登录")]
    public virtual async Task<CommonResult> LogoutAsync(CancellationToken cancellationToken)
    {
        var userId = _workContext.GetCurrentUser().UserId;
        return await _authService.LogoutAsync(userId, cancellationToken);
    }
    
    /// <summary>
    ///     查看账户信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("/api/auth/user"), Authorize, Description("查看账户信息")]
    public virtual async Task<CommonResult> UserAsync(CancellationToken cancellationToken)
    {
        var userId = _workContext.GetCurrentUser().UserId;
        return await _authService.GetUserAsync(userId, cancellationToken);
    }
    
    /// <summary>
    ///     查看账户菜单
    /// </summary>
    /// <returns></returns>
    [HttpGet("/api/auth/menus"), Authorize, Description("查看账户菜单")]
    public virtual async Task<CommonResult> MenusAsync(string code, CancellationToken cancellationToken)
    {
        var userId = _workContext.GetCurrentUser().UserId;
        return await _authService.GetUserMenusAsync(userId, code, cancellationToken);
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
        var userId = _workContext.GetCurrentUser().UserId;
        return await _authService.UpdatePasswordAsync(userId, req, cancellationToken);
    }
    
    /// <summary>
    ///     修改账户信息
    /// </summary>
    /// <returns></returns>
    [HttpPut("/api/auth/user"), Authorize, Description("修改账户信息")]
    public virtual async Task<CommonResult> SaveUserAsync([FromBody] UpdateUserDto req, CancellationToken cancellationToken)
    {
        var userId = _workContext.GetCurrentUser().UserId;
        return await _authService.UpdateUserAsync(userId, req, cancellationToken);
    }
    
    /// <summary>
    ///     修改账户头像
    /// </summary>
    /// <returns></returns>
    [HttpPut("/api/auth/user/avatar"), Authorize, Description("修改账户头像")]
    public virtual async Task<CommonResult> SaveUserAvatarAsync([FromBody] UpdateUserAvatarDto req, CancellationToken cancellationToken)
    {
        var userId = _workContext.GetCurrentUser().UserId;
        return await _authService.UpdateUserAvatarAsync(userId, req, cancellationToken);
    }
}