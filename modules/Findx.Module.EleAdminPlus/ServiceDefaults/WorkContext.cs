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
    private readonly ICurrentUser _currentUser;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="currentUser"></param>
    public WorkContext(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    ///     获取当前用户信息
    /// </summary>
    /// <returns></returns>
    public UserContextSimplifyVo GetCurrentUser()
    {
        if (_currentUser is { IsAuthenticated: true })
        {
            return new UserContextSimplifyVo
            {
                UserId = _currentUser.UserId.CastTo<long>(),
                Nickname  = _currentUser.UserName, 
                OrgId = _currentUser.FindClaim(Shared.Const.Default.OrgIdKey)?.Value.CastTo<long>(),
                OrgName = _currentUser.FindClaim(Shared.Const.Default.OrgNameKey)?.Value,
                TenantId = _currentUser.TenantId
            };
        }
        return null;
    }

    /// <summary>
    ///     数据范围
    /// </summary>
    public DataScope? DataScope { get; private set; }

    /// <summary>
    ///     机构集合
    /// </summary>
    public IEnumerable<long> OrgIds { get; private set; } = new List<long>();

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
    public void SetOrgIds(IEnumerable<long> ids)
    {
        OrgIds = ids.Distinct();
    }
}