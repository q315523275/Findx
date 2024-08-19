using Findx.Module.EleAdminPlus.Shared.Dtos;
using Findx.Module.EleAdminPlus.Shared.Enums;

namespace Findx.Module.EleAdminPlus.Shared.ServiceDefaults;

/// <summary>
///     工作上下文
/// </summary>
public interface IWorkContext
{
    /// <summary>
    ///     获取当前用户
    /// </summary>
    UserDto GetCurrentUser();

    /// <summary>
    ///     数据范围
    /// </summary>
    DataScope? DataScope { get; }
    
    /// <summary>
    ///     机构Id集合
    /// </summary>
    IEnumerable<long> OrgIds { get; }

    /// <summary>
    ///     设置数据范围
    /// </summary>
    /// <param name="dataScope"></param>
    void SetDataScope(DataScope dataScope);
    
    /// <summary>
    ///     设置机构范围
    /// </summary>
    /// <param name="ids"></param>
    void SetOrgIds(IEnumerable<long> ids);
}