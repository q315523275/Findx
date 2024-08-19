using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Linq;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.Role;

/// <summary>
///     查询角色入参
/// </summary>
public class RoleQueryDto : SortCondition
{
    /// <summary>
    ///     名称
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Name { set; get; }

    /// <summary>
    ///     编号
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Code { set; get; }

    /// <summary>
    ///     备注
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Comments { set; get; }
}