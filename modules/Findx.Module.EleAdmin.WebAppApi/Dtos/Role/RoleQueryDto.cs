using Findx.Data;
using Findx.Expressions;

namespace Findx.Module.EleAdmin.Dtos.Role;

/// <summary>
///     查询角色入参
/// </summary>
public class RoleQueryDto : PageBase
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

    /// <summary>
    ///     应用名称
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public string ApplicationCode { get; set; }
}