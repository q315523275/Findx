using Findx.Data;
using FreeSql.DataAnnotations;
using System;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 文件信息表
    /// </summary>
    [Table(Name = "sys_file_info")]
    public class SysFileInfo : EntityBase<long>, ICreateUser<long>, IUpdateUser<long>, IResponse, IRequest
	{
		/// <summary>
		/// 主键id
		/// </summary>
		[Column(Name = "id", IsPrimary = true)]
		public override long Id { get; set; }

		/// <summary>
		/// 文件仓库
		/// </summary>
		[Column(Name = "file_bucket", DbType = "varchar(1000)")]
		public string FileBucket { get; set; }

		/// <summary>
		/// 文件存储位置（1:阿里云，2:腾讯云，3:minio，4:本地）
		/// </summary>
		[Column(Name = "file_location", DbType = "tinyint(4)")]
		public int FileLocation { get; set; }

		/// <summary>
		/// 存储到bucket的名称（文件唯一标识id）
		/// </summary>
		[Column(Name = "file_object_name", DbType = "varchar(100)")]
		public string FileObjectName { get; set; }

		/// <summary>
		/// 文件名称（上传时候的文件名）
		/// </summary>
		[Column(Name = "file_origin_name", DbType = "varchar(100)")]
		public string FileOriginName { get; set; }

		/// <summary>
		/// 存储路径
		/// </summary>
		[Column(Name = "file_path", DbType = "varchar(1000)")]
		public string FilePath { get; set; }

		/// <summary>
		/// 文件大小信息，计算后的
		/// </summary>
		[Column(Name = "file_size_info", DbType = "varchar(100)")]
		public string FileSizeInfo { get; set; }

		/// <summary>
		/// 文件大小kb
		/// </summary>
		[Column(Name = "file_size_kb")]
		public long? FileSizeKb { get; set; }

		/// <summary>
		/// 文件后缀
		/// </summary>
		[Column(Name = "file_suffix", DbType = "varchar(50)")]
		public string FileSuffix { get; set; }

		/// <summary>
		/// 修改时间
		/// </summary>
		[Column(Name = "update_time", DbType = "datetime")]
		public DateTime? UpdateTime { get; set; }

		/// <summary>
		/// 修改用户
		/// </summary>
		[Column(Name = "update_user")]
		public long? UpdateUser { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		[Column(Name = "create_time", DbType = "datetime")]
		public DateTime CreateTime { get; set; }

		/// <summary>
		/// 创建用户
		/// </summary>
		[Column(Name = "create_user")]
		public long? CreateUser { get; set; }
	}
}
