using Findx.Data;
using Findx.Events;
using Findx.Extensions.AuditLogs.Events;
using Findx.Extensions.AuditLogs.Models;
using Findx.Mapping;
using Findx.Tracing;
using Microsoft.Extensions.Logging;

namespace Findx.Extensions.AuditLogs.ServiceDefaults;

/// <summary>
///     审计信息存储服务
/// </summary>
public class AuditStoreServices: IAuditStore
{
    private readonly ILogger<AuditStoreServices> _logger;
    private readonly ICorrelationIdProvider _correlationIdProvider;
    private readonly IApplicationContext _applicationContext;
    private readonly IKeyGenerator<long> _keyGenerator;
    private readonly IEventBus _eventBus;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="correlationIdProvider"></param>
    /// <param name="applicationContext"></param>
    /// <param name="keyGenerator"></param>
    /// <param name="logger"></param>
    /// <param name="eventBus"></param>
    public AuditStoreServices(ICorrelationIdProvider correlationIdProvider, IApplicationContext applicationContext, IKeyGenerator<long> keyGenerator, ILogger<AuditStoreServices> logger, IEventBus eventBus)
    {
        _correlationIdProvider = correlationIdProvider;
        _applicationContext = applicationContext;
        _keyGenerator = keyGenerator;
        _logger = logger;
        _eventBus = eventBus;
    }

    /// <summary>
    ///     保存
    /// </summary>
    /// <param name="operationEntry"></param>
    /// <param name="cancelToken"></param>
    public async Task SaveAsync(AuditOperationEntry operationEntry, CancellationToken cancelToken = default)
    {
        var auditLogInfo = BuildOperation(operationEntry);
        
        await _eventBus.PublishAsync(new AuditLogSaveEvent(auditLogInfo), cancelToken);
    }

    /// <summary>
    ///     构建操作日志
    /// </summary>
    /// <param name="operationEntry"></param>
    /// <returns></returns>
    private AuditLogInfo BuildOperation(AuditOperationEntry operationEntry)
    {
        var operation = operationEntry.MapTo<AuditLogInfo>();
        operation.Id = _keyGenerator.Create();
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
        
        foreach (var entityEntry in operation.EntityEntries)
        {
            entityEntry.Id = _keyGenerator.Create();
            entityEntry.AuditLogId = operation.Id;
            foreach (var propertyEntry in entityEntry.PropertyEntries)
            {
                propertyEntry.Id = _keyGenerator.Create();
                propertyEntry.EntityChangeId = entityEntry.Id;
            }
        }

        foreach (var sqlRawEntry in operation.SqlRawEntries)
        {
            sqlRawEntry.Id = _keyGenerator.Create();
            sqlRawEntry.AuditLogId = operation.Id;
            foreach (var parameterEntry in sqlRawEntry.DbParameters)
            {
                parameterEntry.Id = _keyGenerator.Create();
                parameterEntry.SqlRawId = sqlRawEntry.Id;
            }
        }
        
        return operation;
    }
}