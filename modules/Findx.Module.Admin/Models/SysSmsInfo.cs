using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 短信信息发送表
    /// </summary>
    public partial class SysSms
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 回执id，可根据该id查询具体的发送状态
        /// </summary>
        public string BizId { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long? CreateUser { get; set; }

        /// <summary>
        /// 失效时间
        /// </summary>
        public DateTime? InvalidTime { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string PhoneNumbers { get; set; } = string.Empty;

        /// <summary>
        /// 来源（字典 1 app， 2 pc， 3 其他）
        /// </summary>
        public sbyte Source { get; set; }

        /// <summary>
        /// 发送状态（字典 0 未发送，1 发送成功，2 发送失败，3 失效）
        /// </summary>
        public sbyte Status { get; set; }

        /// <summary>
        /// 短信模板ID
        /// </summary>
        public string TemplateCode { get; set; } = string.Empty;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public long? UpdateUser { get; set; }

        /// <summary>
        /// 短信验证码
        /// </summary>
        public string ValidateCode { get; set; } = string.Empty;

    }

}
