using System.Threading.Tasks;

namespace Findx.Aspect;

/// <summary>
///     拦截器接口
/// </summary>
public interface IInterceptor
{
    /// <summary>
    ///     拦截执行
    /// </summary>
    /// <param name="invocation"></param>
    /// <returns></returns>
    Task InterceptAsync(IMethodInvocation invocation);
}