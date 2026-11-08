using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Vos.UserFile;

/// <summary>
///     用户文件简化信息Vo
/// </summary>
public class UserFileSimplifyVo : IResponse
{
    /// <summary>
    ///     主键id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     用户id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    ///     文件名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     是否是文件夹, 0否, 1是
    /// </summary>
    public int IsDirectory { get; set; }

    /// <summary>
    ///     上级id
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    ///     文件路径
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    ///     文件大小
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    ///     文件类型
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    ///     文件访问地址
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     文件缩略图访问地址
    /// </summary>
    public string Thumbnail { get; set; }

    /// <summary>
    ///     文件下载地址
    /// </summary>
    public string DownloadUrl { get; set; }

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }
}
