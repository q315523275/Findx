using System;
using System.ComponentModel.DataAnnotations;
namespace Findx.Module.Admin.DTO
{
	/// <summary>
    /// 发送邮件
    /// </summary>
	public class SendEmailRequest
	{
		/// <summary>
        /// 接收者
        /// </summary>
        [Required]
		[EmailAddress]
		public string To { set; get; }

		/// <summary>
		/// 标题
		/// </summary>
		[Required]
		public string Title { set; get; }

		/// <summary>
		/// 内容
		/// </summary>
		[Required]
		public string Content { set; get; }
	}
}

