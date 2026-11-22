using Microsoft.AspNetCore.Http;

namespace Findx.Extensions.Scheduler.Execution;

/// <summary>
///     机器间接口共享令牌校验：配置 ClusterToken 后，请求必须携带一致的 X-Scheduler-Token 头
/// </summary>
internal static class SchedulerTokenValidator
{
    /// <summary>
    ///     校验请求令牌
    /// </summary>
    /// <param name="context">当前请求上下文</param>
    /// <param name="clusterToken">配置的令牌（空=不校验）</param>
    /// <returns></returns>
    public static bool Validate(HttpContext context, string clusterToken)
    {
        if (string.IsNullOrWhiteSpace(clusterToken)) return true;

        var headerToken = context.Request.Headers["X-Scheduler-Token"].FirstOrDefault();
        return string.Equals(headerToken, clusterToken, StringComparison.Ordinal);
    }
}
