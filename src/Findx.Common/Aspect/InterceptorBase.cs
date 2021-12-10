using System.Threading.Tasks;

namespace Findx.Aspect
{
    public abstract class InterceptorBase : IInterceptor
    {
        public abstract Task InterceptAsync(IMethodInvocation invocation);
    }
}
