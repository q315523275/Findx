using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.File;

/// <summary>
/// 上传Dto
/// </summary>
public class UploadFileDto
{
    /// <summary>
    /// 文件信息
    /// </summary>
    [Required]
    public IFormFile File { set; get; }
    
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