using Findx.AspNetCore.Mvc;
using Findx.Extensions.WorkflowCore.Models;
using Findx.Module.EleAdminPlus.Shared.ServiceDefaults;

namespace Findx.Extensions.WorkflowCore.Controllers;

/// <summary>
///     工作流控制器基类
/// </summary>
public abstract class FlwControllerBase: AreaApiControllerBase
{
    /// <summary>
    ///     工作上下文
    /// </summary>
    protected readonly IWorkContext WorkContext;

    /// <summary>
    ///     Ctor
    /// </summary>
    protected FlwControllerBase(IWorkContext workContext)
    {
        WorkContext = workContext;
    }

    /// <summary>
    ///     当前操作人
    /// </summary>
    protected FlwCreator CurrentActor
    {
        get
        {
            var contextUser = WorkContext.ContextUser;
            return new FlwCreator
            {
                Id = contextUser?.UserId.ToString() ?? WorkContext.CurrentUser?.UserId,
                Name = contextUser?.Nickname ?? contextUser?.Username ?? WorkContext.CurrentUser?.UserName,
                DeptId = contextUser?.OrgId?.ToString(),
                RoleIds = WorkContext.CurrentUser?.Roles?.ToList()
            };
        }
    }
}
