namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     惰性解析辅助：绕开 Findx 全局扩展与 MS DI 扩展的方法签名冲突。
///     <para>仅用于「角色受限」的服务：先由守卫确认角色，再解析依赖，避免纯执行端触发数据库原语构造。</para>
/// </summary>
internal static class LazyServices
{
    /// <summary>
    ///     解析必需服务；未注册时抛出可读异常（正常路径不会走到，守卫已先返回 503）
    /// </summary>
    internal static T Require<T>(IServiceProvider serviceProvider) where T : class
    {
        return serviceProvider.GetService(typeof(T)) as T
               ?? throw new InvalidOperationException(
                   $"服务 {typeof(T).Name} 未注册：本节点未启用调度端（Findx:Scheduler:SchedulerEnabled=false）时不提供调度端能力");
    }
}
