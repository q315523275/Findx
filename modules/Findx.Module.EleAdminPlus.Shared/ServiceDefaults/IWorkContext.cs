using Findx.Module.EleAdminPlus.Shared.Enums;
using Findx.Module.EleAdminPlus.Shared.Vos;
using Findx.Module.EleAdminPlus.Shared.Vos.Context;

namespace Findx.Module.EleAdminPlus.Shared.ServiceDefaults;

/// <summary>
///     工作上下文
/// </summary>
public interface IWorkContext
{
    /// <summary>
    ///     获取当前用户上下文信息
    /// </summary>
    UserContextSimplifyVo GetCurrentUser();

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