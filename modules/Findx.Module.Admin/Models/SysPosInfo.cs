//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//     Website: http://www.freesql.net
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------
using Findx.Data;
using SqlSugar;
using System;
using FreeSql.DataAnnotations;
namespace Findx.Module.Admin.Models
{
	/// <summary>
	/// 系统职位表
	/// </summary>
	[Table(Name = "sys_pos")]
	public partial class SysPosInfo : EntityBase<long>, ICreateUser<long>, IUpdateUser<long>, ISort, IResponse, IRequest
	{

		/// <summary>
		/// 主键
		/// </summary>
		[Column(Name = "id", IsPrimary = true)]
		public override long Id { get; set; }

		/// <summary>
		/// 编码
		/// </summary>
		[Column(Name = "code", DbType = "varchar(50)")]
		public string Code { get; set; }

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
		/// 名称
		/// </summary>
		[Column(Name = "name", DbType = "varchar(100)")]
		public string Name { get; set; }

		/// <summary>
		/// 备注
		/// </summary>
		[Column(Name = "remark")]
		public string Remark { get; set; }

		/// <summary>
		/// 排序
		/// </summary>
		[Column(Name = "sort")]
		public int Sort { get; set; }

		/// <summary>
		/// 状态（字典 0正常 1停用 2删除）
		/// </summary>
		[Column(Name = "status", DbType = "tinyint(4)")]
		public int Status { get; set; }

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
		/// 初始化
		/// </summary>
		public override void Init()
		{
			Id = Findx.Utils.SnowflakeId.Default().NextId();
		}
	}
}
