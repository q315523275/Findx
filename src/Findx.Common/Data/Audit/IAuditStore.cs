using System.Threading;
using System.Threading.Tasks;

namespace Findx.Data
{
    /// <summary>
    /// 定义Audit数据存储功能
    /// </summary>
    public interface IAuditStore
    {
        /// <summary>
        /// 异步保存实体审计数据
        /// </summary>
        /// <param name="operationEntry">操作审计数据</param>
        /// <param name="cancelToken">异步取消标识</param>
        /// <returns></returns>
        Task SaveAsync(AuditOperationEntry operationEntry, CancellationToken cancelToken = default(CancellationToken));
    }
}

