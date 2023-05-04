using System.Security.Principal;

namespace Findx.Security;

/// <summary>
///     定义功能权限验证
/// </summary>
public interface IFunctionAuthorization
{
    /// <summary>
    ///     检查指定用户是否有执行指定功能的权限
    /// </summary>
    /// <param name="function">要检查的功能</param>
    /// <param name="principal">在线用户信息</param>
    /// <returns>功能权限检查结果</returns>
    AuthorizationStatus Authorize(IFunction function, IPrincipal principal);

    /// <summary>
    ///     获取功能权限检查通过的角色
    /// </summary>
    /// <param name="function">要检查的功能</param>
    /// <param name="principal">在线用户信息</param>
    /// <returns>通过的角色</returns>
    IEnumerable<string> GetOkRoles(IFunction function, IPrincipal principal);
}