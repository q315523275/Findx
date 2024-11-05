using Findx.Module.EleAdmin.Shared.Dtos;
using Findx.Module.EleAdmin.Shared.Enum;

namespace Findx.Module.EleAdmin.Shared.ServiceDefaults;

/// <summary>
///     工作上下文
/// </summary>
public interface IWorkContext
{
    /// <summary>
    ///     获取当前用户
    /// </summary>
    UserSimplifyDto GetCurrentUser();

    /// <summary>
    ///     数据范围
    /// </summary>
    DataScope? DataScope { get; }
    
    /// <summary>
    ///     机构Id集合
    /// </summary>
    IEnumerable<Guid> OrgIds { get; }

    /// <summary>
    ///     设置数据范围
    /// </summary>
    /// <param name="dataScope"></param>
    void SetDataScope(DataScope dataScope);
    
    /// <summary>
    ///     设置机构范围
    /// </summary>
    /// <param name="ids"></param>
    void SetOrgIds(IEnumerable<Guid> ids);
}