﻿using System.ComponentModel;

namespace Findx.Security;

/// <summary>
///     功能访问类型
/// </summary>
public enum FunctionAccessType
{
    /// <summary>
    ///     匿名用户可访问
    /// </summary>
    [Description("匿名")] Anonymous = 0,

    /// <summary>
    ///     登录用户可访问
    /// </summary>
    [Description("登录")] Login = 1,

    /// <summary>
    ///     指定角色可访问
    /// </summary>
    [Description("角色")] RoleLimit = 2,

    /// <summary>
    ///     指定权限可访问
    /// </summary>
    [Description("权限资源")] AuthorityLimit = 3,

    /// <summary>
    ///     指定角色权限可访问
    /// </summary>
    [Description("角色及权限资源")] RoleAuthorityLimit = 4
}