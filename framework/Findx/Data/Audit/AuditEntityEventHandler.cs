using System.Threading.Tasks;
using Findx.Messaging;

namespace Findx.Data;

/// <summary>
///     实体审计事件处理器
/// </summary>
public class AuditEntityEventHandler: IApplicationEventHandler<AuditEntityEvent>
{
    private readonly ScopedDictionary _scopedDictionary;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="scopedDictionary"></param>
    public AuditEntityEventHandler(ScopedDictionary scopedDictionary)
    {
        _scopedDictionary = scopedDictionary;
    }

    /// <summary>
    ///     事件处理
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="cancellationToken"></param>
    public Task HandleAsync(AuditEntityEvent eventData, CancellationToken cancellationToken = default)
    {
        var operation = _scopedDictionary.AuditOperation;
        if (operation == null)
        {
            return Task.CompletedTask;
        }
        
        foreach (var auditEntity in eventData.AuditEntities)
        {
            operation.EntityEntries.Add(auditEntity);
        }

        return Task.CompletedTask;
    }
}