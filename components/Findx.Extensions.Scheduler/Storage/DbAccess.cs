using Findx.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Extensions.Scheduler.Storage;

/// <summary>
///     仓储短时作用域访问器。
///     <para>调度组件内的存储实现多为单例，而 IRepository 是 Scoped 生命周期，因此每次操作自建一个作用域。</para>
/// </summary>
internal static class DbAccess
{
    /// <summary>
    ///     在独立作用域内执行一次仓储操作
    /// </summary>
    public static async Task<TResult> UseAsync<TEntity, TResult>(IServiceProvider serviceProvider,
        Func<IRepository<TEntity, long>, Task<TResult>> action, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<long>
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<TEntity, long>>();
        return await action(repo);
    }
}
