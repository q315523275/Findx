using Castle.DynamicProxy;
using IInterceptor = Findx.Aspect.IInterceptor;

namespace Findx.Castle
{
    public class CastleAsyncDeterminationInterceptor<TInterceptor> : AsyncDeterminationInterceptor
        where TInterceptor : IInterceptor
    {
        public CastleAsyncDeterminationInterceptor(TInterceptor interceptor) : base(
            new CastleAsyncInterceptorAdapter<TInterceptor>(interceptor))
        {
        }
    }
}