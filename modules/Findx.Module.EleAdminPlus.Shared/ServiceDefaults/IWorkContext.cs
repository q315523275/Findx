using System.Security.Principal;
using Findx.Module.EleAdminPlus.Shared.Enums;
using Findx.Module.EleAdminPlus.Shared.Vos;
using Findx.Module.EleAdminPlus.Shared.Vos.Context;
using Findx.Security;

namespace Findx.Module.EleAdminPlus.Shared.ServiceDefaults;

/// <summary>
///     工作上下文
/// </summary>
public interface IWorkContext
{
    /// <summary>
    ///     获取当前用户上下文信息
    /// </summary>
    [Obsolete]
    UserContextSimplifyVo GetCurrentUser();
    
    /// <summary>
    ///     用户上下文
    /// </summary>
    UserContextSimplifyVo ContextUser { get; }
    
    /// <summary>
    ///     用户接口
    /// </summary>
    ICurrentUser CurrentUser { get; }
    
    /// <summary>
    ///     身份信息
    /// </summary>
    IPrincipal Principal { get; }

    /// <summary>
    ///     数据范围
    /// </summary>
    DataScope? DataScope { get; }
    
    /// <summary>
    ///     机构Id集合
    /// </summary>
    List<long> OrgIds { get; }

    /// <summary>
    ///     设置数据范围
    /// </summary>
    /// <param name="dataScope"></param>
    void SetDataScope(DataScope dataScope);
    
    /// <summary>
    ///     设置机构范围
    /// </summary>
    /// <param name="ids"></param>
    void SetOrgIds(List<long> ids);
}