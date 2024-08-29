using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Expressions;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.Org;

/// <summary>
///     查询组织入参
/// </summary>
public class OrgQueryDto : SortCondition
{
    /// <summary>
    ///     类型
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public string Type { set; get; }

    /// <summary>
    ///     名称
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Name { set; get; }
    
    /// <summary>
    ///     负责人
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public string Owner { set; get; }
}