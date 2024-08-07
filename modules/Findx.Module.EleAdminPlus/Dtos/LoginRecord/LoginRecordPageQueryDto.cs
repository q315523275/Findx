using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos.LoginRecord;

/// <summary>
///     查询登录日志参数
/// </summary>
public class LoginRecordPageQueryDto : LoginRecordQueryDto, IPager
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