using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos.File;

/// <summary>
///     分页文件查询参数Dto
/// </summary>
public class FilePageQueryDto: FileQueryDto, IPager
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