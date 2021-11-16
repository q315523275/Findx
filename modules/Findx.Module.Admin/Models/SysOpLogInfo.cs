using Findx.Data;
using FreeSql.DataAnnotations;
using System;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 系统操作日志表
    /// </summary>
    [Table(Name = "sys_op_log")]
    public class SysOpLogInfo: EntityBase<long>, IResponse, IRequest
	{
		/// <summary>
		/// 主键
		/// </summary>
		[Column(Name = "id", IsPrimary = true)]
		public override long Id { get; set; }

		/// <summary>
		/// 操作账号
		/// </summary>
		[Column(Name = "account", DbType = "varchar(50)")]
		public string Account { get; set; }

		/// <summary>
		/// 浏览器
		/// </summary>
		[Column(Name = "browser")]
		public string Browser { get; set; }

		/// <summary>
		/// 类名称
		/// </summary>
		[Column(Name = "class_name", DbType = "varchar(500)")]
		public string ClassName { get; set; }

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
		/// 方法名称
		/// </summary>
		[Column(Name = "method_name", DbType = "varchar(500)")]
		public string MethodName { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		[Column(Name = "name", DbType = "varchar(50)")]
		public string Name { get; set; }

		/// <summary>
		/// 操作时间
		/// </summary>
		[Column(Name = "op_time", DbType = "datetime")]
		public DateTime? OpTime { get; set; }

		/// <summary>
		/// 操作类型
		/// </summary>
		[Column(Name = "op_type", DbType = "tinyint(4)")]
		public int? OpType { get; set; }

		/// <summary>
		/// 操作系统
		/// </summary>
		[Column(Name = "os")]
		public string Os { get; set; }

		/// <summary>
		/// 请求参数
		/// </summary>
		[Column(Name = "param", DbType = "text")]
		public string Param { get; set; }

		/// <summary>
		/// 请求方式（GET POST PUT DELETE)
		/// </summary>
		[Column(Name = "req_method")]
		public string ReqMethod { get; set; }

		/// <summary>
		/// 返回结果
		/// </summary>
		[Column(Name = "result", DbType = "longtext")]
		public string Result { get; set; }

		/// <summary>
		/// 是否执行成功（Y-是，N-否）
		/// </summary>
		[Column(Name = "success", DbType = "char(1)")]
		public string Success { get; set; }

		/// <summary>
		/// 请求地址
		/// </summary>
		[Column(Name = "url", DbType = "varchar(500)")]
		public string Url { get; set; }
	}
}
