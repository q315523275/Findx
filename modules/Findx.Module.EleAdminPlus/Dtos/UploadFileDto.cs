using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Findx.Module.EleAdminPlus.Dtos;

/// <summary>
/// 上传Dto
/// </summary>
public class UploadFileDto
{
    /// <summary>
    /// 文件信息
    /// </summary>
    [Required]
    // [FileExtensions]
    public IFormFile File { set; get; }
}