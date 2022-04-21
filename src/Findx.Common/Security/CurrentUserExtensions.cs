using System;
using JetBrains.Annotations;

namespace Findx.Security
{
	/// <summary>
    /// 用户扩展
    /// </summary>
	public static class CurrentUserExtensions
	{
        [CanBeNull]
        public static string FindClaimValue(this ICurrentUser currentUser, string claimType)
        {
            return currentUser.FindClaim(claimType)?.Value;
        }
    }
}

