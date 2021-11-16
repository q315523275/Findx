using Findx.Data;
using FreeSql.DataAnnotations;
using System;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 通知表
    /// </summary>
    [Table(Name = "sys_notice")]
    public class SysNoticeInfo : EntityBase<long>, ICreateUser<long>, IUpdateUser<long>, IResponse, IRequest
	{
		/// <summary>
		/// 主键
		/// </summary>
		[Column(Name = "id", IsPrimary = true)]
		public override long Id { get; set; }

		/// <summary>
		/// 撤回时间
		/// </summary>
		[Column(Name = "cancel_time", DbType = "datetime")]
		public DateTime? CancelTime { get; set; }

		/// <summary>
		/// 内容
		/// </summary>
		[Column(Name = "content", DbType = "text")]
		public string Content { get; set; }

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
		/// 发布机构id
		/// </summary>
		[Column(Name = "public_org_id")]
		public long? PublicOrgId { get; set; }

		/// <summary>
		/// 发布机构名称
		/// </summary>
		[Column(Name = "public_org_name", DbType = "varchar(50)")]
		public string PublicOrgName { get; set; }

		/// <summary>
		/// 发布时间
		/// </summary>
		[Column(Name = "public_time", DbType = "datetime")]
		public DateTime? PublicTime { get; set; }

		/// <summary>
		/// 发布人id
		/// </summary>
		[Column(Name = "public_user_id")]
		public long PublicUserId { get; set; }

		/// <summary>
		/// 发布人姓名
		/// </summary>
		[Column(Name = "public_user_name", DbType = "varchar(100)")]
		public string PublicUserName { get; set; }

		/// <summary>
		/// 状态（字典 0草稿 1发布 2撤回 3删除）
		/// </summary>
		[Column(Name = "status", DbType = "tinyint(4)")]
		public int Status { get; set; }

		/// <summary>
		/// 标题
		/// </summary>
		[Column(Name = "title", DbType = "varchar(1000)")]
		public string Title { get; set; }

		/// <summary>
		/// 类型（字典 1通知 2公告）
		/// </summary>
		[Column(Name = "type", DbType = "tinyint(4)")]
		public int Type { get; set; }

		/// <summary>
		/// 修改时间
		/// </summary>
		[Column(Name = "update_time", DbType = "datetime")]
		public DateTime? UpdateTime { get; set; }

		/// <summary>
		/// 修改人
		/// </summary>
		[Column(Name = "update_user")]
		public long? UpdateUser { get; set; }
	}
}
