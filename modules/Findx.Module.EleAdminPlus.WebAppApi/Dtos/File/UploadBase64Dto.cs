using System.ComponentModel.DataAnnotations;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.File;

/// <summary>
///     上传Base64 Dto
/// </summary>
public class UploadBase64Dto
{
    /// <summary>
    ///     Base64 数据(兼容 data:image/png;base64,xxx 形式)
    /// </summary>
    [Required]
    public string Base64 { set; get; }

    /// <summary>
    ///     文件名称
    /// </summary>
    public string FileName { set; get; }

    /// <summary>
    ///     文件类型
    /// </summary>
    public int FileType { get; set; }

    /// <summary>
    ///     文件类型Id
    /// </summary>
    public long? FileTypeId { get; set; }

    /// <summary>
    ///     文件类型Id名称
    /// </summary>
    public string FileTypeName { get; set; }
}
