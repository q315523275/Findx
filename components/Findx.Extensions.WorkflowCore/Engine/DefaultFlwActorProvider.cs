using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Enums;
using Findx.Extensions.WorkflowCore.Models;
using Microsoft.Extensions.Logging;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     默认办理人解析：user 直配、creator 取发起人、role/dept 按角色/部门匹配
///     <para>superior（发起人上级）依赖组织架构数据，默认实现跳过并告警，由宿主实现 <see cref="IFlwActorProvider"/> 覆盖。</para>
/// </summary>
public class DefaultFlwActorProvider: IFlwActorProvider
{
    private readonly ILogger<DefaultFlwActorProvider> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    public DefaultFlwActorProvider(ILogger<DefaultFlwActorProvider> logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<List<FlwTaskActorInfo>> ResolveAsync(FlwActorResolveContext context, CancellationToken cancellationToken = default)
    {
        var result = new List<FlwTaskActorInfo>();
        if (context?.Node?.Props?.Assignee == null) return Task.FromResult(result);

        foreach (var actor in context.Node.Props.Assignee)
        {
            var type = actor.Type?.Trim().ToLowerInvariant();

            if (type == "creator")
            {
                result.Add(new FlwTaskActorInfo
                {
                    ActorId = context.Instance?.CreatorId?.ToString(),
                    ActorName = context.Instance?.Creator,
                    ActorType = FlwActorType.User,
                    Weight = 1
                });
                continue;
            }

            if (type == "superior")
            {
                _logger?.LogWarning("节点[{Node}]的 superior 办理人需要组织架构数据，默认实现已跳过，请实现 IFlwActorProvider 提供取人逻辑", context.Node.NodeName);
                continue;
            }

            if (string.IsNullOrWhiteSpace(actor.Id)) continue;

            var actorType = type switch
            {
                "role" => FlwActorType.Role,
                "dept" or "department" => FlwActorType.Department,
                _ => FlwActorType.User
            };

            result.Add(new FlwTaskActorInfo
            {
                ActorId = actor.Id,
                ActorName = actor.Name,
                ActorType = actorType,
                Weight = actor.Weight <= 0 ? 1 : actor.Weight
            });
        }

        return Task.FromResult(result
            .GroupBy(x => $"{x.ActorType}:{x.ActorId}")
            .Select(x => x.First())
            .ToList());
    }
}
