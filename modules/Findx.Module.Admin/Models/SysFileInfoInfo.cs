using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 文件信息表
    /// </summary>
    public partial class SysFileInfo
    {

        /// <summary>
        /// 主键id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        public long? CreateUser { get; set; }

        /// <summary>
        /// 文件仓库
        /// </summary>
        public string FileBucket { get; set; } = string.Empty;

        /// <summary>
        /// 文件存储位置（1:阿里云，2:腾讯云，3:minio，4:本地）
        /// </summary>
        public sbyte FileLocation { get; set; }

        /// <summary>
        /// 存储到bucket的名称（文件唯一标识id）
        /// </summary>
        public string FileObjectName { get; set; } = string.Empty;

        /// <summary>
        /// 文件名称（上传时候的文件名）
        /// </summary>
        public string FileOriginName { get; set; } = string.Empty;

        /// <summary>
        /// 存储路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小信息，计算后的
        /// </summary>
        public string FileSizeInfo { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小kb
        /// </summary>
        public long? FileSizeKb { get; set; }

        /// <summary>
        /// 文件后缀
        /// </summary>
        public string FileSuffix { get; set; } = string.Empty;

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 修改用户
        /// </summary>
        public long? UpdateUser { get; set; }

    }

}
