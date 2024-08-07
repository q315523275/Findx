using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos.Menu;

/// <summary>
///     分页查询菜单入参
/// </summary>
public class MenuPageQueryDto: MenuQueryDto, IPager
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