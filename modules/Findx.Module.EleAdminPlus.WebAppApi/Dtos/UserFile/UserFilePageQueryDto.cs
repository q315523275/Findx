using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.UserFile;

/// <summary>
///     分页查询用户文件入参
/// </summary>
public class UserFilePageQueryDto : UserFileQueryDto, IPager
{
    /// <summary>
    ///     页码
    /// </summary>
    public int PageNo { get; set; }

    /// <summary>
    ///     每页数量
    /// </summary>
    public int PageSize { get; set; }
}
