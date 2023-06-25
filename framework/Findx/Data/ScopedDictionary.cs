using System.Security.Claims;
using System.Threading.Tasks;
using Findx.Security;

namespace Findx.Data;

/// <summary>
///     DI作用域字典(线程安全)
/// </summary>
public class ScopedDictionary : ConcurrentDictionary<string, object>, IAsyncDisposable
{
    /// <summary>
    ///     获取或设置 当前执行的功能
    /// </summary>
    public IFunction Function { get; set; }

    /// <summary>
    ///     获取或设置 对于当前功能有效的角色集合，用于数据权限判断
    /// </summary>
    public IEnumerable<string> DataAuthValidRoleNames { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     获取或设置 当前操作审计
    /// </summary>
    public AuditOperationEntry AuditOperation { get; set; }

    /// <summary>
    ///     获取或设置 当前用户
    /// </summary>
    public ClaimsIdentity Identity { get; set; }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        Function = null;
        DataAuthValidRoleNames = null;
        AuditOperation = null;
        Identity = null;

        foreach (var key in Keys)
        {
            switch (this[key])
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        Clear();
    }
}