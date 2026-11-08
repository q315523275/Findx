using System.Collections.Concurrent;
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
    ///     功能角色串 -> 角色数组缓存：避免每次授权都对 Roles 字段做 Split 分配
    /// </summary>
    private static readonly ConcurrentDictionary<string, string[]> FunctionRolesCache = new(StringComparer.Ordinal);

    /// <summary>
    ///     角色缓存上限（防御动态角色串导致缓存膨胀）
    /// </summary>
    private const int FunctionRolesCacheLimit = 2048;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="settingProvider"></param>
    protected FunctionAuthorizationBase(IConfiguration settingProvider)
    {
        //  未配置 SuperRoleName 时规约为空串：不匹配任何角色（超管角色名存实亡），避免 null 语义不确定
        SuperRoleName = settingProvider.GetValue<string>("Findx:Authorization:SuperRoleName") ?? string.Empty;
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
        //  principal 或 Identity 为 null 视为未登录
        if (principal?.Identity?.IsAuthenticated != true) return [];

        var userRoles = principal.Identity.GetRoles();
        if (function.AccessType != FunctionAccessType.RoleLimit)
            //  不是角色限制的功能，允许用户的所有角色
            return userRoles;

        //  按功能角色解析（带缓存），避免每次授权 Split + Linq Intersect 分配
        var functionRoles = GetFunctionRoles(function.Roles);
        var okRoles = new List<string>();
        foreach (var userRole in userRoles)
        {
            foreach (var functionRole in functionRoles)
            {
                if (string.Equals(userRole, functionRole, StringComparison.Ordinal))
                {
                    okRoles.Add(userRole);
                    break;
                }
            }
        }

        return okRoles;
    }

    /// <summary>
    ///     解析功能的角色串为数组（按原始字符串缓存）
    /// </summary>
    private static string[] GetFunctionRoles(string roles)
    {
        if (string.IsNullOrWhiteSpace(roles)) return [];

        if (FunctionRolesCache.TryGetValue(roles, out var cached)) return cached;

        var parsed = roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (FunctionRolesCache.Count < FunctionRolesCacheLimit)
            FunctionRolesCache[roles] = parsed;

        return parsed;
    }

    /// <summary>
    ///     重写以实现权限检查核心验证操作
    /// </summary>
    /// <param name="function">要验证的功能信息</param>
    /// <param name="principal">当前用户在线信息</param>
    /// <returns>功能权限验证结果</returns>
    protected virtual AuthorizationStatus AuthorizeCore(IFunction function, IPrincipal principal)
    {
        if (function == null) 
            return AuthorizationStatus.NoFound;

        if (function.IsLocked) 
            return AuthorizationStatus.Locked;

        if (function.AccessType == FunctionAccessType.Anonymous) 
            return AuthorizationStatus.Ok;

        //  未登录（principal 或 Identity 为 null 视为未登录）
        if (principal?.Identity?.IsAuthenticated != true) 
            return AuthorizationStatus.Unauthorized;

        //  已登录，无角色限制
        if (function.AccessType == FunctionAccessType.Login) 
            return AuthorizationStatus.Ok;

        //  已登录，验证角色
        if (function.AccessType == FunctionAccessType.RoleLimit) 
            return AuthorizeRoleLimit(function, principal);

        //  已登录，验证权限
        if (function.AccessType == FunctionAccessType.AuthorityLimit)
            return AuthorizeAuthorityLimit(function, principal);

        //  已登录，验证角色及权限资源
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
        //  角色限制
        //  检查角色-功能的权限
        return principal.Identity is not ClaimsIdentity identity ? AuthorizationStatus.Error : AuthorizeRoleNames(function, identity.GetRoles());
    }

    /// <summary>
    ///     重写以实现指定角色是否有执行指定功能的权限
    /// </summary>
    /// <param name="function">功能信息</param>
    /// <param name="roleNames">角色名称</param>
    /// <returns>功能权限检查结果</returns>
    protected virtual AuthorizationStatus AuthorizeRoleNames(IFunction function, IEnumerable<string> roleNames)
    {
        Check.NotNull(roleNames, nameof(roleNames));

        //  单次物化，避免对延迟枚举重复遍历（原实现 Any() 调用三次，多次重新解析角色）
        var roles = roleNames as string[] ?? roleNames.ToArray();
        if (roles.Length == 0) return AuthorizationStatus.Forbidden;

        if (function.AccessType != FunctionAccessType.RoleLimit) return AuthorizationStatus.Ok;

        foreach (var role in roles)
        {
            if (string.Equals(role, SuperRoleName, StringComparison.OrdinalIgnoreCase))
                return AuthorizationStatus.Ok;
        }

        //  角色串解析带缓存，直接顺序比较（原 Intersect 会额外分配 HashSet）
        var functionRoleNames = GetFunctionRoles(function.Roles);
        foreach (var role in roles)
        {
            foreach (var functionRole in functionRoleNames)
            {
                if (string.Equals(role, functionRole, StringComparison.Ordinal))
                    return AuthorizationStatus.Ok;
            }
        }

        return AuthorizationStatus.Forbidden;
    }


    /// <summary>
    ///     重写以实现 权限限制 的功能的功能权限检查
    /// </summary>
    /// <param name="function">要验证的功能信息</param>
    /// <param name="principal">用户在线信息</param>
    /// <returns>功能权限验证结果</returns>
    protected virtual AuthorizationStatus AuthorizeAuthorityLimit(IFunction function, IPrincipal principal)
    {
        //  基类未实现权限资源校验，默认拒绝（fail-closed），避免"权限资源级"功能退化为任意登录用户可访问
        //  子类应重写此方法实现资源级权限校验（如 Vue 按钮权限），未实现前一律视为无权
        return AuthorizationStatus.Forbidden;
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
        if (roleLimitAuthorizeStatus != AuthorizationStatus.Ok)
            return roleLimitAuthorizeStatus;

        // 权限资源限制
        // 检查权限资源-功能的权限
        return AuthorizeAuthorityLimit(function, principal);
    }
}