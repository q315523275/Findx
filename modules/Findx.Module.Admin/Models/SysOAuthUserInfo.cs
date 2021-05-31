using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 第三方认证用户信息表
    /// </summary>
    public partial class SysOAuthUser
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 用户授权的token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; } = string.Empty;

        /// <summary>
        /// 用户网址
        /// </summary>
        public string Blog { get; set; } = string.Empty;

        /// <summary>
        /// 所在公司
        /// </summary>
        public string Company { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        public long? CreateUser { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 性别
        /// </summary>
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// 位置
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; } = string.Empty;

        /// <summary>
        /// 用户备注（各平台中的用户个人介绍）
        /// </summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 用户来源
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新用户
        /// </summary>
        public long? UpdateUser { get; set; }

        /// <summary>
        /// 第三方平台的用户唯一id
        /// </summary>
        public string Uuid { get; set; } = string.Empty;

    }

}
