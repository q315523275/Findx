using System.Security.Claims;
using Findx.AspNetCore.Mvc;
using Findx.Extensions.WorkflowCore.Engine;
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
                RoleIds = ResolveRoleIds()
            };
        }
    }

    /// <summary>
    ///     解析当前用户的角色编码。
    ///     <para>JWT 把多角色放进单个 role 声明（逗号分隔），而认证组件不一定把它映射成
    ///     <c>ClaimTypes.Role</c>；只读 <c>CurrentUser.Roles</c> 会得到空集合，导致按角色指派的节点无人可办</para>
    /// </summary>
    private List<string> ResolveRoleIds()
    {
        var roles = WorkContext.CurrentUser?.Roles?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (roles is { Count: > 0 }) return roles;

        var raw = WorkContext.CurrentUser?.FindClaim("role")?.Value ?? WorkContext.CurrentUser?.FindClaim(ClaimTypes.Role)?.Value;
        if (string.IsNullOrWhiteSpace(raw)) return [];

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(x => x.Trim())
                  .Where(x => x.Length > 0)
                  .Distinct()
                  .ToList();
    }

    /// <summary>
    ///     把业务单据字段并入流程变量（发起审批时调用），已存在的 key 不覆盖，保证调用方显式传值优先
    /// </summary>
    /// <param name="variables">流程变量（就地修改）</param>
    /// <param name="businessType">业务类型标识</param>
    /// <param name="businessId">业务主键</param>
    /// <param name="cancellationToken">取消标记</param>
    protected async Task MergeBusinessVariablesAsync(Dictionary<string, object> variables, string businessType, long businessId, CancellationToken cancellationToken = default)
    {
        if (variables == null || string.IsNullOrWhiteSpace(businessType) || businessId <= 0) return;

        var adapter = GetRequiredService<FlwBusinessAdapterRegistry>().Get(businessType);
        if (adapter == null) return;

        var fields = await adapter.GetVariablesAsync(businessId, cancellationToken);
        if (fields == null) return;

        foreach (var item in fields)
        {
            if (string.IsNullOrWhiteSpace(item.Key) || variables.ContainsKey(item.Key)) continue;
            variables[item.Key] = item.Value;
        }
    }
}
