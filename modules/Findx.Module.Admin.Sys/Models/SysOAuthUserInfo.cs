using Findx.Data;
using FreeSql.DataAnnotations;
using System;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 第三方认证用户信息表
    /// </summary>
    [Table(Name = "sys_oauth_user")]
    public class SysOauthUserInfo : EntityBase<long>, ICreateUser<long>, IUpdateUser<long>, IResponse, IRequest
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column(Name = "id", IsPrimary = true)]
        public override long Id { get; set; }

        /// <summary>
        /// 用户授权的token
        /// </summary>
        [Column(Name = "access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [Column(Name = "avatar", DbType = "varchar(500)")]
        public string Avatar { get; set; }

        /// <summary>
        /// 用户网址
        /// </summary>
        [Column(Name = "blog")]
        public string Blog { get; set; }

        /// <summary>
        /// 所在公司
        /// </summary>
        [Column(Name = "company")]
        public string Company { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "create_time", DbType = "datetime")]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        [Column(Name = "create_user")]
        public long? CreateUser { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [Column(Name = "email")]
        public string Email { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [Column(Name = "gender", DbType = "varchar(50)")]
        public string Gender { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        [Column(Name = "location")]
        public string Location { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [Column(Name = "nick_name")]
        public string NickName { get; set; }

        /// <summary>
        /// 用户备注（各平台中的用户个人介绍）
        /// </summary>
        [Column(Name = "remark")]
        public string Remark { get; set; }

        /// <summary>
        /// 用户来源
        /// </summary>
        [Column(Name = "source")]
        public string Source { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column(Name = "update_time", DbType = "datetime")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新用户
        /// </summary>
        [Column(Name = "update_user")]
        public long? UpdateUser { get; set; }

        /// <summary>
        /// 第三方平台的用户唯一id
        /// </summary>
        [Column(Name = "uuid")]
        public string Uuid { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Init()
        {
            Id = Findx.Utils.SnowflakeId.Default().NextId();
        }
    }
}
