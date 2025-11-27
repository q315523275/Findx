using Findx.Data;
using Findx.Module.EleAdminPlus.Shared.Dtos.Auth;
using Findx.Module.EleAdminPlus.Shared.Vos.Auth;

namespace Findx.Module.EleAdminPlus.Shared.ServiceDefaults;

/// <summary>
/// 认证服务接口
/// </summary>
public interface IAuthService
{
    /// <summary>
    ///     用户登录
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CommonResult> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     用户登出
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CommonResult> LogoutAsync(long userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     获取用户信息
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CommonResult<UserAuthSimplifyDto>> GetUserAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     获取用户菜单
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="code"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CommonResult<IEnumerable<UserAuthMenuSimplifyDto>>> GetUserMenusAsync(long userId, string code, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     更新密码
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CommonResult> UpdatePasswordAsync(long userId, UpdatePasswordDto request, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     更新用户信息
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CommonResult> UpdateUserAsync(long userId, UpdateUserDto request, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     更新用户头像
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CommonResult> UpdateUserAvatarAsync(long userId, UpdateUserAvatarDto request, CancellationToken cancellationToken = default);
}