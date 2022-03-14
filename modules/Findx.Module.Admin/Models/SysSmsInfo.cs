using Findx.Data;
using FreeSql.DataAnnotations;
using System;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 短信信息发送表
    /// </summary>
    [Table(Name = "sys_sms")]
    public class SysSmsInfo : EntityBase<long>, ICreateUser<long>, IUpdateUser<long>, IResponse, IRequest
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column(Name = "id", IsPrimary = true)]
        public override long Id { get; set; }

        /// <summary>
        /// 回执id，可根据该id查询具体的发送状态
        /// </summary>
        [Column(Name = "biz_id")]
        public string BizId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "create_time", DbType = "datetime")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Column(Name = "create_user")]
        public long? CreateUser { get; set; }

        /// <summary>
        /// 失效时间
        /// </summary>
        [Column(Name = "invalid_time", DbType = "datetime")]
        public DateTime? InvalidTime { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [Column(Name = "phone_numbers", DbType = "varchar(200)")]
        public string PhoneNumbers { get; set; }

        /// <summary>
        /// 来源（字典 1 app， 2 pc， 3 其他）
        /// </summary>
        [Column(Name = "source", DbType = "tinyint(4)")]
        public int Source { get; set; }

        /// <summary>
        /// 发送状态（字典 0 未发送，1 发送成功，2 发送失败，3 失效）
        /// </summary>
        [Column(Name = "status", DbType = "tinyint(4)")]
        public int Status { get; set; }

        /// <summary>
        /// 短信模板ID
        /// </summary>
        [Column(Name = "template_code")]
        public string TemplateCode { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column(Name = "update_time", DbType = "datetime")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [Column(Name = "update_user")]
        public long? UpdateUser { get; set; }

        /// <summary>
        /// 短信验证码
        /// </summary>
        [Column(Name = "validate_code")]
        public string ValidateCode { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Init()
        {
            Id = Findx.Utils.SnowflakeId.Default().NextId();
        }
    }
}
