using System.Threading.Tasks;

namespace Findx.Aspect
{
    public interface IInterceptor
    {
        Task InterceptAsync(IMethodInvocation invocation);
    }
}
