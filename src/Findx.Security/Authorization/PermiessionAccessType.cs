﻿using System.ComponentModel;

namespace Findx.Security.Authorization
{
    /// <summary>
    /// 权限资方访问方式
    /// </summary>
    public enum PermiessionAccessType
    {
        /// <summary>
        /// 匿名用户可访问
        /// </summary>
        [Description("匿名")]
        Anonymous = 0,

        /// <summary>
        /// 登录用户可访问
        /// </summary>
        [Description("登录")]
        Login = 1,

        /// <summary>
        /// 指定角色可访问
        /// </summary>
        [Description("角色")]
        RoleLimit = 2
    }
}
