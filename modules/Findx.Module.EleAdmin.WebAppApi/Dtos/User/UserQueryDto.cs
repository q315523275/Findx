using Findx.Data;

namespace Findx.Module.EleAdmin.Dtos.User;

/// <summary>
///     查询用户入参
/// </summary>
public class UserQueryDto : PageBase
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
    public Guid? OrgId { set; get; }

    /// <summary>
    ///     机构编号集合
    /// </summary>
    public List<Guid> OrgIds { set; get; } = [];

    /// <summary>
    ///     员工编号
    /// </summary>
    public string EmployeeNumber { get; set; }
}