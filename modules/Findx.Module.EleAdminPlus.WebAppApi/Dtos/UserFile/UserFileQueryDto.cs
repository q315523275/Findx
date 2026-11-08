using Findx.Expressions;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.UserFile;

/// <summary>
///     用户文件查询参数Dto
/// </summary>
public class UserFileQueryDto : SortCondition
{
    /// <summary>
    ///     主键id
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public long? Id { set; get; }

    /// <summary>
    ///     用户id
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public long? UserId { set; get; }

    /// <summary>
    ///     文件名称
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Name { set; get; }

    /// <summary>
    ///     上级id
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public long? ParentId { set; get; }
}
