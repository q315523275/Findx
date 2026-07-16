using System.Security.Principal;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Module.EleAdminPlus.Shared.Enums;
using Findx.Module.EleAdminPlus.Shared.ServiceDefaults;
using Findx.Module.EleAdminPlus.Shared.Vos.Context;
using Findx.Security;

namespace Findx.Module.EleAdminPlus.ServiceDefaults;

/// <summary>
///     工作上下文
/// </summary>
public class WorkContext: IWorkContext, IScopeDependency
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="principal"></param>
    public WorkContext(ICurrentUser currentUser, IPrincipal principal)
    {
        CurrentUser = currentUser;
        Principal = principal;
    }

    /// <summary>
    ///     获取当前用户信息
    /// </summary>
    /// <returns></returns>
    public UserContextSimplifyVo GetCurrentUser()
    {
        if (CurrentUser is { IsAuthenticated: true })
        {
            return new UserContextSimplifyVo
            {
                UserId = CurrentUser.UserId.CastTo<long>(),
                Username = CurrentUser.UserName,
                Nickname = CurrentUser.Nickname, 
                OrgId = CurrentUser.FindClaim(Shared.Const.Default.OrgIdKey)?.Value.CastTo<long>(),
                OrgName = CurrentUser.FindClaim(Shared.Const.Default.OrgNameKey)?.Value,
                TenantId = CurrentUser.TenantId
            };
        }
        return null;
    }

    /// <summary>
    ///     用户上下文
    /// </summary>
    public UserContextSimplifyVo ContextUser 
    {
        get
        {
            if (CurrentUser is { IsAuthenticated: true })
            {
                return new UserContextSimplifyVo
                {
                    UserId = CurrentUser.UserId.CastTo<long>(),
                    Username = CurrentUser.UserName, 
                    Nickname = CurrentUser.Nickname,
                    OrgId = CurrentUser.FindClaim(Shared.Const.Default.OrgIdKey)?.Value.CastTo<long>(),
                    OrgName = CurrentUser.FindClaim(Shared.Const.Default.OrgNameKey)?.Value,
                    TenantId = CurrentUser.TenantId
                };
            }
            return null;
        }
    }

    /// <summary>
    ///     用户接口
    /// </summary>
    public ICurrentUser CurrentUser { get; }
    
    /// <summary>
    ///     身份信息
    /// </summary>
    public IPrincipal Principal { get; }

    /// <summary>
    ///     数据范围
    /// </summary>
    public DataScope? DataScope { get; private set; }

    /// <summary>
    ///     机构集合
    /// </summary>
    public List<long> OrgIds { get; private set; } = [];

    /// <summary>
    ///     设置数据范围
    /// </summary>
    /// <param name="dataScope"></param>
    public void SetDataScope(DataScope dataScope)
    {
        DataScope = dataScope;
    }

    /// <summary>
    ///     设置机构Id集合
    /// </summary>
    /// <param name="ids"></param>
    public void SetOrgIds(List<long> ids)
    {
        OrgIds = ids;
    }
}