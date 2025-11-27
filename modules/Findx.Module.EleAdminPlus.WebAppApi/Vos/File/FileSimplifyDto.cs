using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Vos.File;

/// <summary>
///     文件简化信息Vo
/// </summary>
public class FileSimplifyDto: IResponse
{
    /// <summary>
    ///     文件id
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    ///     文件名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     文件路径
    /// </summary>
    public string Path { get; set; }
    
    /// <summary>
    ///     文件大小(kb)
    /// </summary>
    public long Length { get; set; }
    
    /// <summary>
    ///     文件网址
    /// </summary>
    public string Url { get; set; }
    
    /// <summary>
    ///     缩略图网址
    /// </summary>
    public string Thumbnail { get; set; }
    
    /// <summary>
    ///     下载地址
    /// </summary>
    public string DownloadUrl { get; set; }
    
    /// <summary>
    ///     内容类型
    /// </summary>
    public string ContentType { get; set; }
    
    /// <summary>
    ///     创建人
    /// </summary>
    public string Creator { get; set; }
    
    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }
}