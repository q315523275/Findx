using Findx.Data;
using FreeSql.DataAnnotations;
using System;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 系统访问日志表
    /// </summary>
    [Table(Name = "sys_vis_log")]
    public class SysVisLogInfo : EntityBase<long>, IResponse, IRequest
	{
		/// <summary>
		/// 主键
		/// </summary>
		[Column(Name = "id", IsPrimary = true)]
		public override long Id { get; set; }

		/// <summary>
		/// 访问账号
		/// </summary>
		[Column(Name = "account", DbType = "varchar(50)")]
		public string Account { get; set; }

		/// <summary>
		/// 浏览器
		/// </summary>
		[Column(Name = "browser")]
		public string Browser { get; set; }

		/// <summary>
		/// ip
		/// </summary>
		[Column(Name = "ip")]
		public string Ip { get; set; }

		/// <summary>
		/// 地址
		/// </summary>
		[Column(Name = "location")]
		public string Location { get; set; }

		/// <summary>
		/// 具体消息
		/// </summary>
		[Column(Name = "message", DbType = "text")]
		public string Message { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		[Column(Name = "name", DbType = "varchar(50)")]
		public string Name { get; set; }

		/// <summary>
		/// 操作系统
		/// </summary>
		[Column(Name = "os")]
		public string Os { get; set; }

		/// <summary>
		/// 是否执行成功（Y-是，N-否）
		/// </summary>
		[Column(Name = "success", DbType = "char(1)")]
		public string Success { get; set; }

		/// <summary>
		/// 访问时间
		/// </summary>
		[Column(Name = "vis_time", DbType = "datetime")]
		public DateTime? VisTime { get; set; }

		/// <summary>
		/// 操作类型（字典 1登入 2登出）
		/// </summary>
		[Column(Name = "vis_type", DbType = "tinyint(4)")]
		public sbyte VisType { get; set; }
	}
}
