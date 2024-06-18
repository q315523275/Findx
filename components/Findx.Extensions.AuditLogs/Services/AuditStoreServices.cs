using Findx.Data;
using Findx.Extensions.AuditLogs.Models;
using Findx.Mapping;
using Findx.Tracing;
using Microsoft.Extensions.Logging;

namespace Findx.Extensions.AuditLogs.Services;

/// <summary>
///     审计信息存储服务
/// </summary>
public class AuditStoreServices: IAuditStore
{
    private readonly ILogger<AuditStoreServices> _logger;
    private readonly ICorrelationIdProvider _correlationIdProvider;
    private readonly IApplicationContext _applicationContext;
    private readonly IKeyGenerator<Guid> _keyGenerator;
    private readonly IRepository<AuditLogInfo> _auditLogRepo;
    private readonly IRepository<AuditLogActionInfo> _auditLogActionRepo;
    private readonly IRepository<AuditEntityInfo> _auditEntityRepo;
    private readonly IRepository<AuditEntityPropertyInfo> _auditEntityPropertyRepo;
    private readonly IRepository<AuditSqlRawInfo> _auditSqlRawRepo;
    private readonly IRepository<AuditSqlRawParameterInfo> _auditSqlRawParameterRepo;
    
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="correlationIdProvider"></param>
    /// <param name="applicationContext"></param>
    /// <param name="keyGenerator"></param>
    public AuditStoreServices(ICorrelationIdProvider correlationIdProvider, IApplicationContext applicationContext, IKeyGenerator<Guid> keyGenerator, IRepository<AuditLogInfo> auditLogRepo, IRepository<AuditLogActionInfo> auditLogActionRepo, IRepository<AuditEntityInfo> auditEntityRepo, IRepository<AuditEntityPropertyInfo> auditEntityPropertyRepo, IRepository<AuditSqlRawInfo> auditSqlRawRepo, IRepository<AuditSqlRawParameterInfo> auditSqlRawParameterRepo, ILogger<AuditStoreServices> logger)
    {
        _correlationIdProvider = correlationIdProvider;
        _applicationContext = applicationContext;
        _keyGenerator = keyGenerator;
        _auditLogRepo = auditLogRepo;
        _auditLogActionRepo = auditLogActionRepo;
        _auditEntityRepo = auditEntityRepo;
        _auditEntityPropertyRepo = auditEntityPropertyRepo;
        _auditSqlRawRepo = auditSqlRawRepo;
        _auditSqlRawParameterRepo = auditSqlRawParameterRepo;
        _logger = logger;
    }

    public async Task SaveAsync(AuditOperationEntry operationEntry, CancellationToken cancelToken = default)
    {
        var auditLogInfo = BuildOperation(operationEntry);
        
        // _logger.LogDebug(auditLogInfo?.ToJson());

        await _auditLogRepo.InsertAsync(auditLogInfo, cancelToken);
        if (auditLogInfo.EntityEntries.Any())
        {
            await _auditEntityRepo.InsertAsync(auditLogInfo.EntityEntries, cancelToken);
            var changeList = auditLogInfo.EntityEntries.SelectMany(x => x.PropertyEntries);
            if (changeList.Any())
                await _auditEntityPropertyRepo.InsertAsync(changeList, cancelToken);
        }

        if (auditLogInfo.SqlRawEntries.Any())
        {
            await _auditSqlRawRepo.InsertAsync(auditLogInfo.SqlRawEntries, cancelToken);
            var paramList = auditLogInfo.SqlRawEntries.SelectMany(x => x.DbParameters);
            if (paramList.Any())
                await _auditSqlRawParameterRepo.InsertAsync(paramList, cancelToken);
        }
    }

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