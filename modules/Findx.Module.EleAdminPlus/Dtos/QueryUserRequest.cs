using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos;

/// <summary>
///     查询用户入参
/// </summary>
public class QueryUserRequest : PageBase
{
    /// <summary>
    ///     账号
    /// </summary>
    public string UserName { set; get; }

    /// <summary>
    ///     用户名
    /// </summary>
    public string Nickname { set; get; }

    /// <summary>
    ///     性别
    /// </summary>
    public int? Sex { set; get; }

    /// <summary>
    ///     状态
    /// </summary>
    public int? Status { set; get; }

    /// <summary>
    ///     机构编号
    /// </summary>
    public long? OrgId { set; get; }
}