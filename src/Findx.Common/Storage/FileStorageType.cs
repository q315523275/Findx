using System.ComponentModel;

namespace Findx.Storage
{
	/// <summary>
	/// 文件存储器类型
	/// </summary>
    public enum FileStorageType
	{
		/// <summary>
		/// 目录文件存储
		/// </summary>
		[Description("目录文件存储")]
		Folder,

		/// <summary>
		/// Minio分布式存储
		/// </summary>
		[Description("Minio分布式存储")]
		Minio,

		/// <summary>
		/// 阿里云OSS存储
		/// </summary>
		[Description("阿里云OSS存储")]
		Aliyun,

		/// <summary>
		/// 腾讯OSS存储
		/// </summary>
		[Description("腾讯OSS存储")]
		Tencent,
	}
}

