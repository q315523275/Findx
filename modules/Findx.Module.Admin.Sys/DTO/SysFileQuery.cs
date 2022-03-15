namespace Findx.Module.Admin.DTO
{
    /// <summary>
    /// 文件查询
    /// </summary>
    public class SysFileQuery : Findx.Data.PageBase
    {
        /// <summary>
        /// 文件存储位置（1:阿里云，2:腾讯云，3:minio，4:本地）
        /// </summary>
        public int FileLocation { get; set; }

        /// <summary>
        /// 文件仓库
        /// </summary>
        public string FileBucket { get; set; }

        /// <summary>
        /// 文件名称（上传时候的文件名）
        /// </summary>
        public string FileOriginName { get; set; }
    }
}
