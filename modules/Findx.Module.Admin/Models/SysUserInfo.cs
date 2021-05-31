using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 系统用户表
    /// </summary>
    public partial class SysUser
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 管理员类型（0超级管理员 1非管理员）
        /// </summary>
        public sbyte AdminType { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public long? Avatar { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long? CreateUser { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 最后登陆IP
        /// </summary>
        public string LastLoginIp { get; set; } = string.Empty;

        /// <summary>
        /// 最后登陆时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 手机
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// 性别(字典 1男 2女 3未知)
        /// </summary>
        public sbyte Sex { get; set; }

        /// <summary>
        /// 状态（字典 0正常 1冻结 2删除）
        /// </summary>
        public sbyte Status { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string Tel { get; set; } = string.Empty;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public long? UpdateUser { get; set; }

    }

}
