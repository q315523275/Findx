using System.Security.Claims;
using System.Security.Principal;
using Findx.Common;

namespace Findx.Security;

/// <summary>
///     功能权限检查基类
/// </summary>
public abstract class FunctionAuthorizationBase : IFunctionAuthorization
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="settingProvider"></param>
    protected FunctionAuthorizationBase(IConfiguration settingProvider)
    {
        SuperRoleName = settingProvider.GetValue<string>("Findx:Authorization:SuperRoleName");
    }

    /// <summary>
    ///     获取 超级管理员角色
    /// </summary>
    protected virtual string SuperRoleName { get; }

    /// <summary>
    ///     检查指定用户是否有执行指定功能的权限
    /// </summary>
    /// <param name="function">要检查的功能</param>
    /// <param name="principal">在线用户信息</param>
    /// <returns>功能权限检查结果</returns>
    public AuthorizationStatus Authorize(IFunction function, IPrincipal principal)
    {
        return AuthorizeCore(function, principal);
    }

    /// <summary>
    ///     获取功能权限检查通过的角色
    /// </summary>
    /// <param name="function">要检查的功能</param>
    /// <param name="principal">在线用户信息</param>
    /// <returns>通过的角色</returns>
    public virtual IEnumerable<string> GetOkRoles(IFunction function, IPrincipal principal)
    {
        if (!principal.Identity.IsAuthenticated) return Array.Empty<string>();

        var userRoles = principal.Identity.GetRoles();
        if (function.AccessType != FunctionAccessType.RoleLimit)
            // 不是角色限制的功能，允许用户的所有角色
            return userRoles;

        var functionRoles = function.Roles.Split(",");
        return userRoles.Intersect(functionRoles);
    }

    /// <summary>
    ///     重写以实现权限检查核心验证操作
    /// </summary>
    /// <param name="function">要验证的功能信息</param>
    /// <param name="principal">当前用户在线信息</param>
    /// <returns>功能权限验证结果</returns>
    protected virtual AuthorizationStatus AuthorizeCore(IFunction function, IPrincipal principal)
    {
        if (function == null) return AuthorizationStatus.NoFound;

        if (function.IsLocked) return AuthorizationStatus.Locked;

        if (function.AccessType == FunctionAccessType.Anonymous) return AuthorizationStatus.Ok;

        // 未登录
        if (principal == null || !principal.Identity.IsAuthenticated) return AuthorizationStatus.Unauthorized;

        // 已登录，无角色限制
        if (function.AccessType == FunctionAccessType.Login) return AuthorizationStatus.Ok;

        // 已登录，验证角色
        if (function.AccessType == FunctionAccessType.RoleLimit) return AuthorizeRoleLimit(function, principal);

        // 已登录，验证权限
        if (function.AccessType == FunctionAccessType.AuthorityLimit)
            return AuthorizeAuthorityLimit(function, principal);

        // 已登录，验证角色及权限资源
        if (function.AccessType == FunctionAccessType.RoleAuthorityLimit)
            return AuthorizeRoleAuthorityLimit(function, principal);

        return AuthorizationStatus.NoFound;
    }

    /// <summary>
    ///     重写以实现 角色限制 的功能的功能权限检查
    /// </summary>
    /// <param name="function">要验证的功能信息</param>
    /// <param name="principal">用户在线信息</param>
    /// <returns>功能权限验证结果</returns>
    protected virtual AuthorizationStatus AuthorizeRoleLimit(IFunction function, IPrincipal principal)
    {
        // 角色限制
        // 检查角色-功能的权限
        return principal.Identity is not ClaimsIdentity identity
            ? AuthorizationStatus.Error
            : AuthorizeRoleNames(function, identity.GetRoles());
    }

    /// <summary>
    ///     重写以实现指定角色是否有执行指定功能的权限
    /// </summary>
    /// <param name="function">功能信息</param>
    /// <param name="roleNames">角色名称</param>
    /// <returns>功能权限检查结果</returns>
    protected virtual AuthorizationStatus AuthorizeRoleNames(IFunction function, IEnumerable<string> roleNames)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        Check.NotNull(roleNames, nameof(roleNames));

        // ReSharper disable once PossibleMultipleEnumeration
        if (!roleNames.Any()) return AuthorizationStatus.Forbidden;

        // ReSharper disable once PossibleMultipleEnumeration
        if (function.AccessType != FunctionAccessType.RoleLimit || roleNames.Contains(SuperRoleName))
            return AuthorizationStatus.Ok;

        var functionRoleNames = function.Roles.Split(",");
        // ReSharper disable once PossibleMultipleEnumeration
        return roleNames.Intersect(functionRoleNames).Any() ? AuthorizationStatus.Ok : AuthorizationStatus.Forbidden;
    }


    /// <summary>
    ///     重写以实现 权限限制 的功能的功能权限检查
    /// </summary>
    /// <param name="function">要验证的功能信息</param>
    /// <param name="principal">用户在线信息</param>
    /// <returns>功能权限验证结果</returns>
    protected virtual AuthorizationStatus AuthorizeAuthorityLimit(IFunction function, IPrincipal principal)
    {
        // 拥有权限资源限制
        return AuthorizationStatus.Ok;
    }

    /// <summary>
    ///     重写以实现 角色及权限资源限制 的功能的功能权限检查
    /// </summary>
    /// <param name="function">要验证的功能信息</param>
    /// <param name="principal">用户在线信息</param>
    /// <returns>功能权限验证结果</returns>
    protected virtual AuthorizationStatus AuthorizeRoleAuthorityLimit(IFunction function, IPrincipal principal)
    {
        // 角色限制
        // 检查角色-功能的权限
        var roleLimitAuthorizeStatus = AuthorizeRoleLimit(function, principal);
        if (AuthorizeRoleLimit(function, principal) != AuthorizationStatus.Ok)
            return roleLimitAuthorizeStatus;

        // 权限资源限制
        // 检查权限资源-功能的权限
        return AuthorizeAuthorityLimit(function, principal);
    }
}