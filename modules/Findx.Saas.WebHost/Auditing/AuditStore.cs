using Findx.Data;
using Findx.DependencyInjection;
using Findx.Extensions;

namespace Findx.Saas.WebHost.Auditing
{
    public class AuditStore : IAuditStore, ISingletonDependency
    {
        private readonly ILogger<AuditStore> _logger;

        public AuditStore(ILogger<AuditStore> logger)
        {
            _logger = logger;
        }

        public Task SaveAsync(AuditOperationEntry operationEntry, CancellationToken cancelToken = default)
        {
            _logger.LogInformation(operationEntry?.ToJson());
            return Task.CompletedTask;
        }
    }
}

