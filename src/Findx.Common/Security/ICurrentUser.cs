using System.Security.Claims;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Findx.Security
{
    /// <summary>
    /// 当前用户
    /// </summary>
    public interface ICurrentUser
    {
        /// <summary>
        /// 是否已认证
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// 用户标识
        /// </summary>
        [CanBeNull]
        string UserId { get; }

        /// <summary>
        /// 用户名
        /// </summary>
        [CanBeNull]
        string UserName { get; }

        /// <summary>
        /// 姓名
        /// </summary>
        [CanBeNull]
        string Name { get; }
        
        /// <summary>
        /// 昵称
        /// </summary>
        [CanBeNull]
        string Nickname { get; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [CanBeNull]
        string PhoneNumber { get; }

        /// <summary>
        /// 是否已验证手机号码
        /// </summary>
        bool PhoneNumberVerified { get; }

        /// <summary>
        /// 电子邮箱
        /// </summary>
        [CanBeNull]
        string Email { get; }

        /// <summary>
        /// 是否已验证邮箱
        /// </summary>
        bool EmailVerified { get; }

        /// <summary>
        /// 租户标识
        /// </summary>
        [CanBeNull]
        string TenantId { get; }

        /// <summary>
        /// 角色列表
        /// </summary>
        [NotNull]
        IEnumerable<string> Roles { get; }

        /// <summary>
        /// 查找声明
        /// </summary>
        /// <param name="claimType">声明类型</param>
        [CanBeNull]
        Claim FindClaim(string claimType);

        /// <summary>
        /// 查找声明列表
        /// </summary>
        /// <param name="claimType">声明类型</param>
        [NotNull]
        IEnumerable<Claim> FindClaims(string claimType);

        /// <summary>
        /// 获取所有声明列表
        /// </summary>
        [NotNull]
        IEnumerable<Claim> GetAllClaims();

        /// <summary>
        /// 是否包含指定角色
        /// </summary>
        /// <param name="roleName">角色名</param>
        bool IsInRole(string roleName);
    }
}
