using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Module.EleAdmin.Shared.Const;
using Findx.Module.EleAdmin.Shared.Dtos;
using Findx.Module.EleAdmin.Shared.Enum;
using Findx.Security;

namespace Findx.Module.EleAdmin.Shared.ServiceDefaults;

/// <summary>
///     工作上下文
/// </summary>
public class DefaultWorkContext: IWorkContext, IScopeDependency
{
    private readonly ICurrentUser _currentUser;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="currentUser"></param>
    public DefaultWorkContext(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    ///     获取当前用户信息
    /// </summary>
    /// <returns></returns>
    public UserSimplifyDto GetCurrentUser()
    {
        if (_currentUser is { IsAuthenticated: true })
        {
            return new UserSimplifyDto
            {
                UserId = _currentUser.UserId.CastTo<Guid>(),
                Nickname  = _currentUser.UserName, 
                OrgId = _currentUser.FindClaim(Default.OrgIdKey)?.Value.CastTo<Guid>(),
                OrgName = _currentUser.FindClaim(Default.OrgNameKey)?.Value
            };
        }
        return default;
    }

    /// <summary>
    ///     数据范围
    /// </summary>
    public DataScope? DataScope { get; private set; }

    /// <summary>
    ///     机构集合
    /// </summary>
    public IEnumerable<Guid> OrgIds { get; private set; } = new List<Guid>();

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
    public void SetOrgIds(IEnumerable<Guid> ids)
    {
        OrgIds = ids.Distinct();
    }
}