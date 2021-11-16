using Findx.Data;
using FreeSql.DataAnnotations;
using System;

namespace Findx.Module.Admin.Models
{
	/// <summary>
	/// 系统用户数据范围表
	/// </summary>
	[Table(Name = "sys_notice_user")]
    public class SysNoticeUserInfo : EntityBase<long>, IResponse, IRequest
	{
		/// <summary>
		/// 主键
		/// </summary>
		[Column(Name = "id", IsPrimary = true)]
		public override long Id { get; set; }

		/// <summary>
		/// 通知公告id
		/// </summary>
		[Column(Name = "notice_id")]
		public long NoticeId { get; set; }

		/// <summary>
		/// 阅读时间
		/// </summary>
		[Column(Name = "read_time", DbType = "datetime")]
		public DateTime? ReadTime { get; set; }

		/// <summary>
		/// 状态（字典 0未读 1已读）
		/// </summary>
		[Column(Name = "status", DbType = "tinyint(4)")]
		public int Status { get; set; }

		/// <summary>
		/// 用户id
		/// </summary>
		[Column(Name = "user_id")]
		public long UserId { get; set; }
	}
}
