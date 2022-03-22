using System.Collections.Generic;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Module.Admin.Enum;
using Findx.Module.Admin.Models;
using Findx.Security;
using Microsoft.AspNetCore.Http;

namespace Findx.Module.Admin
{
    /// <summary>
    /// 管理员扩展
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// 是否超级管理员
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public static bool IsSuperAdmin(this SysUserInfo userInfo)
        {
            return AdminTypeEnum.SuperAdmin.To<int>() == userInfo.AdminType;
        }

        /// <summary>
        /// 数据范围
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public static List<long> DataScope(this SysUserInfo userInfo)
        {
            return (List<long>)ServiceLocator.GetService<IHttpContextAccessor>().HttpContext?.Items.GetOrDefault("dataScope");
        }

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
		public static bool IsSuperAdmin(this ICurrentUser user)
        {
            return user.FindClaim(Const.ClaimConst.SUPER_ADMIN)?.Value == ((int)AdminTypeEnum.SuperAdmin).ToString();
        }

        /// <summary>
        /// 获取组织机构编号
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
		public static T OrgId<T>(this ICurrentUser user)
        {
            var value = user.FindClaim(Const.ClaimConst.ORG_ID)?.Value;
            return value.IsNullOrWhiteSpace() ? default : value.CastTo<T>();
        }

        /// <summary>
        /// 获取组织机构名称
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
		public static string OrgName(this ICurrentUser user)
        {
            return user.FindClaim(Const.ClaimConst.ORG_NAME)?.Value;
        }

        /// <summary>
        /// 数据范围
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public static List<long> DataScope(this ICurrentUser user)
        {
            return (List<long>)ServiceLocator.GetService<IHttpContextAccessor>().HttpContext?.Items.GetOrDefault("dataScope");
        }
    }
}
