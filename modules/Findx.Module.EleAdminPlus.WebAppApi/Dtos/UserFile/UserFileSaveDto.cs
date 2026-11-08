using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.UserFile;

/// <summary>
///     新增用户文件参数Dto
/// </summary>
public class UserFileSaveDto : IRequest
{
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
    public long? ParentId { get; set; }

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
}
