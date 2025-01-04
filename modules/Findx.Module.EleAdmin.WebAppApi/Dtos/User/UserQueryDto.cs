using Findx.Data;
using Findx.Expressions;

namespace Findx.Module.EleAdmin.Dtos.User;

/// <summary>
///     查询用户入参
/// </summary>
public class UserQueryDto : PageBase
{
    /// <summary>
    ///     账号
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string UserName { set; get; }

    /// <summary>
    ///     用户名
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Nickname { set; get; }

    /// <summary>
    ///     性别
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public int? Sex { set; get; }

    /// <summary>
    ///     状态
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public int? Status { set; get; }

    /// <summary>
    ///     机构编号
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public Guid? OrgId { set; get; }
    
    /// <summary>
    ///     机构编号
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.In, Name = "OrgId")]
    public string OrgIds { set; get; }
    
    /// <summary>
    ///     员工编号
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public string EmployeeNumber { get; set; }
}