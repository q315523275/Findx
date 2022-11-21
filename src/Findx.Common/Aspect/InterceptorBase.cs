using System.Threading.Tasks;

namespace Findx.Aspect
{
    /// <summary>
    /// 拦截器基类
    /// </summary>
    public abstract class InterceptorBase : IInterceptor
    {
        /// <summary>
        /// 拦截方法
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        public abstract Task InterceptAsync(IMethodInvocation invocation);
    }
}
