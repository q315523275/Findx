﻿using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.Auth;

/// <summary>
///     用户授权信息
/// </summary>
public class UserAuthDto : IResponse
{
    /// <summary>
    ///     账号
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    ///     昵称
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     头像
    /// </summary>
    public string Avatar { get; set; }

    /// <summary>
    ///     性别
    /// </summary>
    public int Sex { get; set; }

    /// <summary>
    ///     手机号
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    ///     邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    ///     邮箱是否验证, 0否, 1是
    /// </summary>
    public int EmailVerified { get; set; }

    /// <summary>
    ///     真实姓名
    /// </summary>
    public string RealName { get; set; }

    /// <summary>
    ///     身份证号
    /// </summary>
    public string IdCard { get; set; }

    /// <summary>
    ///     出生日期
    /// </summary>
    public DateTime? Birthday { get; set; }

    /// <summary>
    ///     个人简介
    /// </summary>
    public string Introduction { get; set; }

    /// <summary>
    ///     机构id
    /// </summary>
    public long? OrgId { get; set; }

    /// <summary>
    ///     机构名称
    /// </summary>
    public string OrgName { get; set; }

    /// <summary>
    ///     角色集合
    /// </summary>
    public IEnumerable<UserAuthRoleDto> Roles { get; set; }

    /// <summary>
    ///     权限集合
    /// </summary>
    public IEnumerable<UserAuthMenuDto> Authorities { get; set; }
}

/// <summary>
///     ；角色模型
/// </summary>
public class UserAuthRoleDto
{
    /// <summary>
    ///     角色id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     角色名称
    /// </summary>
    public string RoleName { get; set; }

    /// <summary>
    ///     角色标识
    /// </summary>
    public string RoleCode { get; set; }
}

/// <summary>
///     菜单Dto模型
/// </summary>
public class UserAuthMenuDto
{
    /// <summary>
    ///     菜单id
    /// </summary>
    public long MenuId { get; set; }

    /// <summary>
    ///     上级id, 0是顶级
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    ///     菜单名称
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     菜单路由地址
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    ///     菜单组件地址, 目录可为空
    /// </summary>
    public string Component { get; set; }

    /// <summary>
    ///     类型, 0菜单, 1按钮
    /// </summary>
    public int MenuType { get; set; }

    /// <summary>
    ///     排序号
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     权限标识
    /// </summary>
    public string Authority { get; set; }

    /// <summary>
    ///     菜单图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    ///     是否隐藏, 0否, 1是(仅注册路由不显示在左侧菜单)
    /// </summary>
    public int Hide { get; set; }

    /// <summary>
    ///     其它路由元信息
    /// </summary>
    public string Meta { get; set; }
}