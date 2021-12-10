using System;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    /// <summary>
    /// 登录用户信息DTO模型
    /// </summary>
    public class LoginUserDTO
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 管理员类型（0超级管理员 1非管理员）
        /// </summary>
        public int AdminType { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public long? Avatar { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 最后登陆IP
        /// </summary>
        public string LastLoginIp { get; set; }

        /// <summary>
        /// 最后登陆时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 最后登录地址
        /// </summary>
        public string LastLoginAddress { get; set; }

        /// <summary>
        /// 最后登录浏览器
        /// </summary>
        public string LastLoginBrowser { get; set; }

        /// <summary>
        /// 最后登录系统
        /// </summary>
        public string LastLoginOs { get; set; }


        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 性别(字典 1男 2女 3未知)
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 状态正常
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string Tel { get; set; }

        /// <summary>
        /// 角色范围
        /// </summary>
        public object Roles { get; set; }
        /// <summary>
        /// 系统范围
        /// </summary>
        public dynamic Apps { get; set; }
        /// <summary>
        /// 数据范围
        /// </summary>
        public object DataScopes { get; set; }
        /// <summary>
        /// 菜单范围
        /// </summary>
        public object Menus { get; set; }
        /// <summary>
        /// 权限范围
        /// </summary>
        public object Permissions { get; set; }
        /// <summary>
        /// 员工信息
        /// </summary>
        public object LoginEmpInfo { get; set; }
    }

    /// <summary>
    /// 登录用户员工信息
    /// </summary>
    public class LoginEmpDTO
    {
        /// <summary>
        /// 附属职位
        /// </summary>
        public object ExtOrgPos { set; get; }
        /// <summary>
        /// 工号
        /// </summary>
        public string JobNum { set; get; }
        /// <summary>
        /// 组织ID
        /// </summary>
        public long OrgId { set; get; }
        /// <summary>
        /// 组织名称
        /// </summary>
        public string OrgName { set; get; }
        /// <summary>
        /// 职位信息
        /// </summary>
        public object Positions { set; get; }
    }
    /// <summary>
    /// 登录用户菜单
    /// </summary>
    public class LoginUserMenuDTO
    {
        public long Id { set; get; }
        public long Pid { set; get; }
        public string Name { set; get; }
        public string Component { set; get; }
        public string Redirect { set; get; }
        public string Path { set; get; }
        public bool Hidden { set; get; }
        public LoginUserMenuMetaDTO Meta { set; get; }
        public int OpenType { set; get; }
    }
    /// <summary>
    /// 登录用户菜单元数据
    /// </summary>
    public class LoginUserMenuMetaDTO
    {
        public string Title { set; get; }
        public string Icon { set; get; }
        public bool Show { set; get; }
        public string Target { set; get; }
        public string Link { set; get; }
    }
}
