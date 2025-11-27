using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.User;

/// <summary>
///     分页查询用户入参
/// </summary>
public partial class UserPageQueryDto: UserQueryDto, IPager
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