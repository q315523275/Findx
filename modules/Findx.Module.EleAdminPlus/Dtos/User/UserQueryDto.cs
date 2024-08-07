using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Linq;

namespace Findx.Module.EleAdminPlus.Dtos.User;

/// <summary>
///     查询用户入参
/// </summary>
public class UserQueryDto : SortCondition
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
    public long? OrgId { set; get; }
    
    /// <summary>
    ///     机构编号
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.In, Name = "OrgId")]
    public string OrgIds { set; get; }
}