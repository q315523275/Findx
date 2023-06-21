
namespace Findx.Security;

/// <summary>
///     用户扩展
/// </summary>
public static class CurrentUserExtensions
{
    /// <summary>
    ///     获取声明对应结果值
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="claimType"></param>
    /// <returns></returns>
    public static string FindClaimValue(this ICurrentUser currentUser, string claimType)
    {
        return currentUser.FindClaim(claimType)?.Value;
    }
}