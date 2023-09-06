using System.Security.Claims;
using System.Security.Principal;
using Findx.Common;
using Findx.Extensions;

namespace Findx.Security;

/// <summary>
///     身份标识 - 扩展
/// </summary>
public static class ClaimsIdentityExtensions
{
    /// <summary>
    ///     获取指定类型的Claim
    /// </summary>
    public static Claim GetClaimFirstOrDefault(this IIdentity identity, string type)
    {
        Check.NotNull(identity, nameof(identity));
        return identity is not ClaimsIdentity claimsIdentity ? null : claimsIdentity.FindFirst(type);
    }

    /// <summary>
    ///     获取指定类型的Claim值
    /// </summary>
    public static string GetClaimValueFirstOrDefault(this IIdentity identity, string type)
    {
        Check.NotNull(identity, nameof(identity));
        return identity is not ClaimsIdentity claimsIdentity ? null : claimsIdentity.FindFirst(type)?.Value;
    }

    /// <summary>
    ///     获取指定类型的所有Claim值
    /// </summary>
    public static IEnumerable<Claim> GetClaims(this IIdentity identity, string type)
    {
        Check.NotNull(identity, nameof(identity));
        return identity is not ClaimsIdentity claimsIdentity
            ? Array.Empty<Claim>()
            : claimsIdentity.Claims.Where(m => m.Type == type);
    }

    /// <summary>
    ///     获取指定类型的所有Claim值
    /// </summary>
    public static IEnumerable<string> GetClaimValues(this IIdentity identity, string type)
    {
        Check.NotNull(identity, nameof(identity));
        return identity is not ClaimsIdentity claimsIdentity
            ? null
            : claimsIdentity.Claims.Where(m => m.Type == type).Select(m => m.Value);
    }

    /// <summary>
    ///     获取用户ID
    /// </summary>
    public static T GetUserId<T>(this IIdentity identity)
    {
        Check.NotNull(identity, nameof(identity));
        if (identity is not ClaimsIdentity claimsIdentity) return default;
        var value = claimsIdentity.FindFirst(ClaimTypes.UserId)?.Value;
        return value == null ? default : value.CastTo<T>();
    }

    /// <summary>
    ///     获取用户ID
    /// </summary>
    public static string GetUserId(this IIdentity identity)
    {
        Check.NotNull(identity, nameof(identity));
        return identity is not ClaimsIdentity claimsIdentity
            ? null
            : claimsIdentity.FindFirst(ClaimTypes.UserId)?.Value;
    }

    /// <summary>
    ///     获取用户名
    /// </summary>
    public static string GetUserName(this IIdentity identity)
    {
        Check.NotNull(identity, nameof(identity));
        return identity is not ClaimsIdentity claimsIdentity
            ? null
            : claimsIdentity.FindFirst(ClaimTypes.UserName)?.Value;
    }

    /// <summary>
    ///     获取Email
    /// </summary>
    public static string GetEmail(this IIdentity identity)
    {
        Check.NotNull(identity, nameof(identity));
        return identity is not ClaimsIdentity claimsIdentity ? null : claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    ///     获取昵称
    /// </summary>
    public static string GetNickname(this IIdentity identity)
    {
        Check.NotNull(identity, nameof(identity));
        return identity is not ClaimsIdentity claimsIdentity
            ? null
            : claimsIdentity.FindFirst(ClaimTypes.Nickname)?.Value;
    }

    /// <summary>
    ///     获取昵称
    /// </summary>
    public static string GetTenantId(this IIdentity identity)
    {
        Check.NotNull(identity, nameof(identity));
        return identity is not ClaimsIdentity claimsIdentity
            ? null
            : claimsIdentity.FindFirst(ClaimTypes.TenantId)?.Value;
    }

    /// <summary>
    ///     移除指定类型的声明
    /// </summary>
    public static void RemoveClaim(this IIdentity identity, string claimType)
    {
        Check.NotNull(identity, nameof(identity));
        if (identity is not ClaimsIdentity claimsIdentity) return;
        var claim = claimsIdentity.FindFirst(claimType);
        if (claim == null) return;
        claimsIdentity.RemoveClaim(claim);
    }

    /// <summary>
    ///     获取所有角色
    /// </summary>
    public static IEnumerable<string> GetRoles(this IIdentity identity)
    {
        Check.NotNull(identity, nameof(identity));
        if (identity is not ClaimsIdentity claimsIdentity) return Array.Empty<string>();
        // 不知道什么原因，netcore认证组建会自动将自定义role key转换为 System.Security.Claims.ClaimTypes.Role key
        return claimsIdentity.FindAll(System.Security.Claims.ClaimTypes.Role).SelectMany(m =>
        {
            return m.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        });
    }
}