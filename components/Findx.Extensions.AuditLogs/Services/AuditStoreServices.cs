using Findx.Data;
using Findx.Extensions.AuditLogs.Models;
using Findx.Mapping;
using Findx.Tracing;

namespace Findx.Extensions.AuditLogs.Services;

/// <summary>
///     审计信息存储服务
/// </summary>
public class AuditStoreServices: IAuditStore
{
    private readonly ICorrelationIdProvider _correlationIdProvider;
    private readonly IApplicationContext _applicationContext;
    private readonly IKeyGenerator<Guid> _keyGenerator;
    
    public AuditStoreServices(ICorrelationIdProvider correlationIdProvider, IApplicationContext applicationContext, IKeyGenerator<Guid> keyGenerator)
    {
        _correlationIdProvider = correlationIdProvider;
        _applicationContext = applicationContext;
        _keyGenerator = keyGenerator;
    }

    public Task SaveAsync(AuditOperationEntry operationEntry, CancellationToken cancelToken = default)
    {

        return Task.CompletedTask;
    }
    
    
    private AuditLogInfo BuildOperation(AuditOperationEntry operationEntry)
    {
        var operation = operationEntry.MapTo<AuditLogInfo>();
        operation.CorrelationId = _correlationIdProvider.Get();
        operation.ApplicationName = _applicationContext.ApplicationName;
        operation.BrowserInfo = operationEntry.UserAgent;
        operation.ExecutionDuration = (int)operationEntry.EndedTime.Subtract(operationEntry.ExecutionTime).TotalMilliseconds;
        if (operationEntry.Exception != null)
        {
            operation.Exceptions = operationEntry.Exception.FormatMessage();
        }
        if (operationEntry.ExtraObject.TryGetValue("http.url", out var url))
        {
            operation.Url = url;
        }
        if (operationEntry.ExtraObject.TryGetValue("http.method", out var httpMethod))
        {
            operation.HttpMethod = httpMethod;
        }
        if (operationEntry.ExtraObject.TryGetValue("http.status_code", out var httpStatusCode))
        {
            operation.HttpStatusCode = httpStatusCode.CastTo<int>();
        }
        
        foreach (var entityEntry in operationEntry.EntityEntries)
        {
            var entity = entityEntry.MapTo<AuditEntityInfo>();
            operation.AuditEntities.Add(entity);
            foreach (var propertyEntry in entityEntry.PropertyEntries)
            {
                var property = propertyEntry.MapTo<AuditEntityPropertyInfo>();
                entity.Properties.Add(property);
            }
        }
        return operation;
    }
}