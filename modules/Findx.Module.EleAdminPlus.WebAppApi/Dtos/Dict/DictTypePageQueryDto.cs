using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.Dict;

/// <summary>
///     分页查询字典入参
/// </summary>
public class DictTypePageQueryDto: DictTypeQueryDto, IPager
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